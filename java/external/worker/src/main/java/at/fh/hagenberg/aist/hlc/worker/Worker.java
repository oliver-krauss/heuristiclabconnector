/*
 *
 *  * Copyright (c) 2022 the original author or authors.
 *  * DO NOT ALTER OR REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
 *  *
 *  * This Source Code Form is subject to the terms of the Mozilla Public
 *  * License, v. 2.0. If a copy of the MPL was not distributed with this
 *  * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 */

package at.fh.hagenberg.aist.hlc.worker;

import at.fh.hagenberg.aist.hlc.core.*;
import at.fh.hagenberg.aist.hlc.core.messages.*;
import at.fh.hagenberg.aist.seshat.Logger;
import com.google.protobuf.Any;
import com.google.protobuf.Empty;
import com.google.protobuf.InvalidProtocolBufferException;
import com.google.protobuf.Message;
import org.zeromq.*;
import org.zeromq.ZMQ.Poller;
import org.zeromq.ZMQ.Socket;

import java.util.List;
import java.util.UUID;

/**
 * Queue that implements the Paranoid Pirate Pattern, based on http://zguide.zeromq.org/java:ppqueue.
 *
 * @author Daniel Dorfmeister on 2019-06-19
 */
public class Worker {
    private static boolean logMessages = true;
    private static boolean logResponses = false;

    private ExternalOptimizationWorker worker;
    private ConfigurationManagementWorker configWorker;
    private Logger logger;

    private String brokerBackend;
    private List<Long> supportedLanguages;
    private int intervalMax;
    private int intervalInit;
    private int heartbeatInterval;
    private int heartbeatLiveness;

    private void logMessage(Message message) {
        if (!logMessages)
            return;

        logger.info("====== " + message.getClass().getSimpleName() + " ======");
        logger.info(message.toString());
    }

    /**
     * Helper function that returns a new configured socket connected to the Paranoid Pirate queue.
     *
     * @param ctx:      ZeroMQ context
     * @param endpoint: Connect to this endpoint.
     * @param supportedLanguages: IDs of all languages supported by the worker.
     * @return The configured and connected socket.
     */
    private Socket createSocket(ZContext ctx, String endpoint, List<Long> supportedLanguages) {
        UUID uuid = UUID.randomUUID();
        logger.info("worker ID: " + uuid);

        //  Configure socket
        Socket socket = ctx.createSocket(ZMQ.DEALER);
        socket.setIdentity(UuidHelper.getBytesFromUUID(uuid));
        socket.connect(endpoint);

        //  Tell queue we're ready for work
        logger.info("worker ready");
        ZFrame frame = new ZFrame(ParanoidPirateProtocolConstants.PPP_READY);
        frame.send(socket, 0);
        frame.destroy();

        WorkerConfiguration config = WorkerConfiguration.newBuilder()
                .addAllSupportedLanguages(supportedLanguages)
                .setIsConfigWorker(configWorker != null)
                .build();
        frame = new ZFrame(config.toByteArray());
        frame.send(socket, 0);
        frame.destroy();

        return socket;
    }

    /**
     * Implements the worker side of the Paranoid Pirate Protocol (PPP).
     */
    public void work() {
        try (ZContext ctx = new ZContext()) {
            Socket worker = createSocket(ctx, brokerBackend, supportedLanguages);

            Poller poller = ctx.createPoller(1);
            poller.register(worker, Poller.POLLIN);

            //  If liveness hits zero, queue is considered disconnected
            int liveness = heartbeatLiveness;
            int interval = intervalInit;

            //  Send out heartbeats at regular intervals
            long heartbeatAt = System.currentTimeMillis() + heartbeatInterval;

            while (true) {
                int rc = poller.poll(heartbeatInterval);
                if (rc == -1)
                    break; //  Interrupted

                if (poller.pollin(0)) {
                    ZMsg msg = ZMsg.recvMsg(worker);
                    if (msg == null)
                        break; //  Interrupted

                    if (msg.size() >= 3) { // frames: ZMQ ID, empty, [algorithm run ID], message
                        ZFrame request = msg.removeLast();

                        if (msg.size() == 3) { // frames: ZMQ ID, empty, algorithm run ID
                            String algorithmRunId = msg.removeLast().toString();
                            msg.add(processRequest(request, algorithmRunId));
                        } else { // frames: ZMQ ID, empty
                            msg.add(processRequest(request));
                        }

                        logger.info("normal reply");
                        msg.send(worker);
                        liveness = heartbeatLiveness;
                    } else if (msg.size() == 1) { // frames: signal (HEARTBEAT)
                        //  When we get a heartbeat message from the broker, it means the broker was (recently) alive,
                        //  so reset our liveness indicator:
                        ZFrame frame = msg.getFirst();
                        String frameData = new String(frame.getData(), ZMQ.CHARSET);

                        if (ParanoidPirateProtocolConstants.PPP_HEARTBEAT.equals(frameData)) {
                            liveness = heartbeatLiveness;
                        } else {
                            logger.error("invalid message: " + msg);
                        }

                        msg.destroy();
                    } else {
                        logger.error("invalid message: " + msg);
                    }

                    interval = intervalInit;
                } else if (--liveness == 0) {
                    //  If the broker hasn't sent us heartbeats in a while,
                    //  destroy the socket and reconnect. This is the simplest
                    //  most brutal way of discarding any messages we might have
                    //  sent in the meantime.
                    logger.warn("heartbeat failure, can't reach broker");
                    logger.warn(String.format("reconnecting in %d msec", interval));

                    try {
                        Thread.sleep(interval);
                    } catch (InterruptedException e) {
                        e.printStackTrace();
                    }

                    if (interval < intervalMax) {
                        interval *= 2;
                    }

                    ctx.destroySocket(worker);
                    poller.unregister(worker);
                    worker = createSocket(ctx, brokerBackend, supportedLanguages);
                    poller.register(worker, Poller.POLLIN);
                    liveness = heartbeatLiveness;
                }

                //  Send heartbeat to queue if it's time
                if (System.currentTimeMillis() > heartbeatAt) {
                    long now = System.currentTimeMillis();
                    heartbeatAt = now + heartbeatInterval;
                    logger.info("worker heartbeat");
                    ZFrame frame = new ZFrame(ParanoidPirateProtocolConstants.PPP_HEARTBEAT);
                    frame.send(worker, 0);
                }
            }
        } catch (InvalidProtocolBufferException ex) {
            logger.error(ex);
        }
    }

