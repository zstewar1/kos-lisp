using System;
using System.Collections.Generic;
using System.Linq;

using ZStewart.Compilers;
using ZStewart.KOSLisp.Types;

namespace ZStewart.KOSLisp {
  /// <summary>
  /// Parses a token stream into s-expressions.
  /// </summary>
  public class LispParser : Parser<LispTokType, LispLexMode> {

    public CodeGenerator Parse (Tokenizer<LispTokType, LispLexMode> tokenizer) {

      return null;
    }

    /// <summary>
    /// Read the top-level s-expressions in a file. These must all start with OPEN_PAREN.
    /// </summary>
    /// <param name="tokenizer">The tokenizer to read expressions from.</param>
    /// <returns>A lisp list of the parsed expressions.</returns>
    private LispList ReadTopLevelSExps(Tokenizer<LispTokType, LispLexMode> tokenizer) {
      var exps = new List<LispObject>();

      Token<LispTokType> tok;
      while((tok = tokenizer.Next(LispLexMode.NORMAL)) != null) {
        if (tok.TokenType != LispTokType.OPEN_PAREN) {
          throw new UnexpectedInput(string.Format("Unexpected token {0}", tok.TokenType), tok.Column, tok.Line, tok.Column);
        }
        exps.Add(ReadSExp(tokenizer));
      }
      return LispTypeHelpers.ToLispList(exps);
    }

    /// <summary>
    /// Reads a single s-expression. Assumes that the OPEN_PAREN of this expression has already been read, and reads util CLOSE_PAREN.
    /// </summary>
    /// <param name="tokenizer">The tokenizer to read from.</param>
    /// <returns>A lisp object representing the expression that was read.</returns>
    private LispObject ReadSExp(Tokenizer<LispTokType, LispLexMode> tokenizer) {
      return LispNil.Nil;
    }
  }
}
