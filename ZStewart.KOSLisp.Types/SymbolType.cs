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

    [BuiltinFunction(Name = "--new--")]
    private static SymbolType New(
        [PositionalArgument] LispType subtype,
        [PositionalArgument] LispObject value) {
      if (subtype == Symbol) {
          if (value is SymbolType) {
            return (SymbolType)value;
          }
          else if (value is StringType) {
            return Create(((StringType)value).Value);
          } else {
            throw ExceptionType.ThrowTypeError(
              "argument to (sym) must be a symbol (returned unmodified) or a string to " +
              "look up");
          }
      } else {
        if (!LispType.IsSubtype(subtype, Symbol)) {
          throw ExceptionType.ThrowTypeError("type must be a subtype of sym");
        }
        if (!IsCorrectInstanceType(subtype, Symbol)) {
          throw ExceptionType.ThrowTypeError(
            "sym.--new-- cannot be used to instantiate object of type {0}", subtype);
        }
        if (!(value is StringType || value is NumberType)) {
          throw ExceptionType.ThrowTypeError(
            "identifier for new gensym must be a string or number");
        }
        string ident = value is StringType ? ((StringType)value).Value :
          ((NumberType)value).Value.ToString();
        return new SymbolType(ident, true) {
          __class__ = subtype,
          __dict__ = DictType.Create(),
        };
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
    #endregion Static Helpers

    /// <summary>
    /// Dictionary used to cache symbols by their identifier string, ensuring that symbols
    /// with the same name are globaly unique.
    ///
    /// Does not apply to gensyms.
    /// </summary>
    private static readonly Dictionary<string, SymbolType> UniqueSymbolDictionary =
      new Dictionary<string, SymbolType>(comparer);

    /// <summary>
    /// Create a symbol with the given identifier. Error if the symbol is already cached.
    /// </summary>
    protected SymbolType (string identifier) : this(identifier, false) {}

    protected SymbolType (string identifier, bool gensym) {
      // We don't need to do this for unique dictionary comparisons, but we do it so that
      // we get uniform output from read out symbols.
      Identifier = identifier.ToUpperInvariant();

      // Gensyms skip the symbol dictionary, all other symbols go in.
      if (!gensym) {
        if (UniqueSymbolDictionary.ContainsKey(Identifier)) {
          throw new InvalidOperationException(
            string.Format("Attempting to instantiate duplicate symbol {0}!", Identifier));
        }
        UniqueSymbolDictionary[Identifier] = this;
      }
    }

    public string Identifier { get; }

    public virtual bool IsSelfEvaluating { get { return false; } }
  }
}
