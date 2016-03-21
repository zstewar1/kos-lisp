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
      Token<LispTokType> tok;
      while ((tok = tokenizer.Next(LispLexMode.NORMAL)) != null) {
        Console.WriteLine("Read Token: {0}", tok);
      }
      return null;
    }

    /// <summary>
    /// Read the top-level s-expressions in a file. These must all start with OPEN_PAREN.
    /// </summary>
    /// <param name="tokenizer">The tokenizer to read expressions from.</param>
    /// <returns>A lisp list of the parsed expressions.</returns>
    private LispList ReadTopLevelSExps (Tokenizer<LispTokType, LispLexMode> tokenizer) {
      var sExps = new List<LispObject>();

      LispObject sExp;
      while ((sExp = ReadSExp(tokenizer)) != null) {
        sExps.Add(sExp);
      }
      return LispTypeHelpers.ToLispList(sExps);
    }

    /// <summary>
    /// Reads a single s-expression.
    /// </summary>
    /// <param name="tokenizer">The tokenizer to read from.</param>
    /// <returns>A lisp object representing the expression that was read.</returns>
    private LispObject ReadSExp (Tokenizer<LispTokType, LispLexMode> tokenizer) {
      Token<LispTokType> nextToken = tokenizer.Next(LispLexMode.NORMAL);
      if (nextToken == null)
        return null;
      switch (nextToken.TokenType) {
      case LispTokType.OPEN_PAREN:
        return ReadToCloseParen(tokenizer);
      case LispTokType.QUOTE:
        return ReadQuoted(tokenizer);
      case LispTokType.STARTSTRING:
        return ReadString(tokenizer);
      case LispTokType.NIL:
        return LispNil.Nil;
      case LispTokType.T:
        return LispT.T;
      case LispTokType.FLOAT:
        return LispFloat.Of((nextToken as GenericToken<LispTokType, double>).Value);
      case LispTokType.INT:
        return LispInt.Of((nextToken as GenericToken<LispTokType, long>).Value);
      case LispTokType.IDENTIFIER:
        return LispSymbol.Of(nextToken.RawValue);
      default:
        throw new UnexpectedToken(
          string.Format("Unexpected token {0}", nextToken.TokenType),
          nextToken.Index, nextToken.Line, nextToken.Column);
      }
    }
  }
}
