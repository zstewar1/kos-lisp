using System.Text;
using ZStewart.KOSLisp.Types;
using ZStewart.KOSLisp.Types.Helpers;

namespace ZStewart.KOSLisp.Parser {
  /// <summary>
  /// Parses a token stream into s-expressions.
  /// </summary>
  public class LispParser {
    private readonly LispLexer lexer;

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
          return SymbolType.Create(tok.RawValue);
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
          throw new UnexpectedToken(
            string.Format(
              "Unexpected token of type {0} while parsing expression.", tok.TokenType),
            tok.SourceInformation);
      }
    }

    /// <summary>
    /// Parse a list assuming the opening paren has already been read.
    /// </summary>
    /// <returns>A list of tokens.</returns>
    private LispObject ParseList () {
      Preconditions.CheckState(tok.TokenType == LispTokType.OPEN_PAREN);
      var start = tok;
      LispObject list = NilType.Nil;
      LispObject end = list;
      // Whether the last token was a dot. If it was, we insert the next read value in the
      // cdr instead of appending.
      bool dot = false;
      // Whether a dot was already read. Multiple dots per list expression are illegal.
      bool dotDone = false;
      while (true) {
        tok = lexer.Next(LispLexMode.NORMAL);
        if (tok == null)
          throw new UnexpectedEndOfInput(
            "Unexpected End of Input while reading list.", start.SourceInformation);
        if (tok.TokenType == LispTokType.CLOSE_PAREN) {
          if (dot && !dotDone)
            throw new IllegalDottedList(
              "Illegal end of dotted list.", tok.SourceInformation);
          return list;
        } else if (tok.TokenType == LispTokType.DOT) {
          if (list == NilType.Nil)
            throw new IllegalDottedList(
              "List cannot start with dot.", tok.SourceInformation);
          if (dot || dotDone)
            throw new IllegalDottedList(
              "Cannot have more than one dot in a list", tok.SourceInformation);
          // Read next expression as dotted list element.
          dot = true;
        } else {
          // If it isn't an empty-line error and isn't a close-paren, go back to
          // ParseExpression to figure out what it is.
          var obj = ParseExpression();
          if (obj == null)
            throw new UnexpectedEndOfInput(
              "Unexpected End of Input while reading list.", tok.SourceInformation);
          if (dot) {
            if (dotDone)
              throw new IllegalDottedList(
                "Only one expression is allowed after dot.", tok.SourceInformation);
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
      Preconditions.CheckState(tok.TokenType == LispTokType.STARTSTRING);
      var start = tok;
      var builder = new StringBuilder();
      while (true) {
        tok = lexer.Next(LispLexMode.STRING);
        if (tok == null) {
          throw new UnexpectedEndOfInput(
            "Unexpected end of input while reading string.", start.SourceInformation);
        } else if (tok.TokenType == LispTokType.ENDSTRING) {
          return StringType.Create(builder.ToString());
        } else if (tok.TokenType == LispTokType.CHARACTER) {
          builder.Append((tok as GenericToken<LispTokType, char>).Value);
        } else {
          throw new UnexpectedToken(
            string.Format(
              "Unexpected token type {0} while parsing string.", tok.TokenType),
            tok.SourceInformation);
        }
      }
    }

    LispObject ParseQuoted() {
      Preconditions.CheckState(tok.TokenType == LispTokType.QUOTE);
      return ParseWrappingExpression("quote", "quoted");
    }

    private LispObject ParseWrappingExpression(string wrapper, string exprType) {
      var start = tok;
      tok = lexer.Next(LispLexMode.NORMAL);
      if (tok == null) {
        throw new UnexpectedEndOfInput(
          string.Format(
            "Unexpected end of input while reading {0} expression.", exprType),
          start.SourceInformation);
      }
      var expr = ParseExpression();
      if (expr == null) {
        throw new UnexpectedEndOfInput(
          string.Format(
            "Unexpected end of input while reading quoted expression.", exprType),
          tok.SourceInformation);
      }
      return ConsType.ToLispList(SymbolType.Create(wrapper), expr);
    }

    private LispObject ParseBackquote () {
      Preconditions.CheckState(tok.TokenType == LispTokType.BACKQUOTE);
      Preconditions.CheckState(backquoteDepth >= unquoteDepth);
      var originalBackquoteDepth = backquoteDepth;
      try {
        backquoteDepth++;
        return ParseWrappingExpression("--backquote--", "backquoted");
      } finally {
        backquoteDepth = originalBackquoteDepth;
      }
    }

    private LispObject ParseUnquote () {
      Preconditions.CheckState(tok.TokenType == LispTokType.UNQUOTE);
      Preconditions.CheckState(backquoteDepth >= unquoteDepth);
      if (backquoteDepth == 0)
        throw new IllegalUnquote(
          "Unquote is only allowed inside of backquote.", tok.SourceInformation);
      else if (backquoteDepth == unquoteDepth)
        throw new IllegalUnquote(
          "Read too many unquotes for the depth of backquotes.", tok.SourceInformation);
      var orignalUnquoteDepth = unquoteDepth;
      try {
        unquoteDepth++;
        return ParseWrappingExpression("--unquote--", "unquoted");
      } finally {
        unquoteDepth = orignalUnquoteDepth;
      }
    }

    private LispObject ParseSplice () {
      Preconditions.CheckState(tok.TokenType == LispTokType.SPLICE);
      Preconditions.CheckState(backquoteDepth >= unquoteDepth);
      if (backquoteDepth == 0)
        throw new IllegalUnquote(
          "Splice is only allowed inside of backquote", tok.SourceInformation);
      else if (backquoteDepth == unquoteDepth)
        throw new IllegalUnquote(
          "Read too many unquotes for the depth of backquotes.", tok.SourceInformation);
      var orignalUnquoteDepth = unquoteDepth;
      try {
        unquoteDepth++;
        return ParseWrappingExpression("--splice--", "spliced");
      } finally {
        unquoteDepth = orignalUnquoteDepth;
      }
    }
  }
}
