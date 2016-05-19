using System;
using System.Collections.Immutable;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;

using static ZStewart.KOSLisp.Types.ExceptionType;

using ZStewart.KOSLisp.Types;

namespace ZStewart.KOSLisp.Parse.Lisp {

  /// <summary>
  /// A lexer definition for lexing kOS Lisp.
  /// </summary>
  public class LispLexer : Lexer<LispTokType> {

    // Classes for holding configuration data.
    #region Configuration Classes
    /// <summary>
    /// Enumeration of the various matcher modes available in this lisp. These are a
    /// little like (f)lex start states (but crappier).
    /// </summary>
    protected enum LispLexMode {
      NORMAL,
      STRING,
    }

    protected delegate Token<LispTokType> LexerAction(
        LispLexerStateful lexer, string rawValue, SourceInformation sourceInfo);

    /// <summary>
    /// Configuration for a lexer mode.
    /// </summary>
    protected class LexerModeConfig {
      /// <summary>
      /// List of token regex matchers to try in order, and the token creator functions
      /// to use on the values they match.
      /// </summary>
      public ImmutableList<Tuple<Regex, LexerAction>> Matchers { get; }

      /// <summary>
      /// Whether or not this mode allows line breaks.
      /// </summary>
      public bool AllowLineBreaks { get; }

      private LexerModeConfig(
          ImmutableList<Tuple<Regex, LexerAction>> matchers,
          bool allowLineBreaks) {
        Matchers = matchers;
        AllowLineBreaks = allowLineBreaks;
      }

      public class Builder {
        private List<Tuple<Regex, LexerAction>> matchers =
          new List<Tuple<Regex, LexerAction>>();
        private bool allowLineBreaks = true;
        private RegexOptions regexOptions = DEFAULT_REGEX_OPTIONS;

        public Builder () { }

        public Builder SetAllowLineBreaks(bool allowLineBreaks) {
          this.allowLineBreaks = allowLineBreaks;
          return this;
        }
        public Builder AddMatcher(string matcher, LexerAction action) {
          matchers.Add(Tuple.Create(new Regex("^" + matcher, regexOptions), action));
          return this;
        }
        public Builder AddMatcher(string matcher, TokenCreator<LispTokType> createFunc) {
          return AddMatcher(matcher, (unused, rv, si) => createFunc(rv, si));
        }
        public Builder SetRegexOptions(RegexOptions regexOptions) {
          this.regexOptions = regexOptions;
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
    // Constants for defining the default lexer.
    private const string DOTTED_SYMBOL_REGEX =
      @"(\.?" + SymbolType.SYMBOL_REGEX + @"(\." + SymbolType.SYMBOL_REGEX + ")*)";
    private const string PARTIAL_SYMBOL_REGEX =
      "(" + DOTTED_SYMBOL_REGEX + "|" + KeywordSymbolType.KEYWORD_SYMBOL_REGEX +
      @"|\.)";
    private const string SYMBOL_REGEX =
      PARTIAL_SYMBOL_REGEX + @"(?!" + PARTIAL_SYMBOL_REGEX + ")";

    /// <summary>
    /// The default regex options applied to new lexer matchers.
    /// </summary>
    protected const RegexOptions DEFAULT_REGEX_OPTIONS =
      RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase;

    /// <summary>
    /// The configuration of the Lexer -- this is the set of modes and regexes used for
    /// parsing.
    /// </summary>
    private readonly ImmutableDictionary<LispLexMode, LexerModeConfig> tokenizerConf;

    #endregion Static Properties

    #region Static Setup
    /// <summary>
    /// Create a lexer with the default configuration.
    /// </summary>
    public static LispLexer CreateDefaultLexer() {
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
            if (rv == ".")
              return RawToken.Create(rv, s, LispTokType.DOT);
            return RawToken.Create(rv, s, LispTokType.IDENTIFIER);
          })
          .AddMatcher(@"\(", RawToken.CreateTokenCreator(LispTokType.OPEN_PAREN))
          .AddMatcher(@"\)", RawToken.CreateTokenCreator(LispTokType.CLOSE_PAREN))
          .AddMatcher(@"'", RawToken.CreateTokenCreator(LispTokType.QUOTE))
          .AddMatcher(@"`", RawToken.CreateTokenCreator(LispTokType.BACKQUOTE))
          .AddMatcher(@",@", RawToken.CreateTokenCreator(LispTokType.SPLICE))
          .AddMatcher(@",", RawToken.CreateTokenCreator(LispTokType.UNQUOTE))
          .AddMatcher("\"", (lexer, rv, s) => {
            lexer.lexMode = LispLexMode.STRING;
            lexer.StringCollector.Clear();
            return null;
          }),
          .AddMatcher(@";.*", (rv, s) => null)
          .AddMatcher(@"\s", (rv, s) => null)
          .Build()
      );
      db.Add(
        LispLexMode.STRING,
        new LexerModeConfig.Builder()
          .SetAllowLineBreaks(false)
          .AddMatcher(
            @"\\.", (state, rv, si) => {
              switch (rv.ToLowerInvariant()) {
                case "\\\"":
                  state.StringCollector.Append('"');
                  break;
                case "\\r":
                  state.StringCollector.Append('\r');
                  break;
                case "\\n":
                  state.StringCollector.Append('\n');
                  break;
                case "\\\\":
                  state.StringCollector.Append('\\');
                  break;
                default:
                  throw ThrowSyntaxError("unknown escape sequence: \"{0}\".", rv);
              }
              return null;
            })
          .AddMatcher("\"", (state, rv, si) => {
              return GenericToken.Create(
                LispTokType.STRING, state.StringCollector.ToString());
            })
          .AddMatcher(
            @"[^""\\]+", (state, rv, si) => {
              state.StringCollector.Append(rv);
              return null;
            }))
          .Build()
      );
      return new LispLexer(db.ToImmutable());
    }
    #endregion Static Setup

