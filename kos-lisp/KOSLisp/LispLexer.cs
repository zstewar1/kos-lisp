using System;
using System.IO;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace ZStewart.KOSLisp {

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

      private LexerModeConfig(ImmutableList<Tuple<Regex, TokenCreator<LispTokType>>> matchers, bool allowLineBreaks) {
        Matchers = matchers;
        AllowLineBreaks = allowLineBreaks;
      }

      public class Builder {
        private List<Tuple<Regex, TokenCreator<LispTokType>>> matchers = new List<Tuple<Regex, TokenCreator<LispTokType>>>();
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
          return new LexerModeConfig(ImmutableList.CreateRange(matchers), allowLineBreaks);
        }
      }
    }
    #endregion 

    #region Static Properties
    /// <summary>
    /// The configuration of the Lexer -- this is the set of modes and regexes used for parsing.
    /// </summary>
    private static readonly ImmutableDictionary<LispLexMode, LexerModeConfig> tokenizerConf;

    /// <summary>
    /// Regex mode options to be used in the lexer.
    /// </summary>
    private static readonly RegexOptions regexOptions = RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase;
    #endregion //Static Properties

    #region Static Setup
    static LispLexer () {
      // Initialize the tokenizer configuration for lisp lexers.
      ImmutableDictionary<LispLexMode, LexerModeConfig>.Builder db =
        ImmutableDictionary.CreateBuilder<LispLexMode, LexerModeConfig>();
      db.Add(
        LispLexMode.NORMAL,
        new LexerModeConfig.Builder()
          .SetAllowLineBreaks(true)
          .AddMatcher(@"[+-]?[0-9]*\.[0-9]+(e[+-]?[0-9]+)?", GenericToken.CreateTokenCreator(LispTokType.FLOAT, double.Parse))
          .AddMatcher(@"[+-]?[0-9]+", GenericToken.CreateTokenCreator(LispTokType.INT, long.Parse))
          .AddMatcher(@"[\p{L}&_+*/\-\.:\d]+", (rv, s, l, c) => {
            // We have to combine the rules for things that *could* be identifiers to prevent certain kinds of parse errors.
            // If we were to split these rules out:
            // .A -> DOT IDENTIFIER, should be Error.
            // A. -> IDENTIFIER DOT, should be Error.
            if (rv == ".")
              return RawToken.Create(rv, s, l, c, LispTokType.DOT);
            if (rv.StartsWith(".") || rv.EndsWith("."))
              throw new InvalidIdentifier("Identifiers cannot start or end with '.'", s, l, c);
            return RawToken.Create(rv, s, l, c, LispTokType.IDENTIFIER);
          })
          .AddMatcher(@"\(", RawToken.CreateTokenCreator(LispTokType.OPEN_PAREN))
          .AddMatcher(@"\)", RawToken.CreateTokenCreator(LispTokType.CLOSE_PAREN))
          .AddMatcher(@"'", RawToken.CreateTokenCreator(LispTokType.QUOTE))
          .AddMatcher(@"`", RawToken.CreateTokenCreator(LispTokType.BACKQUOTE))
          .AddMatcher(@",@", RawToken.CreateTokenCreator(LispTokType.SPLICE))
          .AddMatcher(@",", RawToken.CreateTokenCreator(LispTokType.UNQUOTE))
          .AddMatcher("\"", RawToken.CreateTokenCreator(LispTokType.STARTSTRING))
          .AddMatcher(@";.*", (rv, s, l, c) => null)
          .AddMatcher(@"\s", (rv, s, l, c) => null)
          .Build()
      );
      db.Add(
        LispLexMode.STRING,
        new LexerModeConfig.Builder()
          .SetAllowLineBreaks(false)
          .AddMatcher(@"\\[rn""\\]", GenericToken.CreateTokenCreator(LispTokType.CHARACTER, escape => {
            switch (escape.ToLowerInvariant()) {
              case "\\\"":
                return '"';
              case "\\r":
                return '\r';
              case "\\n":
                return '\n';
              default:
                throw new ArgumentException(String.Format("Unknown escape sequence: \"{0}\".", escape));
            }
          }))
          .AddMatcher("\"", RawToken.CreateTokenCreator(LispTokType.ENDSTRING))
          .AddMatcher(@".", GenericToken.CreateTokenCreator(LispTokType.CHARACTER, c => {
            Preconditions.CheckArgument(c.Length == 1, "Character match must be of length 1.");
            return c[0];
          }))
          .Build()
      );
      tokenizerConf = db.ToImmutable();
    }
    #endregion // Static Setup

    #region Static Methods
    public static LispLexer Lex (TextReader source) {
      return new LispLexer(source);
    }
    #endregion 

    #region Instance Properties
    private readonly TextReader source;
    private int lineNumber = 0, columnIndex = 0;
    private string line = "";
    #endregion

    private LispLexer (TextReader source) {
      this.source = source;
    }

    public Token<LispTokType> Next (LispLexMode mode) {
      var lexConf = tokenizerConf[mode];

      retry_match:
      while (columnIndex >= line.Length) {
        if (!lexConf.AllowLineBreaks)
          // TODO(zstewar1): Better error messaging for this, maybe based on mode?
          throw new UnexpectedEOLException("Unexpected end-of-line.", line, lineNumber, columnIndex);
        lineNumber += 1;
        columnIndex = 0;
        line = source.ReadLine();
        if (line == null) {
          return null;
        }
      }
      foreach (var matcher in lexConf.Matchers) {
        Match match = matcher.Item1.Match(line.Substring(columnIndex));
        // Try the next matcher if this one fails.
        if (!match.Success) continue;
        columnIndex += match.Length;
        var val = matcher.Item2(match.Value, line, lineNumber, columnIndex);
        if (val == null) {
          goto retry_match;
        }
        return val;
      }
      throw new UnexpectedInput(string.Format("Unrecognized input"), line, lineNumber, columnIndex);
    }
  }
}
