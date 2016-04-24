using System;
using System.IO;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

namespace ZStewart.KOSLisp.Parser {

  /// <summary>
  /// A lexer definition for lexing kOS Lisp.
  /// </summary>
  public class LispLexer {

    // Classes for holding configuration data.
    #region Configuration Classes
    /// <summary>
    /// Configuration for a lexer mode.
    /// </summary>
    private class LexerModeConfig {
      /// <summary>
      /// List of token regex matchers to try in order, and the token creator functions
      /// to use on the values they match.
      /// </summary>
      public ImmutableList<Tuple<Regex, TokenCreator<LispTokType>>> Matchers { get; }

      /// <summary>
      /// Whether or not this mode allows line breaks.
      /// </summary>
      public bool AllowLineBreaks { get; }

      private LexerModeConfig(
          ImmutableList<Tuple<Regex, TokenCreator<LispTokType>>> matchers,
          bool allowLineBreaks) {
        Matchers = matchers;
        AllowLineBreaks = allowLineBreaks;
      }

      public class Builder {
        private List<Tuple<Regex, TokenCreator<LispTokType>>> matchers =
          new List<Tuple<Regex, TokenCreator<LispTokType>>>();
        private bool allowLineBreaks = true;

        public Builder () { }

        public Builder SetAllowLineBreaks(bool value) {
          allowLineBreaks = value;
          return this;
        }
        public Builder AddMatcher(string matcher, TokenCreator<LispTokType> createFunc) {
          matchers.Add(Tuple.Create(new Regex("^" + matcher, regexOptions), createFunc));
          return this;
        }
        public LexerModeConfig Build() {
          return new LexerModeConfig(
            ImmutableList.CreateRange(matchers), allowLineBreaks);
        }
      }
    }
    #endregion Configuration Classes

    #region Static Properties
    private const string SUBSYMBOL_REGEX = @"[\p{L}@<>=_+!~*^/\-\.\d]";
    private const string SYMBOL_REGEX =
      @"([&:]|" + SUBSYMBOL_REGEX + ")" + SUBSYMBOL_REGEX + "*";

    /// <summary>
    /// The configuration of the Lexer -- this is the set of modes and regexes used for
    /// parsing.
    /// </summary>
    private static readonly ImmutableDictionary<LispLexMode, LexerModeConfig>
      tokenizerConf;

    /// <summary>
    /// Regex mode options to be used in the lexer.
    /// </summary>
    private static readonly RegexOptions regexOptions =
      RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase;
    #endregion Static Properties

    #region Static Setup
    static LispLexer () {
      // Initialize the tokenizer configuration for lisp lexers.
      ImmutableDictionary<LispLexMode, LexerModeConfig>.Builder db =
        ImmutableDictionary.CreateBuilder<LispLexMode, LexerModeConfig>();
      db.Add(
        LispLexMode.NORMAL,
        new LexerModeConfig.Builder()
          .SetAllowLineBreaks(true)
          .AddMatcher(
            @"[+-]?[0-9]*\.?[0-9]+(e[+-]?[0-9]+)?(?!" + SYMBOL_REGEX + ")",
            GenericToken.CreateTokenCreator(LispTokType.NUMBER, double.Parse))
          .AddMatcher(SYMBOL_REGEX, (rv, s) => {
            // We have to combine the rules for things that *could* be identifiers to
            // prevent certain kinds of parse errors.
            // If we were to split these rules out:
            // .A -> DOT IDENTIFIER, should be Error.
            // A. -> IDENTIFIER DOT, should be Error.
            if (rv == ".")
              return RawToken.Create(rv, s, LispTokType.DOT);
            if ((rv.StartsWith(".") ? rv.Substring(1) : rv).Split('.')
                .Any(st => string.IsNullOrEmpty(st)))
              throw new InvalidIdentifier(
                "Invalid identifier. Cannot have adjacent dots or end with dot.",
                s);
            if ((rv.StartsWith(":") || rv.StartsWith("&")) && rv.Contains("."))
              throw new InvalidIdentifier(
                "Invalid identifier. Keword identifiers cannot contain dot.", s);
            return RawToken.Create(rv, s, LispTokType.IDENTIFIER);
          })
          .AddMatcher(@"\(", RawToken.CreateTokenCreator(LispTokType.OPEN_PAREN))
          .AddMatcher(@"\)", RawToken.CreateTokenCreator(LispTokType.CLOSE_PAREN))
          .AddMatcher(@"'", RawToken.CreateTokenCreator(LispTokType.QUOTE))
          .AddMatcher(@"`", RawToken.CreateTokenCreator(LispTokType.BACKQUOTE))
          .AddMatcher(@",@", RawToken.CreateTokenCreator(LispTokType.SPLICE))
          .AddMatcher(@",", RawToken.CreateTokenCreator(LispTokType.UNQUOTE))
          .AddMatcher("\"", RawToken.CreateTokenCreator(LispTokType.STARTSTRING))
          .AddMatcher(@";.*", (rv, s) => null)
          .AddMatcher(@"\s", (rv, s) => null)
          .Build()
      );
      db.Add(
        LispLexMode.STRING,
        new LexerModeConfig.Builder()
          .SetAllowLineBreaks(false)
          .AddMatcher(
            @"\\[rn""\\]",
            GenericToken.CreateTokenCreator(LispTokType.CHARACTER, escape => {
              switch (escape.ToLowerInvariant()) {
                case "\\\"":
                  return '"';
                case "\\r":
                  return '\r';
                case "\\n":
                  return '\n';
                default:
                  throw new ArgumentException(
                    string.Format("Unknown escape sequence: \"{0}\".", escape));
              }
            }
          ))
          .AddMatcher("\"", RawToken.CreateTokenCreator(LispTokType.ENDSTRING))
          .AddMatcher(
            @".", GenericToken.CreateTokenCreator(LispTokType.CHARACTER, c => {
              if (!(c.Length == 1)) throw new ArgumentException();
              return c[0];
            }
          ))
          .Build()
      );
      tokenizerConf = db.ToImmutable();
    }
    #endregion Static Setup

