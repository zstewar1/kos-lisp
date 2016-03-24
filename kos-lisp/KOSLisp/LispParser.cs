using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZStewart.KOSLisp.Interpreter;

namespace ZStewart.KOSLisp {
  /// <summary>
  /// Parses a token stream into s-expressions.
  /// </summary>
  public class LispParser {
    private readonly LispLexer lexer;

    /// <summary>
    /// The last-read token. Sometimes we need to know what a token is before we can decide what to do.
    /// 
    /// Tokens are only read by certain parser functions. Parse advances the stream to start an expression, 
    /// </summary>
    private Token<LispTokType> tok;

    public LispParser(LispLexer lexer) {
      this.lexer = lexer;
    }

    public LispObject Parse () {
      tok = lexer.Next(LispLexMode.NORMAL);
      return ParseExpression();
    }

    /// <summary>
    /// Parse the next expression from the token stream and return it as a lisp object.
    /// </summary>
    /// <returns>A lisp object containing the raw, unexpanded parse tree of parsed object.</returns>
    private LispObject ParseExpression () {
      // Return a null lisp object if the token is null to indicate end-of-input. (null is not a valid lisp object).
      if (tok == null) return null;

      switch (tok.TokenType) {
        case LispTokType.OPEN_PAREN:
          return ParseList();
        case LispTokType.STARTSTRING:
          return ParseString();
        case LispTokType.IDENTIFIER:
          return LispSymbol.Of(tok.RawValue);
        case LispTokType.FLOAT:
          return LispFloat.Of((tok as GenericToken<LispTokType, double>).Value);
        case LispTokType.INT:
          return LispInt.Of((tok as GenericToken<LispTokType, long>).Value);
        case LispTokType.QUOTE:
          return ParseQuoted();
        case LispTokType.BACKQUOTE:
          throw new NotImplementedException("No backquote support yet.");
          //return ParseBackquoted();
        default:
          throw new UnexpectedToken(
            string.Format("Unexpected token of type {0} while parsing expression.", tok.TokenType),
            tok);
      }
    }

    /// <summary>
    /// Parse a list assuming the opening paren has already been read.
    /// </summary>
    /// <returns>A list of tokens.</returns>
    private LispObject ParseList () {
      Preconditions.CheckState(tok.TokenType == LispTokType.OPEN_PAREN);
      var start = tok;
      LispList list = LispNil.Nil;
      LispList end = list;
      // Whether the last token was a dot. If it was, we insert the next read value in the cdr instead of appending.
      bool dot = false;
      // Whether a dot was already read. Multiple dots per list expression are illegal.
      bool dotDone = false;
      while (true) {
        tok = lexer.Next(LispLexMode.NORMAL);
        if (tok == null) throw new UnexpectedEndOfInput("Unexpected End of Input while reading list.", start);
        if (tok.TokenType == LispTokType.CLOSE_PAREN) {
          if (dot && !dotDone)
            throw new IllegalDottedList("Illegal end of dotted list.", tok);
          return list;
        } else if (tok.TokenType == LispTokType.DOT) {
          if (list == LispNil.Nil)
            throw new IllegalDottedList("List cannot start with dot.", tok);
          if (dot || dotDone)
            throw new IllegalDottedList("Cannot have more than one dot in a list", tok);
          // Read next expression as dotted list element.
          dot = true;
        } else {
          // If it isn't an empty-line error and isn't a close-paren, go back to ParseExpression to figure out what it is.
          var obj = ParseExpression();
          if (obj == null)
            throw new UnexpectedEndOfInput("Unexpected End of Input while reading list.", tok);
          if (dot) {
            if (dotDone)
              throw new IllegalDottedList("Only one expression is allowed after dot.", tok);
            else {
              list.Cdr = obj;
              dotDone = true;
            }
          } else if (list == LispNil.Nil) {
            list = LispCons.Of(obj, LispNil.Nil);
            end = list;
          } else {
            // Maybe assert that cdr is nil?
            end.Cdr = LispCons.Of(obj, LispNil.Nil);
            end = (LispList)end.Cdr;
          }
        }
      }
    }

    LispObject ParseString() {
      Preconditions.CheckState(tok.TokenType == LispTokType.STARTSTRING);
      var start = tok;
      var builder = new StringBuilder();
      while (true) {
        tok = lexer.Next(LispLexMode.STRING);
        if (tok == null) {
          throw new UnexpectedEndOfInput("Unexpected end of input while reading string.", start);
        } else if (tok.TokenType == LispTokType.ENDSTRING) {
          return LispString.Of(builder.ToString());
        } else if (tok.TokenType == LispTokType.CHARACTER) {
          builder.Append((tok as GenericToken<LispTokType, char>).Value);
        } else {
          throw new UnexpectedToken(
            string.Format("Unexpected token type {0} while parsing string.", tok.TokenType),
            tok);
        }
      }
    }

    LispObject ParseQuoted() {
      Preconditions.CheckState(tok.TokenType == LispTokType.QUOTE);
      var start = tok;
      tok = lexer.Next(LispLexMode.NORMAL);
      if (tok == null) {
        throw new UnexpectedEndOfInput(
          "Unexpected end of input while reading quoted expression.", start);
      }
      return LispCons.Of(LispSymbol.Of("quote"), LispCons.Of(ParseExpression(), LispNil.Nil));
    }
  }
}
