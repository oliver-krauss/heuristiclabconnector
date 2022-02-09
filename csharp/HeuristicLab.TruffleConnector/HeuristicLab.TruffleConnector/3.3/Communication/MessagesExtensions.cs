using System;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using HeuristicLab.TruffleConnector.Messages;
using HeuristicLab.TruffleConnector.Properties;
using NetMQ;
using NetMQ.Sockets;

namespace HeuristicLab.TruffleConnector {
  /// <summary>
  ///   Contains some extension methods for NetMQ and Protobuf messages.
  /// </summary>
  public static class MessagesExtensions {
    /// <summary>
    ///   Wraps a Protobuf message into a Wrapper message, sends it using a RequestSocket,
    ///   receives the result (another Wrapper Protobuf message) and unpacks it.
    /// </summary>
    /// <typeparam name="T">The type of the Protobuf message that should be received.</typeparam>
    /// <param name="request">The message that should be sent.</param>
    /// <param name="algorithmRunId">
    ///   ID of the algorithm run. Send with all messages associated with an algorithm run
    ///   (beginning with StartAlgorithm, ending with StopAlgorithm).
    /// </param>
    /// <param name="workerConfig">
    ///   Specifies the configuration that must be supported by the worker.
    ///   Only send with StartAlgorithm message.
    /// </param>
    /// <returns>The unpacked response message.</returns>
    public static T Send<T>(this IMessage request, string algorithmRunId = null, WorkerConfig workerConfig = null)
      where T : IMessage, new() {
      var wrapper = new Wrapper {Message = Any.Pack(request)};

      var requestMessage = new NetMQMessage();
      if (!string.IsNullOrEmpty(algorithmRunId)) {
        requestMessage.Append(algorithmRunId);

        if (workerConfig != null) {
          requestMessage.Append(workerConfig.LanguageId.ToString());
        }
      }

      requestMessage.Append(wrapper.ToByteArray());
      var responseMessage = requestMessage.Send();

      wrapper = Wrapper.Parser.ParseFrom(responseMessage.Pop().ToByteArray());
      var response = wrapper.Message.Unpack<T>();
      return response;
    }

    /// <summary>
    ///   Sends a NetMQMessage and waits for a response.
    ///   Implements the client-side of the Paranoid Pirate pattern (http://zguide.zeromq.org/cs:lpclient).
    /// </summary>
    /// <param name="request">Message to send.</param>
    /// <returns>Received message.</returns>
    public static NetMQMessage Send(this NetMQMessage request) {
      RequestSocket CreateRequester() {
        var socket = new RequestSocket($">{Settings.Default.BrokerFrontend}");
        socket.Options.Linger = TimeSpan.FromMilliseconds(1);
        return socket;
      }

      var retriesLeft = Settings.Default.Retries;
      var timeout = TimeSpan.FromMilliseconds(Settings.Default.Timeout);

      RequestSocket requester;
      NetMQMessage response = null;

      using (requester = CreateRequester()) {
        while (response == null && retriesLeft > 0) {
          // We send a request, then we work to get a reply
          requester.SendMultipartMessage(request);

          // Here we process a server reply and exit our loop if the reply is valid.
          // If we didn't receive a reply, we close the client socket and resend the request.
          // We try a number of times before finally abandoning
          if (requester.TryReceiveMultipartMessage(timeout, ref response)) {
            // Received a response
            break;
          }

          if (--retriesLeft == 0) {
            // Server seems to be offline, abandoning
            break;
          }

          // Old socket is confused; close it and open a new one
          // Request is sent again, on new socket
          requester = CreateRequester();
        }
      }

      if (response == null)
        throw new TimeoutException("Receiving a response from the broker timed out.");
      return response;
    }

    /// <summary>
    ///   Parses a TreeNode Protobuf message to its SymbolicExpressionTreeNode representation.
    /// </summary>
    /// <param name="treeNode">The TreeNode message to be parsed.</param>
    /// <param name="grammar">The grammar that contains the definitions of the symbols.</param>
    /// <returns>The SymbolicExpressionTreeNode representation.</returns>
    public static SymbolicExpressionTreeNode ToSymbolicExpressionTreeNode(this TreeNode treeNode,
      TruffleGrammar grammar) {
      var symbol = grammar.GetTruffleSymbol(treeNode.SymbolId);
      var node = new SymbolicExpressionTreeNode(symbol);

      foreach (var child in treeNode.Children) {
        node.AddSubtree(ToSymbolicExpressionTreeNode(child, grammar));
      }

      return node;
    }

    /// <summary>
    ///   Parses a TreeNode Protobuf message to its SymbolicExpressionTree representation.
    /// </summary>
    /// <param name="response">The root node of the tree to be parsed.</param>
    /// <param name="solutionId">The solution ID of the tree to be parsed.</param>
    /// <param name="grammar">The grammar that contains the definitions of the symbols.</param>
    /// <returns>The SymbolicExpressionTree representation.</returns>
    public static TruffleTree ToSymbolicExpressionTree(this TreeNode root, long solutionId, TruffleGrammar grammar) {
      var tree = new TruffleTree(solutionId, root.ToSymbolicExpressionTreeNode(grammar));
      return tree;
    }
  }
}