    /// <summary>
    /// Create a new lexer which reads tokens using the specified configuration.
    /// </summary>
    protected LispLexer(ImmutableDictionary<LispLexMode, LexerModeConfig> tokenizerConf) {
      this.tokenizerConf = tokenizerConf;
    }

    IEnumerable<Token<LispTokType>> Lex(IEnumerable<string> source) {
      return new LispLexerStateful(source, tokenizerConf);
    }

    #region Inner Stateful Class Implementation
    protected class LispLexerStateful : IEnumerable<Token<LispTokType>> {
      public LispLexerStateful(
          IEnumerable<string> lines,
          ImmutableDictionary<LispLexMode, LexerModeConf> tokenizerConf)
          : this(lines.GetEnumerator(), tokenizerConf) {}
      public LispLexerStateful(
          IEnumerator<string> lines,
          ImmutableDictionary<LispLexMode, LexerModeConf> tokenizerConf) {
        this.lines = lines;
        this.tokenizerConf = tokenizerConf;
      }

      public LispLexMode Mode { get; set; } = LispLexMode.NORMAL;

      public readonly StringBuilder StringCollector { get; set; } = new StringBuilder();

      /// <summary>
      /// Line source for the file/whatever we are reading.
      /// </summary>
      private readonly IEnumerator<string> lines;

      private readonly ImmutableDictionary<LispLexMode, LexerModeConf> tokenizerConf;

      private SourceInformation currentLoc;

      // Delegate these private variables to the source location structure. This
      // automatically keeps them in sync so that the currentLoc can be copied out at any
      // time. Since it's a struct, mutating it is safe and won't affect returned copies.
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

      private LexerModeConf LexConf => tokenizerConf[Mode];

      public IEnumerator<Token<LispTokType>> GetEnumerator () {
        while (AdvanceNextLine()) {
          foreach (var matcher in LexConf.Matchers) {
            Match match = matcher.Item1.Match(Line.Substring(ColumnIndex));
            // Try the next matcher if this one fails.
            if (!match.Success) continue;
            ColumnIndex += match.Value.Length;
            var val = matcher.Item2(this, match.Value, currentLoc);
            if (val == null) {
              // null means tha tthe token matched correctly, but did not produce a token
              // directly. Advance the lexer and try again.
              break;
            }
            yield return val;
          }
          throw ThrowSyntaxError("unrecognized input");
        }
      }

      /// <summary>
      /// Advance the lexer to the next line of the input if necessary, until we find a
      /// non-empty line. Does not change the state if there is still input left on the
      /// current line.
      /// </summary>
      /// <returns>
      /// True if we successfully advanced to a new line (or didn't need to),
      /// false if advancing failed or end of file.
      /// </returns>
      private bool AdvanceNextLine() {
        while (ColumnIndex >= Line.Length) {
          if (!LexConf.AllowLineBreaks) {
            // TODO(zstewar1): Better error messaging for this, maybe based on mode?
            throw ThrowSyntaxError("unexpected end-of-line.");
          }
          LineNumber += 1;
          ColumnIndex = 0;
          if (!lines.MoveNext()) {
            return false;
          }
          Line = lines.Current;
        }
        return true;
      }
    }
    #endregion Inner Stateful Class Implementation
  }
}
