using System;
using System.Collections.Generic;
using System.Linq;

using ZStewart.KOSLisp.Types;

namespace ZStewart.KOSLisp {
  /// <summary>
  /// Parses a token stream into s-expressions.
  /// </summary>
  public class LispParser {
    private readonly LispLexer lexer;

    public LispParser(LispLexer lexer) {
      this.lexer = lexer;
    }

    public IEnumerable<Token<LispTokType>> Parse () {
      LispLexMode mode = LispLexMode.NORMAL;
      Token<LispTokType> tok;
      while((tok = lexer.Next(mode)) != null) {
        if (tok.TokenType == LispTokType.STARTSTRING && mode == LispLexMode.NORMAL)
          mode = LispLexMode.STRING;
        if (tok.TokenType == LispTokType.ENDSTRING && mode == LispLexMode.STRING)
          mode = LispLexMode.NORMAL;
        yield return tok;
      }
    }
  }
}
