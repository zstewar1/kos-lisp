using System;
using System.Text;

using ZStewart.KOSLisp.Types;
using ZStewart.KOSLisp.Types.Helpers;

namespace ZStewart.KOSLisp.Parse.Lisp {
  /// <summary>
  /// Parses a stream of lisp-tokens into s-expressions.
  /// </summary>
  public class LispParser : Parser {
    /// <summary>
    /// The lexer which tokens are to be read from.
    /// </summary>
    private readonly Lexer<LispTokType, LispLexMode> lexer;

    /// <summary>
    /// The last-read token. Sometimes we need to know what a token is before we can
    /// decide what to do.
    ///
    /// Tokens are only read by certain parser functions. Parse advances the stream to
    /// start an expression,
    /// </summary>
    private Token<LispTokType> tok;

    /// <summary>
    /// Indicates the depth to which the parser is in backquotes. , and ,@ are illegal
    /// outside of backquotes, or within other ,/,@ expressions unless an additional
    /// backquote layer has been introduced.
    /// </summary>
    private int backquoteDepth = 0;

    /// <summary>
    /// Indicates the depth to which the parser is in unquote expressions (, or ,@). , and
    /// ,@ are illegal when unquoteDepth == backquoteDepth. unquoteDepth > backquoteDepth
    /// is a state error.
    /// </summary>
    private int unquoteDepth = 0;

    public LispParser(Lexer<LispTokType, LispLexMode> lexer) {
      this.lexer = lexer;
    }

    /// <summary>
    /// Parse the next complete expression the input source and convert it to a lisp
    /// object representation.
    /// </summary>
    public LispObject ParseNext () {
      tok = lexer.NextToken(LispLexMode.NORMAL);
      return ParseExpression();
    }

    /// <summary>
    /// Parse the next expression from the token stream and return it as a lisp object.
    ///
    /// Assumes that the starting token of the expression to be parsed has already been
    /// read into tok.
    /// </summary>
    /// <returns>
    /// A lisp object containing the raw, unexpanded parse tree of parsed object.
    /// </returns>
    private LispObject ParseExpression () {
      // Return a null lisp object if the token is null to indicate end-of-input. (null is
      // not a valid lisp object).
      if (tok == null) return null;

      switch (tok.TokenType) {
        case LispTokType.OPEN_PAREN:
          return ParseList();
        case LispTokType.STARTSTRING:
          return ParseString();
        case LispTokType.IDENTIFIER:
          return ParseIdentifier();
        case LispTokType.NUMBER:
          return NumberType.Create((tok as GenericToken<LispTokType, double>).Value);
        case LispTokType.QUOTE:
          return ParseQuoted();
        case LispTokType.BACKQUOTE:
          return ParseBackquote();
        case LispTokType.UNQUOTE:
          return ParseUnquote();
        case LispTokType.SPLICE:
          return ParseSplice();
        default:
          throw ExceptionType.ThrowSyntaxError(
            "Unexpected token of type {0} while parsing expression.", tok.TokenType);
      }
    }

    /// <summary>
    /// Parse a list assuming the opening paren has already been read.
    /// </summary>
    /// <returns>A list of tokens.</returns>
    private LispObject ParseList () {
      if (!(tok.TokenType == LispTokType.OPEN_PAREN))
        throw new InvalidOperationException();
      // TODO(zstewar1): make use of token start information for better error messaging.
      //var start = tok;
      LispObject list = NilType.Nil;
      LispObject end = list;
      // Whether the last token was a dot. If it was, we insert the next read value in the
      // cdr instead of appending.
      bool dot = false;
      // Whether a dot was already read. Multiple dots per list expression are illegal.
      bool dotDone = false;
      while (true) {
        tok = lexer.NextToken(LispLexMode.NORMAL);
        if (tok == null)
          throw ExceptionType.ThrowSyntaxError(
            "unexpected End of Input while reading list.");
        if (tok.TokenType == LispTokType.CLOSE_PAREN) {
          if (dot && !dotDone)
            throw ExceptionType.ThrowSyntaxError("illegal end of dotted list.");
          return list;
        } else if (tok.TokenType == LispTokType.DOT) {
          if (list == NilType.Nil)
            throw ExceptionType.ThrowSyntaxError("list cannot start with dot");
          if (dot || dotDone)
            throw ExceptionType.ThrowSyntaxError(
              "cannot have more than one dot in a list");
          // Read next expression as dotted list element.
          dot = true;
        } else {
          // If it isn't an empty-line error and isn't a close-paren, go back to
          // ParseExpression to figure out what it is.
          var obj = ParseExpression();
          if (obj == null)
            throw ExceptionType.ThrowSyntaxError(
              "unexpected End of Input while reading list");
          if (dot) {
            if (dotDone)
              throw ExceptionType.ThrowSyntaxError(
                "only one expression is allowed after dot");
            else {
              ListOperations.SetCdr(end, obj);
              dotDone = true;
            }
          } else if (list == NilType.Nil) {
            list = ConsType.Create(obj, NilType.Nil);
            end = list;
          } else {
            var newEnd = ConsType.Create(obj, NilType.Nil);
            ListOperations.SetCdr(end, newEnd);
            end = newEnd;
          }
        }
      }
    }

