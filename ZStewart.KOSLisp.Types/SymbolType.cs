using System;
using System.Collections.Generic;
using System.Linq;

using ZStewart.KOSLisp.Types.Attributes;

namespace ZStewart.KOSLisp.Types {
  public class SymbolType : LispObject {
    #region Static Type Setup
    private static LispType _symbol;
    public static LispType Symbol {
      get {
        if (_symbol != null) return _symbol;

        _symbol = new LispType {
          __name__ = "symbol",
          _instance_type = typeof(SymbolType),
        };
        _symbol.__class__ = LispType.Type;
        _symbol.__bases__ = IConsType.ToLispTuple(LispObject.Object);
        _symbol.__mro__ = IConsType.ToLispTuple(Symbol, LispObject.Object);
        _symbol = LispType.ConfigureType(_symbol);
        // TODO(zstewar1): Not sure how to handle errors in "static" setup.
        if (_symbol == null) throw new InvalidOperationException();

        return _symbol;
      }
    }

    [BuiltinFunction(Name = "--repr--")]
    private static LispObject ToRepr([PositionalArgument] SymbolType obj) {
      return StringType.Create(obj.Identifier);
    }
    // TODO(zstewar1): In language instantiation stuff.
    #endregion

    #region Static Helpers
    /// <summary>
    /// String Comparer used for comparing symbols.
    /// </summary>
    private static StringComparer comparer {
      get { return StringComparer.InvariantCultureIgnoreCase; }
    }

    public static SymbolType Create(string identifier) {
      SymbolType val;
      if (UniqueSymbolDictionary.TryGetValue(identifier, out val))
        return val;

      // Ensure that special symbol subtypes always construct the correct instance.
      if (comparer.Compare("nil", identifier) == 0) return NilType.Nil;
      if (comparer.Compare("t", identifier) == 0) return BoolType.T;
      if (comparer.Compare("f", identifier) == 0) return BoolType.F;

      // TODO(zstewar1): Other special symbol subtype cases here. (Initial setup can be
      // here. If it leads to duplicated logic when __new__ is implemented, we may be able
      // to simplify and just __call__ the type.

      return new SymbolType(identifier) {
        __class__ = Symbol,
      };
    }

    public static bool IsSelfEvaluating(SymbolType symbol) {
      if (symbol == NilType.Nil || symbol == BoolType.T || symbol == BoolType.F)
        return true;
      return false;
    }
    #endregion Static Helpers

    private static readonly Dictionary<string, SymbolType> UniqueSymbolDictionary =
      new Dictionary<string, SymbolType>(comparer);

    protected SymbolType (string identifier) {
      // We don't need to do this for unique dictionary comparisons, but we do it so that
      // we get uniform output from read out symbols.
      Identifier = identifier.ToUpperInvariant();
      if (UniqueSymbolDictionary.ContainsKey(Identifier)) {
        throw new InvalidOperationException(
          string.Format("Attempting to instantiate duplicate symbol {0}!", Identifier));
      }
      UniqueSymbolDictionary[Identifier] = this;
    }

    public string Identifier { get; }
  }
}