    /**
     * Processes messages from HeuristicLab.TruffleConnector.
     *
     * @param request: The frame to process.
     * @return The frame to respond.
     * @throws InvalidProtocolBufferException
     */
    private ZFrame processRequest(ZFrame request) throws InvalidProtocolBufferException {
        return processRequest(request, null);
    }

    /**
     * Processes messages from HeuristicLab.TruffleConnector.
     *
     * @param request:        The frame to process.
     * @param algorithmRunId: The ID of the algorithm run the request is associated with.
     * @return The frame to respond.
     * @throws InvalidProtocolBufferException
     */
    private ZFrame processRequest(ZFrame request, String algorithmRunId) throws InvalidProtocolBufferException {
        Any any = Wrapper.parseFrom(request.getData()).getMessage();
        Message response = processAnyMessage(any, algorithmRunId);
        Wrapper wrapper = Wrapper.newBuilder().setMessage(Any.pack(response)).build();
        return new ZFrame(wrapper.toByteArray());
    }

    private Message processAnyMessage(Any any, String algorithmRunId) throws InvalidProtocolBufferException {
        Message response = Empty.getDefaultInstance();

        if (any.is(ConfigurationRequest.class)) {
            ConfigurationRequest request = any.unpack(ConfigurationRequest.class);
            logMessage(request);
            if (configWorker != null) {
                response = configWorker.getConfiguration(request);
            } else {
                logger.warn("This worker does not support configuration requests.");
            }
        } else if (any.is(StartAlgorithmRequest.class)) {
            StartAlgorithmRequest request = any.unpack(StartAlgorithmRequest.class);
            logMessage(request);
            response = worker.configure(algorithmRunId, request);
        } else if (any.is(StopAlgorithmRequest.class)) {
            StopAlgorithmRequest request = any.unpack(StopAlgorithmRequest.class);
            logMessage(request);
            worker.shutdown(algorithmRunId, request);
        } else { // operators
            String typeUrl = any.getTypeUrl();
            String className = at.fh.hagenberg.aist.hlc.core.messages.StartAlgorithm.class.getPackage().getName() +
                    typeUrl.substring(typeUrl.lastIndexOf('.'));

            try {
                Class clazz = Class.forName(className);
                Message message = any.unpack(clazz);
                logMessage(message);
                response = worker.operate(algorithmRunId, message);
            } catch (ClassNotFoundException e) {
                throw new UnsupportedOperationException("Message of type " + typeUrl + " could not be processed.");
            }
        }

        if (logResponses) {
            logMessage(response);
        }

        return response;
    }

    public List<Long> getSupportedLanguages() {
        return supportedLanguages;
    }

    public void setSupportedLanguages(List<Long> supportedLanguages) {
        this.supportedLanguages = supportedLanguages;
    }

    public String getBrokerBackend() {
        return brokerBackend;
    }

    public void setBrokerBackend(String brokerBackend) {
        this.brokerBackend = brokerBackend;
    }

    public Integer getIntervalMax() {
        return intervalMax;
    }

    public void setIntervalMax(Integer intervalMax) {
        this.intervalMax = intervalMax;
    }

    public Integer getIntervalInit() {
        return intervalInit;
    }

    public void setIntervalInit(Integer intervalInit) {
        this.intervalInit = intervalInit;
    }

    public Integer getHeartbeatInterval() {
        return heartbeatInterval;
    }

    public void setHeartbeatInterval(Integer heartbeatInterval) {
        this.heartbeatInterval = heartbeatInterval;
    }

    public Integer getHeartbeatLiveness() {
        return heartbeatLiveness;
    }

    public void setHeartbeatLiveness(Integer heartbeatLiveness) {
        this.heartbeatLiveness = heartbeatLiveness;
    }

    public ExternalOptimizationWorker getWorker() {
        return worker;
    }

    public void setWorker(ExternalOptimizationWorker worker) {
        this.worker = worker;
    }

    public ConfigurationManagementWorker getConfigWorker() {
        return configWorker;
    }

    public void setConfigWorker(ConfigurationManagementWorker configWorker) {
        this.configWorker = configWorker;
    }

    public Logger getLogger() {
        return logger;
    }

    public void setLogger(Logger logger) {
        this.logger = logger;
    }
}
