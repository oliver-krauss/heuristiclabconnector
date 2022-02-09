#region License Information
/* HeuristicLab
 * Copyright (C) Heuristic and Evolutionary Algorithms Laboratory (HEAL)
 *
 * This file is part of HeuristicLab.
 *
 * HeuristicLab is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * HeuristicLab is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with HeuristicLab. If not, see <http://www.gnu.org/licenses/>.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using HEAL.Attic;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;

namespace HeuristicLab.TruffleConnector {
  /// <summary>
  ///   Patched copy of HeuristicLab.Encodings.SymbolicExpressionTreeEncoding.SymbolicExpressionGrammarBase.
  /// 
  ///   The default symbolic expression grammar stores symbols and syntactic constraints for symbols.
  ///   Symbols are treated as equivalent if they have the same name and, in case of TruffleSymbols, the same ID.
  ///   Syntactic constraints limit the number of allowed sub-trees for a node with a symbol and which symbols are allowed 
  ///   in the sub-trees of a symbol (can be specified for each sub-tree index separately).
  /// </summary>
  [StorableType("056D263A-9DAF-4C45-A1CE-BF5B38CFA989")]
  public abstract class SymbolicExpressionGrammarBase : NamedItem, ISymbolicExpressionGrammarBase {

    #region properties for separation between implementation and persistence
    private IEnumerable<ISymbol> storableSymbols;
    [Storable(Name = "Symbols")]
    private IEnumerable<ISymbol> StorableSymbols {
      get { return symbols.Values.ToArray(); }
      set { storableSymbols = value; }
    }

    private IEnumerable<KeyValuePair<ISymbol, Tuple<int, int>>> storableSymbolSubtreeCount;
    [Storable(Name = "SymbolSubtreeCount")]
    private IEnumerable<KeyValuePair<ISymbol, Tuple<int, int>>> StorableSymbolSubtreeCount {
      get { return symbolSubtreeCount.Select(x => new KeyValuePair<ISymbol, Tuple<int, int>>(GetSymbol(x.Key), x.Value)).ToArray(); }
      set { storableSymbolSubtreeCount = value; }
    }

    private IEnumerable<KeyValuePair<ISymbol, IEnumerable<ISymbol>>> storableAllowedChildSymbols;
    [Storable(Name = "AllowedChildSymbols")]
    private IEnumerable<KeyValuePair<ISymbol, IEnumerable<ISymbol>>> StorableAllowedChildSymbols {
      get { return allowedChildSymbols.Select(x => new KeyValuePair<ISymbol, IEnumerable<ISymbol>>(GetSymbol(x.Key), x.Value.Select(GetSymbol).ToArray())).ToArray(); }
      set { storableAllowedChildSymbols = value; }
    }

    private IEnumerable<KeyValuePair<Tuple<ISymbol, int>, IEnumerable<ISymbol>>> storableAllowedChildSymbolsPerIndex;
    [Storable(Name = "AllowedChildSymbolsPerIndex")]
    private IEnumerable<KeyValuePair<Tuple<ISymbol, int>, IEnumerable<ISymbol>>> StorableAllowedChildSymbolsPerIndex {
      get { return allowedChildSymbolsPerIndex.Select(x => new KeyValuePair<Tuple<ISymbol, int>, IEnumerable<ISymbol>>(Tuple.Create(GetSymbol(x.Key.Item1), x.Key.Item2), x.Value.Select(GetSymbol).ToArray())).ToArray(); }
      set { storableAllowedChildSymbolsPerIndex = value; }
    }
    #endregion

    private bool suppressEvents;
    protected readonly Dictionary<string, ISymbol> symbols;
    protected readonly Dictionary<string, Tuple<int, int>> symbolSubtreeCount;
    protected readonly Dictionary<string, List<string>> allowedChildSymbols;
    protected readonly Dictionary<Tuple<string, int>, List<string>> allowedChildSymbolsPerIndex;

    public override bool CanChangeName {
      get { return false; }
    }
    public override bool CanChangeDescription {
      get { return false; }
    }

    [StorableConstructor]
    protected SymbolicExpressionGrammarBase(StorableConstructorFlag _) : base(_) {

      symbols = new Dictionary<string, ISymbol>();
      symbolSubtreeCount = new Dictionary<string, Tuple<int, int>>();
      allowedChildSymbols = new Dictionary<string, List<string>>();
      allowedChildSymbolsPerIndex = new Dictionary<Tuple<string, int>, List<string>>();

      suppressEvents = false;
    }

    [StorableHook(HookType.AfterDeserialization)]
    private void AfterDeserialization() {
      foreach (var s in storableSymbols) symbols.Add(GetKey(s), s);
      foreach (var pair in storableSymbolSubtreeCount) symbolSubtreeCount.Add(GetKey(pair.Key), pair.Value);
      foreach (var pair in storableAllowedChildSymbols) allowedChildSymbols.Add(GetKey(pair.Key), pair.Value.Select(y => GetKey(y)).ToList());
      foreach (var pair in storableAllowedChildSymbolsPerIndex)
        allowedChildSymbolsPerIndex.Add(Tuple.Create(GetKey(pair.Key.Item1), pair.Key.Item2), pair.Value.Select(y => GetKey(y)).ToList());

      storableSymbols = null;
      storableSymbolSubtreeCount = null;
      storableAllowedChildSymbols = null;
      storableAllowedChildSymbolsPerIndex = null;
    }

    protected SymbolicExpressionGrammarBase(SymbolicExpressionGrammarBase original, Cloner cloner)
      : base(original, cloner) {

      symbols = original.symbols.ToDictionary(x => x.Key, y => cloner.Clone(y.Value));
      symbolSubtreeCount = new Dictionary<string, Tuple<int, int>>(original.symbolSubtreeCount);

      allowedChildSymbols = new Dictionary<string, List<string>>();
      foreach (var element in original.allowedChildSymbols)
        allowedChildSymbols.Add(element.Key, new List<string>(element.Value));

      allowedChildSymbolsPerIndex = new Dictionary<Tuple<string, int>, List<string>>();
      foreach (var element in original.allowedChildSymbolsPerIndex)
        allowedChildSymbolsPerIndex.Add(element.Key, new List<string>(element.Value));

      suppressEvents = false;
    }

    protected SymbolicExpressionGrammarBase(string name, string description)
      : base(name, description) {
      symbols = new Dictionary<string, ISymbol>();
      symbolSubtreeCount = new Dictionary<string, Tuple<int, int>>();
      allowedChildSymbols = new Dictionary<string, List<string>>();
      allowedChildSymbolsPerIndex = new Dictionary<Tuple<string, int>, List<string>>();

      suppressEvents = false;
    }

    #region protected grammar manipulation methods
    public virtual void AddSymbol(ISymbol symbol) {
      if (ContainsSymbol(symbol)) throw new ArgumentException("Symbol " + symbol + " is already defined.");
      foreach (var s in symbol.Flatten()) {
        var sName = GetKey(s);
        symbols.Add(sName, s);
        int maxSubTreeCount = Math.Min(s.MinimumArity + 1, s.MaximumArity);
        symbolSubtreeCount.Add(sName, Tuple.Create(s.MinimumArity, maxSubTreeCount));
      }
      ClearCaches();
    }

    public virtual void RemoveSymbol(ISymbol symbol) {
      foreach (var s in symbol.Flatten()) {
        var sName = GetKey(s);
        symbols.Remove(sName);
        allowedChildSymbols.Remove(sName);
        for (int i = 0; i < GetMaximumSubtreeCount(s); i++)
          allowedChildSymbolsPerIndex.Remove(Tuple.Create(sName, i));
        symbolSubtreeCount.Remove(sName);

        foreach (var parent in Symbols) {
          var parentName = GetKey(parent);
          List<string> allowedChilds;
          if (allowedChildSymbols.TryGetValue(parentName, out allowedChilds))
            allowedChilds.Remove(sName);

          for (int i = 0; i < GetMaximumSubtreeCount(parent); i++) {
            if (allowedChildSymbolsPerIndex.TryGetValue(Tuple.Create(parentName, i), out allowedChilds))
              allowedChilds.Remove(sName);
          }
        }
        suppressEvents = true;
        foreach (var groupSymbol in Symbols.OfType<GroupSymbol>())
          groupSymbol.SymbolsCollection.Remove(symbol);
        suppressEvents = false;
      }
      ClearCaches();
    }

    public virtual ISymbol GetSymbol(string symbolName) {
      ISymbol symbol;
      if (symbols.TryGetValue(symbolName, out symbol)) return symbol;
      return null;
    }

    public virtual void AddAllowedChildSymbol(ISymbol parent, ISymbol child) {
      bool changed = false;

      foreach (ISymbol p in parent.Flatten().Where(p => !(p is GroupSymbol)))
        changed |= AddAllowedChildSymbolToDictionaries(p, child);

      if (changed) {
        ClearCaches();
        OnChanged();
      }
    }

    private bool AddAllowedChildSymbolToDictionaries(ISymbol parent, ISymbol child) {
      var parentName = GetKey(parent);
      var childName = GetKey(child);

      List<string> childSymbols;
      if (!allowedChildSymbols.TryGetValue(parentName, out childSymbols)) {
        childSymbols = new List<string>();
        allowedChildSymbols.Add(parentName, childSymbols);
      }
      if (childSymbols.Contains(childName)) return false;

      suppressEvents = true;
      for (int argumentIndex = 0; argumentIndex < GetMaximumSubtreeCount(parent); argumentIndex++)
        RemoveAllowedChildSymbol(parent, child, argumentIndex);
      suppressEvents = false;

      childSymbols.Add(childName);
      return true;
    }

    public virtual void AddAllowedChildSymbol(ISymbol parent, ISymbol child, int argumentIndex) {
      bool changed = false;

      foreach (ISymbol p in parent.Flatten().Where(p => !(p is GroupSymbol)))
        changed |= AddAllowedChildSymbolToDictionaries(p, child, argumentIndex);

      if (changed) {
        ClearCaches();
        OnChanged();
      }
    }


    private bool AddAllowedChildSymbolToDictionaries(ISymbol parent, ISymbol child, int argumentIndex) {
      var parentName = GetKey(parent);
      var childName = GetKey(child);

      List<string> childSymbols;
      if (!allowedChildSymbols.TryGetValue(parentName, out childSymbols)) {
        childSymbols = new List<string>();
        allowedChildSymbols.Add(parentName, childSymbols);
      }
      if (childSymbols.Contains(childName)) return false;


      var key = Tuple.Create(parentName, argumentIndex);
      if (!allowedChildSymbolsPerIndex.TryGetValue(key, out childSymbols)) {
        childSymbols = new List<string>();
        allowedChildSymbolsPerIndex.Add(key, childSymbols);
      }

      if (childSymbols.Contains(childName)) return false;

      childSymbols.Add(childName);
      return true;
    }

    public virtual void RemoveAllowedChildSymbol(ISymbol parent, ISymbol child) {
      var parentName = GetKey(parent);
      var childName = GetKey(child);

      bool changed = false;
      List<string> childSymbols;
      if (allowedChildSymbols.TryGetValue(childName, out childSymbols)) {
        changed |= childSymbols.Remove(childName);
      }

      for (int argumentIndex = 0; argumentIndex < GetMaximumSubtreeCount(parent); argumentIndex++) {
        var key = Tuple.Create(parentName, argumentIndex);
        if (allowedChildSymbolsPerIndex.TryGetValue(key, out childSymbols))
          changed |= childSymbols.Remove(childName);
      }

      if (changed) {
        ClearCaches();
        OnChanged();
      }
    }

    public virtual void RemoveAllowedChildSymbol(ISymbol parent, ISymbol child, int argumentIndex) {
      bool changed = false;
      var parentName = GetKey(parent);
      var childName = GetKey(child);

      suppressEvents = true;
      List<string> childSymbols;
      if (allowedChildSymbols.TryGetValue(parentName, out childSymbols)) {
        if (childSymbols.Remove(childName)) {
          for (int i = 0; i < GetMaximumSubtreeCount(parent); i++) {
            if (i != argumentIndex) AddAllowedChildSymbol(parent, child, i);
          }
          changed = true;
        }
      }
      suppressEvents = false;

      var key = Tuple.Create(parentName, argumentIndex);
      if (allowedChildSymbolsPerIndex.TryGetValue(key, out childSymbols))
        changed |= childSymbols.Remove(childName);

      if (changed) {
        ClearCaches();
        OnChanged();
      }
    }

    public virtual void SetSubtreeCount(ISymbol symbol, int minimumSubtreeCount, int maximumSubtreeCount) {
      var symbols = symbol.Flatten().Where(s => !(s is GroupSymbol));
      if (symbols.Any(s => s.MinimumArity > minimumSubtreeCount)) throw new ArgumentException("Invalid minimum subtree count " + minimumSubtreeCount + " for " + symbol);
      if (symbols.Any(s => s.MaximumArity < maximumSubtreeCount)) throw new ArgumentException("Invalid maximum subtree count " + maximumSubtreeCount + " for " + symbol);

      foreach (ISymbol s in symbols)
        SetSubTreeCountInDictionaries(s, minimumSubtreeCount, maximumSubtreeCount);

      ClearCaches();
      OnChanged();
    }

    private void SetSubTreeCountInDictionaries(ISymbol symbol, int minimumSubtreeCount, int maximumSubtreeCount) {
      var symbolName = GetKey(symbol);

      for (int i = maximumSubtreeCount; i < GetMaximumSubtreeCount(symbol); i++) {
        var key = Tuple.Create(symbolName, i);
        allowedChildSymbolsPerIndex.Remove(key);
      }

      symbolSubtreeCount[symbolName] = Tuple.Create(minimumSubtreeCount, maximumSubtreeCount);
    }
    #endregion

    public virtual IEnumerable<ISymbol> Symbols {
      get { return symbols.Values; }
    }
    public virtual IEnumerable<ISymbol> AllowedSymbols {
      get { return Symbols.Where(s => s.Enabled); }
    }
    public virtual bool ContainsSymbol(ISymbol symbol) {
      return symbols.ContainsKey(GetKey(symbol));
    }

    private readonly Dictionary<Tuple<string, string>, bool> cachedIsAllowedChildSymbol = new Dictionary<Tuple<string, string>, bool>();
    public virtual bool IsAllowedChildSymbol(ISymbol parent, ISymbol child) {
      if (allowedChildSymbols.Count == 0) return false;
      if (!child.Enabled) return false;

      var parentName = GetKey(parent);
      var childName = GetKey(child);

      bool result;
      var key = Tuple.Create(parentName, childName);
      if (cachedIsAllowedChildSymbol.TryGetValue(key, out result)) return result;

      // value has to be calculated and cached make sure this is done in only one thread
      lock (cachedIsAllowedChildSymbol) {
        // in case the value has been calculated on another thread in the meanwhile
        if (cachedIsAllowedChildSymbol.TryGetValue(key, out result)) return result;

        List<string> temp;
        if (allowedChildSymbols.TryGetValue(parentName, out temp)) {
          for (int i = 0; i < temp.Count; i++) {
            var symbol = GetSymbol(temp[i]);
            foreach (var s in symbol.Flatten())
              if (GetKey(s) == childName) {
                cachedIsAllowedChildSymbol.Add(key, true);
                return true;
              }
          }
        }
        cachedIsAllowedChildSymbol.Add(key, false);
        return false;
      }
    }

    private readonly Dictionary<Tuple<string, string, int>, bool> cachedIsAllowedChildSymbolIndex = new Dictionary<Tuple<string, string, int>, bool>();
    public virtual bool IsAllowedChildSymbol(ISymbol parent, ISymbol child, int argumentIndex) {
      if (!child.Enabled) return false;
      if (IsAllowedChildSymbol(parent, child)) return true;
      if (allowedChildSymbolsPerIndex.Count == 0) return false;

      var parentName = GetKey(parent);
      var childName = GetKey(child);

      bool result;
      var key = Tuple.Create(parentName, childName, argumentIndex);
      if (cachedIsAllowedChildSymbolIndex.TryGetValue(key, out result)) return result;

      // value has to be calculated and cached make sure this is done in only one thread
      lock (cachedIsAllowedChildSymbolIndex) {
        // in case the value has been calculated on another thread in the meanwhile
        if (cachedIsAllowedChildSymbolIndex.TryGetValue(key, out result)) return result;

        List<string> temp;
        if (allowedChildSymbolsPerIndex.TryGetValue(Tuple.Create(parentName, argumentIndex), out temp)) {
          for (int i = 0; i < temp.Count; i++) {
            var symbol = GetSymbol(temp[i]);
            foreach (var s in symbol.Flatten())
              if (GetKey(s) == childName) {
                cachedIsAllowedChildSymbolIndex.Add(key, true);
                return true;
              }
          }
        }
        cachedIsAllowedChildSymbolIndex.Add(key, false);
        return false;
      }
    }

    public IEnumerable<ISymbol> GetAllowedChildSymbols(ISymbol parent) {
      foreach (ISymbol child in AllowedSymbols) {
        if (IsAllowedChildSymbol(parent, child)) yield return child;
      }
    }

    public IEnumerable<ISymbol> GetAllowedChildSymbols(ISymbol parent, int argumentIndex) {
      foreach (ISymbol child in AllowedSymbols) {
        if (IsAllowedChildSymbol(parent, child, argumentIndex)) yield return child;
      }
    }

    public virtual int GetMinimumSubtreeCount(ISymbol symbol) {
      return symbolSubtreeCount[GetKey(symbol)].Item1;
    }
    public virtual int GetMaximumSubtreeCount(ISymbol symbol) {
      return symbolSubtreeCount[GetKey(symbol)].Item2;
    }

    protected void ClearCaches() {
      cachedMinExpressionLength.Clear();
      cachedMaxExpressionLength.Clear();
      cachedMinExpressionDepth.Clear();
      cachedMaxExpressionDepth.Clear();

      cachedIsAllowedChildSymbol.Clear();
      cachedIsAllowedChildSymbolIndex.Clear();
    }

    private readonly Dictionary<string, int> cachedMinExpressionLength = new Dictionary<string, int>();
    public int GetMinimumExpressionLength(ISymbol symbol) {
      var symbolName = GetKey(symbol);

      int res;
      if (cachedMinExpressionLength.TryGetValue(symbolName, out res))
        return res;

      // value has to be calculated and cached make sure this is done in only one thread
      lock (cachedMinExpressionLength) {
        // in case the value has been calculated on another thread in the meanwhile
        if (cachedMinExpressionLength.TryGetValue(symbolName, out res)) return res;

        GrammarUtils.CalculateMinimumExpressionLengths(this, cachedMinExpressionLength);
        return cachedMinExpressionLength[symbolName];
      }
    }


    private readonly Dictionary<Tuple<string, int>, int> cachedMaxExpressionLength = new Dictionary<Tuple<string, int>, int>();
    public int GetMaximumExpressionLength(ISymbol symbol, int maxDepth) {
      int temp;
      var key = Tuple.Create(GetKey(symbol), maxDepth);
      if (cachedMaxExpressionLength.TryGetValue(key, out temp)) return temp;
      // value has to be calculated and cached make sure this is done in only one thread
      lock (cachedMaxExpressionLength) {
        // in case the value has been calculated on another thread in the meanwhile
        if (cachedMaxExpressionLength.TryGetValue(key, out temp)) return temp;

        cachedMaxExpressionLength[key] = int.MaxValue; // prevent infinite recursion
        long sumOfMaxTrees = 1 + (from argIndex in Enumerable.Range(0, GetMaximumSubtreeCount(symbol))
                                  let maxForSlot = (long)(from s in GetAllowedChildSymbols(symbol, argIndex)
                                                          where s.InitialFrequency > 0.0
                                                          where GetMinimumExpressionDepth(s) < maxDepth
                                                          select GetMaximumExpressionLength(s, maxDepth - 1)).DefaultIfEmpty(0).Max()
                                  select maxForSlot).DefaultIfEmpty(0).Sum();
        cachedMaxExpressionLength[key] = (int)Math.Min(sumOfMaxTrees, int.MaxValue);
        return cachedMaxExpressionLength[key];
      }
    }

    private readonly Dictionary<string, int> cachedMinExpressionDepth = new Dictionary<string, int>();
    public int GetMinimumExpressionDepth(ISymbol symbol) {
      var symbolName = GetKey(symbol);

      int res;
      if (cachedMinExpressionDepth.TryGetValue(symbolName, out res))
        return res;

      // value has to be calculated and cached make sure this is done in only one thread
      lock (cachedMinExpressionDepth) {
        // in case the value has been calculated on another thread in the meanwhile
        if (cachedMinExpressionDepth.TryGetValue(symbolName, out res)) return res;

        GrammarUtils.CalculateMinimumExpressionDepth(this, cachedMinExpressionDepth);
        return cachedMinExpressionDepth[symbolName];
      }
    }

    private readonly Dictionary<string, int> cachedMaxExpressionDepth = new Dictionary<string, int>();
    public int GetMaximumExpressionDepth(ISymbol symbol) {
      var symbolName = GetKey(symbol);

      int temp;
      if (cachedMaxExpressionDepth.TryGetValue(symbolName, out temp)) return temp;
      // value has to be calculated and cached make sure this is done in only one thread
      lock (cachedMaxExpressionDepth) {
        // in case the value has been calculated on another thread in the meanwhile
        if (cachedMaxExpressionDepth.TryGetValue(symbolName, out temp)) return temp;

        cachedMaxExpressionDepth[symbolName] = int.MaxValue;
        long maxDepth = 1 + (from argIndex in Enumerable.Range(0, GetMaximumSubtreeCount(symbol))
                             let maxForSlot = (long)(from s in GetAllowedChildSymbols(symbol, argIndex)
                                                     where s.InitialFrequency > 0.0
                                                     select GetMaximumExpressionDepth(s)).DefaultIfEmpty(0).Max()
                             select maxForSlot).DefaultIfEmpty(0).Max();
        cachedMaxExpressionDepth[symbolName] = (int)Math.Min(maxDepth, int.MaxValue);
        return cachedMaxExpressionDepth[symbolName];
      }
    }

    public event EventHandler Changed;
    protected virtual void OnChanged() {
      if (suppressEvents) return;
      var handler = Changed;
      if (handler != null) handler(this, EventArgs.Empty);
    }

    /// <summary>
    ///   Gets a string representation of a symbol for use as a dictionary key.
    /// </summary>
    /// <param name="symbol">The symbol to get a string representation for.</param>
    /// <returns>The string representation of the symbol.</returns>
    protected string GetKey(ISymbol symbol) {
      if (symbol is TruffleSymbol truffleSymbol)
        return $"{truffleSymbol.Id}_{truffleSymbol.Name}"; // TruffleSymbols may have the same name and can be distinguished by their ID.
      return symbol.Name;
    }
  }
}