    LispObject ParseString() {
      if (!(tok.TokenType == LispTokType.STARTSTRING))
        throw new InvalidOperationException();
      //var start = tok;
      var builder = new StringBuilder();
      while (true) {
        tok = lexer.NextToken(LispLexMode.STRING);
        if (tok == null) {
          throw ExceptionType.ThrowSyntaxError(
            "unexpected end of input while reading string.");
        } else if (tok.TokenType == LispTokType.ENDSTRING) {
          return StringType.Create(builder.ToString());
        } else if (tok.TokenType == LispTokType.CHARACTER) {
          builder.Append((tok as GenericToken<LispTokType, char>).Value);
        } else {
          throw ExceptionType.ThrowSyntaxError(
            "unexpected token type {0} while parsing string.", tok.TokenType);
        }
      }
    }

    LispObject ParseIdentifier() {
      if (!(tok.TokenType == LispTokType.IDENTIFIER))
        throw new InvalidOperationException();
      if (!tok.RawValue.Contains(".")) {
        return SymbolType.Create(tok.RawValue);
      }
      var split = tok.RawValue.Split('.');
      // lexer should guarantee split has at least one element
      LispObject result;
      if (split[0] == "") {
        // ident begins with a dot, meaning load from next item.
        tok = lexer.NextToken(LispLexMode.NORMAL);
        result = ParseExpression();
        if (result == null) {
          throw ExceptionType.ThrowSyntaxError(
            "unexpected end of input while reading prefix-dotted identifier.");
        }
      } else {
        // begins with ident, meaning load from this variable.
        result = SymbolType.Create(split[0]);
      }
      for (int i = 1; i < split.Length; i++) {
        // This assumes that the getattr function is named getattr.
        result = ConsType.ToLispList(
          SymbolType.Create("getattr"), result,
          ConsType.ToLispList(SymbolType.Create("quote"), SymbolType.Create(split[i])));
      }
      return result;
    }

    LispObject ParseQuoted() {
      if (!(tok.TokenType == LispTokType.QUOTE)) throw new InvalidOperationException();
      return ParseWrappingExpression("quote", "quoted");
    }

    private LispObject ParseWrappingExpression(string wrapper, string exprType) {
      //var start = tok;
      tok = lexer.NextToken(LispLexMode.NORMAL);
      if (tok == null) {
        throw ExceptionType.ThrowSyntaxError(
          "unexpected end of input while reading {0} expression.", exprType);
      }
      var expr = ParseExpression();
      if (expr == null) {
        throw ExceptionType.ThrowSyntaxError(
          "unexpected end of input while reading quoted expression.", exprType);
      }
      return ConsType.ToLispList(SymbolType.Create(wrapper), expr);
    }

    private LispObject ParseBackquote () {
      if (!(tok.TokenType == LispTokType.BACKQUOTE))
        throw new InvalidOperationException();
      if (!(backquoteDepth >= unquoteDepth))
        throw new InvalidOperationException();
      var originalBackquoteDepth = backquoteDepth;
      try {
        backquoteDepth++;
        return ParseWrappingExpression("--backquote--", "backquoted");
      } finally {
        backquoteDepth = originalBackquoteDepth;
      }
    }

    private LispObject ParseUnquote () {
      if (!(tok.TokenType == LispTokType.UNQUOTE))
        throw new InvalidOperationException();
      if (!(backquoteDepth >= unquoteDepth))
        throw new InvalidOperationException();
      if (backquoteDepth == 0)
        throw ExceptionType.ThrowSyntaxError(
          "unquote is only allowed inside of backquote.");
      else if (backquoteDepth == unquoteDepth)
        throw ExceptionType.ThrowSyntaxError(
          "read too many unquotes for the depth of backquotes.");
      var orignalUnquoteDepth = unquoteDepth;
      try {
        unquoteDepth++;
        return ParseWrappingExpression(":--unquote--", "unquoted");
      } finally {
        unquoteDepth = orignalUnquoteDepth;
      }
    }

    private LispObject ParseSplice () {
      if (!(tok.TokenType == LispTokType.SPLICE))
        throw new InvalidOperationException();
      if (!(backquoteDepth >= unquoteDepth))
        throw new InvalidOperationException();
      if (backquoteDepth == 0)
        throw ExceptionType.ThrowSyntaxError(
          "splice is only allowed inside of backquote");
      else if (backquoteDepth == unquoteDepth)
        throw ExceptionType.ThrowSyntaxError(
          "read too many unquotes for the depth of backquotes");
      var orignalUnquoteDepth = unquoteDepth;
      try {
        unquoteDepth++;
        return ParseWrappingExpression(":--splice--", "spliced");
      } finally {
        unquoteDepth = orignalUnquoteDepth;
      }
    }
  }
}