    #region Static Methods
    public static LispLexer Lex (string name, TextReader source) {
      return new LispLexer(name, source);
    }
    #endregion Static Methods

    #region Instance Properties
    private readonly TextReader source;
    private SourceInformation currentLoc;

    // Delegate these private variables to the source location structure. This
    // automatically keeps them in sync so that the currentLoc can be copied out at any
    // time. Since it's a struct, no reference is kept.
    private string Line {
      get { return currentLoc.Line; }
      set { currentLoc.Line = value; }
    }

    private int LineNumber {
      get { return currentLoc.LineNumber; }
      set { currentLoc.LineNumber = value; }
    }

    private int ColumnIndex {
      get { return currentLoc.ColumnIndex; }
      set { currentLoc.ColumnIndex = value; }
    }
    #endregion Instance Properties

    private LispLexer (string fileName, TextReader source) {
      if (!(!string.IsNullOrEmpty(fileName))) throw new ArgumentException();
      if (!(source != null)) throw new ArgumentNullException();
      this.source = source;
      currentLoc = new SourceInformation(fileName, "", 0, 0);
    }

    public Token<LispTokType> Next (LispLexMode mode) {
      var lexConf = tokenizerConf[mode];

      retry_match:
      if (AdvanceNextLine(lexConf)) return null;
      foreach (var matcher in lexConf.Matchers) {
        Match match = matcher.Item1.Match(Line.Substring(ColumnIndex));
        // Try the next matcher if this one fails.
        if (!match.Success) continue;
        ColumnIndex += match.Value.Length;
        var val = matcher.Item2(match.Value, currentLoc);
        if (val == null) {
          goto retry_match;
        }
        return val;
      }
      var loc = currentLoc;
      // On unrecognized input, advance to the end of the line to ensure that the next
      // read will try to fetch a new line from the input file. In non-interactive mode,
      // this shouldn't matter because non-interactive files shouldn't be retried. In
      // interactive mode, this ensures that we get a new line when the user typed
      // something invalid.
      ColumnIndex = Line.Length;
      throw new UnexpectedInput(
        string.Format("Unrecognized input"), currentLoc);
    }

    private bool AdvanceNextLine(LexerModeConfig lexConf) {
      while (ColumnIndex >= Line.Length) {
        if (!lexConf.AllowLineBreaks)
          // TODO(zstewar1): Better error messaging for this, maybe based on mode?
          throw new UnexpectedEOLException(
            "Unexpected end-of-line.", currentLoc);
        LineNumber += 1;
        ColumnIndex = 0;
        Line = source.ReadLine();
        if (Line == null) {
          return true;
        }
      }
      return false;
    }
  }
}
