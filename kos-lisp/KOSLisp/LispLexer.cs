using System;
using System.IO;
using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace ZStewart.KOSLisp {

  /// <summary>
  /// A lexer definition for lexing kOS Lisp.
  /// </summary>
  public class LispLexer {

    #region Static Properties
    /// <summary>
    /// The configuration of the Lexer -- this is the set of modes and regexes used for parsing.
    /// </summary>
    private static readonly ImmutableDictionary<LispLexMode, ImmutableList<Tuple<Regex, TokenCreator<LispTokType>>>> tokenizerConf;

    /// <summary>
    /// Regex mode options to be used in the lexer.
    /// </summary>
    private static readonly RegexOptions regexOptions = RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase;
    #endregion //Static Properties

    #region Static Setup
    /// <summary>
    /// Creates a regex matcher for the given regular expression.
    /// </summary>
    /// <param name="matcher">The regular expression to match against. This will have the ^ anchor prepended to it.</param>
    /// <param name="createFunc">The token-creation function to use for this regex.</param>
    /// <returns>A regex, token creator match line for the given regex and create func.</returns>
    private static Tuple<Regex, TokenCreator<LispTokType>> CreateMatcher (
        string matcher, TokenCreator<LispTokType> createFunc) {
      return Tuple.Create(
        new Regex("^" + matcher, regexOptions),
        createFunc
      );
    }

    static LispLexer () {
      // Initialize the tokenizer configuration for lisp lexers.
      ImmutableDictionary<LispLexMode, ImmutableList<Tuple<Regex, TokenCreator<LispTokType>>>>.Builder db =
        ImmutableDictionary.CreateBuilder<LispLexMode, ImmutableList<Tuple<Regex, TokenCreator<LispTokType>>>>();
      db.Add(
        LispLexMode.NORMAL,
        ImmutableList.Create(
          CreateMatcher(@"[+-]?[0-9]*\.[0-9]+(e[+-]?[0-9]+)?", GenericToken.CreateTokenCreator(LispTokType.FLOAT, double.Parse)),
          CreateMatcher(@"[+-]?[0-9]+", GenericToken.CreateTokenCreator(LispTokType.INT, long.Parse)),
          CreateMatcher(@"\.", RawToken.CreateTokenCreator(LispTokType.DOT)),
          CreateMatcher(@"nil", RawToken.CreateTokenCreator(LispTokType.NIL)),
          CreateMatcher(@"t", RawToken.CreateTokenCreator(LispTokType.T)),
          CreateMatcher(@"[\p{L}_+*/\-\.:\d]+", RawToken.CreateTokenCreator(LispTokType.IDENTIFIER)),
          CreateMatcher(@"\(", RawToken.CreateTokenCreator(LispTokType.OPEN_PAREN)),
          CreateMatcher(@"\)", RawToken.CreateTokenCreator(LispTokType.CLOSE_PAREN)),
          CreateMatcher(@"'", RawToken.CreateTokenCreator(LispTokType.QUOTE)),
          CreateMatcher("\"", RawToken.CreateTokenCreator(LispTokType.STARTSTRING)),
          CreateMatcher(@";.*", (rv, i, l, c) => null),
          CreateMatcher(@"\s", (rv, i, l, c) => null)
        )
      );
      db.Add(
        LispLexMode.STRING,
        ImmutableList.Create(
          CreateMatcher(
            @"\\[rn""\\]", GenericToken.CreateTokenCreator(
              LispTokType.CHARACTER, escape => {
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
              }
            )
          ),
          CreateMatcher(
            @"$", (rv, sourceLine, line, column) => {
              throw new UnexpectedEOLException("Unexpected end of line while parsing string.", sourceLine, line, column);
            }
          ),
          CreateMatcher("\"", RawToken.CreateTokenCreator(LispTokType.ENDSTRING)),
          CreateMatcher(
            @".", GenericToken.CreateTokenCreator(
              LispTokType.CHARACTER, c => {
                Preconditions.CheckArgument(c.Length == 1, "Character match must be of length 1.");
                return c[0];
              }
            )
          )
        )
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
      var lexList = tokenizerConf[mode];

      retry_match:
      while (columnIndex >= line.Length) {
        lineNumber += 1;
        columnIndex = 0;
        line = source.ReadLine();
        if (line == null) {
          return null;
        }
      }
      foreach (var matcher in lexList) {
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
