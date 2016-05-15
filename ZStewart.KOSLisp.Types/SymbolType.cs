using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using ZStewart.KOSLisp.Types.Attributes;
using ZStewart.KOSLisp.Types.Helpers;
using ZStewart.KOSLisp.Types.TypeCategories;

namespace ZStewart.KOSLisp.Types {
  public class SymbolType : LispObject {
    #region Static Type Setup
    private static LispType _symbol;
    public static LispType Symbol {
      get {
        if (_symbol != null) return _symbol;

        _symbol = new LispType {
          __name__ = "sym",
          _instance_type = typeof(SymbolType),
        };
        _symbol.__class__ = LispType.Type;
        _symbol.__bases__ = IConsType.ToLispTuple(LispObject.Object);
        _symbol.__mro__ = IConsType.ToLispTuple(Symbol, LispObject.Object);
        LispType.ConfigureType(_symbol);

        return _symbol;
      }
    }

    [BuiltinFunction(Name = "--new--")]
    private static SymbolType New(
        [Required] LispType subtype,
        [Required] LispObject value) {
      if (ReferenceEquals(subtype, Symbol)) {
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

    [BuiltinFunction(Name = "--init--")]
    private static void Init([RestIgnore] byte ri, [RestKwIgnore] byte rki) {}

    [BuiltinFunction(Name = "--repr--")]
    private static LispObject ToRepr([Required] SymbolType obj) {
      return StringType.Create(obj.Identifier);
    }
    #endregion

    public const string SUBSYMBOL_REGEX = @"[\p{L}@<>=_+!~*^%$/\-\d]";
    public const string SYMBOL_REGEX = SUBSYMBOL_REGEX + "+";
    public const RegexOptions SYMBOL_REGEX_OPTIONS =
      RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase;

    internal static readonly Regex symbolMatcher =
      new Regex("^" + SYMBOL_REGEX + "$", SYMBOL_REGEX_OPTIONS);

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

      if (identifier.StartsWith(":") || identifier.StartsWith("&")) {
        return KeywordSymbolType.Create(identifier);
      } else if (symbolMatcher.Match(identifier).Success) {
        return new SymbolType(identifier) {
          __class__ = Symbol,
        };
      } else {
        throw ExceptionType.ThrowValueError("not a valid symbol name: {0}", identifier);
      }
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

    // Bypass lookups for symbols.
    public override int GetHashCode() {
      if (__class__.RefEq(Symbol)) {
        return BaseObjectHash();
      } else {
        // LispObject ghc does the correct lookup.
        return base.GetHashCode();
      }
    }

    public override bool Equals(object other) {
      if (__class__.RefEq(Symbol)) {
        return ReferenceEquals(this, other);
      } else {
        return base.Equals(other);
      }
    }
  }
}
