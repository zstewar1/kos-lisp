using System;
using System.Collections.Generic;

using static ZStewart.KOSLisp.Types.ExceptionType;

using ZStewart.KOSLisp.Types;
using ZStewart.KOSLisp.Types.Helpers;

namespace ZStewart.KOSLisp.Parse.Lisp {
  /// <summary>
  /// Parses a stream of lisp-tokens into s-expressions.
  /// </summary>
  public class LispParser : Parser {

    private readonly Lexer<LispTokType> lexer;

    public LispParser(Lexer<LispTokType> lexer) {
      this.lexer = lexer;
    }

    public IEnumerable<LispObject> Parse(IEnumerable<string> source) {
      return new LispParserStateful(lexer.Lex(source));
    }

    /// <summary>
    /// The inner class which actually implements the parsing logic. This is a stateful
    /// object which handles a single lisp token at a time.
    /// </summary>
    private class LispParserStateful : IEnumerable<LispObject> {
      /// <summary>
      /// The lexer which tokens are to be read from.
      /// </summary>
      private readonly IEnumerator<LispTokType> lexer;

      /// <summary>
      /// For convenience lexer.Current is available as tok.
      /// </summary>
      private Token<LispTokType> tok => lexer.Current;

      // We store the back- and un- quote depth on the object because otherwise we would
      // have to pass it to every method. And this object is stateful anyway, so who
      // cares.
      /// <summary>
      /// Indicates the depth to which the parser is in backquotes. , and ,@ are illegal
      /// outside of backquotes, or within other ,/,@ expressions unless an additional
      /// backquote layer has been introduced.
      /// </summary>
      private int backquoteDepth = 0;

      /// <summary>
      /// Indicates the depth to which the parser is in unquote expressions (, or ,@). , and
      /// ,@ are illegal when unquoteDepth == backquoteDepth. unquoteDepth > backquoteDepth
      /// is a state error. This is not checked because it is private to the parser and it
      /// is assumed that the parser is correct.
      /// </summary>
      private int unquoteDepth = 0;

      internal LispParserStateful(IEnumerable<LispTokType> lexer)
          : this(lexer.GetEnumerator()) {}

      internal LispParserStateful(IEnumerator<LispTokType> lexer) {
        this.lexer = lexer;
      }

      IEnumerator<LispObject> GetEnumerator() {
        while (lexer.MoveNext()) {
          yield return ParseExpression();
        }
      }

      /// <summary>
      /// Parse the next expression from the token stream and return it as a lisp object.
      ///
      /// Assumes that the starting token of the expression to be parsed has already been
      /// read into tok. We do this because sometimes another method needs to move next
      /// before deciding whether it can handle the next token or should call back to
      /// ParseExpression. For example a list might find its close-paren, or it might find
      /// some other token, in which case it would need to call ParseExpression to
      /// continue parsing from the next token. Since ParseExpression just calls another
      /// expression type for most identifier types, all the Parse* methods have to assume
      /// that they have had their start-token read already.
      /// </summary>
      /// <returns>
      /// A lisp object containing the raw, unexpanded parse tree of parsed object.
      /// </returns>
      private LispObject ParseExpression () {
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
            throw ThrowSyntaxError(
              "Unexpected token of type {0} while parsing expression.", tok.TokenType);
        }
      }

