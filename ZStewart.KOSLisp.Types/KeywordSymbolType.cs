using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using ZStewart.KOSLisp.Types.Attributes;

namespace ZStewart.KOSLisp.Types {
  public sealed class KeywordSymbolType : SymbolType {
    #region Static Type Setup
    private static LispType _keywordSymbol;
    public static LispType KeywordSymbol {
      get {
        if (_keywordSymbol != null) return _keywordSymbol;

        _keywordSymbol = new LispType {
          __name__ = "keyword",
          _instance_type = typeof(KeywordSymbolType),
        };
        _keywordSymbol.__class__ = LispType.Type;
        _keywordSymbol.__bases__ = IConsType.ToLispTuple(Symbol);
        LispType.ConfigureType(_keywordSymbol);

        return _keywordSymbol;
      }
    }

    [BuiltinFunction(Name = "--new--")]
    private static KeywordSymbolType New(
        [Required] LispType kwd,
        [Required] string ident) {
      if (kwd != KeywordSymbol) {
        throw ExceptionType.ThrowTypeError(
          "keyword can only be used to look up keyword symbols");
      }
      return Create(ident);
    }

    [BuiltinFunction]
    private static SymbolType unprefix([Required] KeywordSymbolType self) {
      return self.Unprefix();
    }
    #endregion Static Type Setup

    public const string KEYWORD_SYMBOL_REGEX = @"[&:]" + SUBSYMBOL_REGEX + "*";
    internal static Regex keywordMatcher =
      new Regex("^" + KEYWORD_SYMBOL_REGEX + "$", SYMBOL_REGEX_OPTIONS);

    #region Static Helpers
    public static new KeywordSymbolType Create(string ident) {
      if (KeywordSymbolType.keywordMatcher.Match(ident).Success) {
        return new KeywordSymbolType(ident) {
          __class__ = KeywordSymbol,
        };
      }
      throw ExceptionType.ThrowValueError("not a valid keyword symbol name: {0}", ident);
    }
    #endregion Static Helpers

    // Keywords are always not gensyms.
    private KeywordSymbolType(string ident) : base(ident) {}

    public override bool IsSelfEvaluating { get { return true; } }

    /// <summary>
    /// Get the regular non-keyword symbol with the same name as this keyword symbol.
    /// </summary>
    public SymbolType Unprefix() {
      return SymbolType.Create(Identifier.Substring(1, Identifier.Length - 1));
    }
  }
}