      /// <summary>
      /// Parse a list assuming the opening paren has already been read.
      /// </summary>
      /// <returns>A list of tokens.</returns>
      private LispObject ParseList () {
        // TODO(zstewar1): make use of token source information for better error
        // messaging?

        LispObject list = NilType.Nil;
        LispObject end = list;
        // Whether the last token was a dot. If it was, we insert the next read value in the
        // cdr instead of appending.
        bool dot = false;
        // Whether a dot was already read. Multiple dots per list expression are illegal.
        bool dotDone = false;
        while (true) {
          if (!lexer.MoveNext()) {
            throw ThrowSyntaxError(
              "unexpected End of Input while reading list.");
          }
          if (tok.TokenType == LispTokType.CLOSE_PAREN) {
            if (dot && !dotDone) {
              throw ThrowSyntaxError("illegal end of dotted list.");
            }
            return list;
          } else if (tok.TokenType == LispTokType.DOT) {
            if (ReferenceEquals(list, NilType.Nil)) {
              throw hrowSyntaxError("list cannot start with dot");
            } else if (dot || dotDone) {
              throw ThrowSyntaxError(
                "cannot have more than one dot in a list");
            }
            // Read next expression as dotted list element.
            dot = true;
          } else {
            // If it isn't an empty-line error and isn't a close-paren, go back to
            // ParseExpression to figure out what it is.
            var obj = ParseExpression();
            if (dot) {
              if (dotDone) {
                throw ThrowSyntaxError(
                  "only one expression is allowed after dot");
              } else {
                ListOperations.SetCdr(end, obj);
                dotDone = true;
              }
            } else if (ReferenceEquals(list, NilType.Nil)) {
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
        var builder = new StringBuilder();
        while (true) {
          if (!lexer.MoveNext()) {
            throw ThrowSyntaxError(
              "unexpected end of input while reading string.");
          } else if (tok.TokenType == LispTokType.ENDSTRING) {
            return StringType.Create(builder.ToString());
          } else if (tok.TokenType == LispTokType.CHARACTER) {
            builder.Append((tok as GenericToken<LispTokType, char>).Value);
          } else {
            throw ThrowSyntaxError(
              "unexpected token type {0} while parsing string.", tok.TokenType);
          }
        }
      }

      LispObject ParseIdentifier() {
        if (!tok.RawValue.Contains(".")) {
          return SymbolType.Create(tok.RawValue);
        }
        var split = tok.RawValue.Split('.');
        // lexer should guarantee split has at least one element
        LispObject result;
        if (split[0] == "") {
          // ident begins with a dot, meaning load from next item.
          if (!lexer.MoveNext()) {
            throw ThrowSyntaxError(
              "unexpected end of input while reading prefix-dotted identifier.");
          }
          result = ParseExpression();
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
        return ParseWrappingExpression("quote", "quoted");
      }

      private LispObject ParseBackquote () {
        var originalBackquoteDepth = backquoteDepth;
        try {
          backquoteDepth++;
          return ParseWrappingExpression("--backquote--", "backquoted");
        } finally {
          backquoteDepth = originalBackquoteDepth;
        }
      }

      private LispObject ParseUnquote () {
        if (backquoteDepth == 0) {
          throw ThrowSyntaxError("unquote is only allowed inside of backquote.");
        } else if (backquoteDepth == unquoteDepth) {
          throw ThrowSyntaxError("read too many unquotes for the depth of backquotes.");
        }
        var orignalUnquoteDepth = unquoteDepth;
        try {
          unquoteDepth++;
          return ParseWrappingExpression(":--unquote--", "unquoted");
        } finally {
          unquoteDepth = orignalUnquoteDepth;
        }
      }

      private LispObject ParseSplice () {
        if (backquoteDepth == 0) {
          throw ThrowSyntaxError("splice is only allowed inside of backquote");
        } else if (backquoteDepth == unquoteDepth) {
          throw ThrowSyntaxError("read too many unquotes for the depth of backquotes");
        }
        var orignalUnquoteDepth = unquoteDepth;
        try {
          unquoteDepth++;
          return ParseWrappingExpression(":--splice--", "spliced");
        } finally {
          unquoteDepth = orignalUnquoteDepth;
        }
      }

      /// <summary>
      /// Parse an expression type which wrapps another type of expression. E.g. '(a) is a
      /// "quoted" expression which wraps a cons, and `a is a backquoted expression which
      /// wraps a symbol.
      ///
      /// This expression only assumes that the wrapping indicator token (e.g. the
      /// quote, backquote, etc.) has been read, and that the start token of the wrapped
      /// expression has not been.
      /// </summary>
      /// <param name="wrapper">
      /// The symbol name of the symbol to wrap the parsed expression with.
      /// </param>
      /// <param name="exprType">
      /// This is a descriptiv string for what kind of wrapping expression this is, used
      /// only in error messages. e.g. "quoted" or "backquoted" in the phrase error while
      /// reading quoted expression.
      /// </param>
      /// <returns>
      /// The return value will be a cons-list of length two with the first cell
      /// containing the symbol form of wrapper, and the second cell containing the
      /// expression that is wrapped.
      /// </returns>
      private LispObject ParseWrappingExpression(string wrapper, string exprType) {
        if (!lexer.MoveNext()) {
          throw ExceptionType.ThrowSyntaxError(
            "unexpected end of input while reading {0} expression.", exprType);
        }
        var expr = ParseExpression();
        return ConsType.ToLispList(SymbolType.Create(wrapper), expr);
      }
    }
  }
}
