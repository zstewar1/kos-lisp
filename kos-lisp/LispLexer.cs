using System;
using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace ZStewart.KOSLisp {

  /// <summary>
  /// A lexer definition for lexing kOS Lisp.
  /// </summary>
  public class LispLexer : Lexer<LispTokType, LispLexMode> {

    /// <summary>
    /// An exception that indicates a state in the lexer, not an error.
    /// </summary>
    class LexerSigil : Exception { }
    /// <summary>
    /// A Lexer Sigil for when the end of the line is reached.
    /// </summary>
    class EOLSigil : LexerSigil { }
    /// <summary>
    /// A Lexer Sigil for when the end of the file is reached.
    /// </summary>
    class EOFSigil : LexerSigil { }

    /// <summary>
    /// A Lexer Sigil for when text is skipped.
    /// </summary>
    class SkipTextSigil : LexerSigil { }

    /// <summary>
    /// Special case of skip-text for when the text skipped is a comment.
    /// </summary>
    class CommentSigil : SkipTextSigil { }

    /// <summary>
    /// The configuration of the Lexer -- this is the set of modes and regexes used for parsing.
    /// </summary>
    private readonly ImmutableDictionary<LispLexMode, ImmutableList<Tuple<Regex, TokenCreator<LispTokType>>>> tokenizerConf;

    /// <summary>
    /// Regex mode options to be used in the lexer.
    /// </summary>
    private readonly RegexOptions regexOptions = RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase;

    /// <summary>
    /// Creates a regex matcher for the given regular expression.
    /// </summary>
    /// <param name="matcher">The regular expression to match against. This will have the ^ anchor prepended to it.</param>
    /// <param name="createFunc">The token-creation function to use for this regex.</param>
    /// <returns>A regex, token creator match line for the given regex and create func.</returns>
    private Tuple<Regex, TokenCreator<LispTokType>> CreateMatcher (
        string matcher, TokenCreator<LispTokType> createFunc) {
      return Tuple.Create(
        new Regex("^" + matcher, regexOptions),
        createFunc
      );
    }

    public LispLexer () {
      // Initialize the tokenizer configuration for lisp lexers.
      ImmutableDictionary<LispLexMode, ImmutableList<Tuple<Regex, TokenCreator<LispTokType>>>>.Builder db =
        ImmutableDictionary.CreateBuilder<LispLexMode, ImmutableList<Tuple<Regex, TokenCreator<LispTokType>>>>();
      db.Add(
        LispLexMode.NORMAL,
        ImmutableList.Create(
          CreateMatcher(@"[+-]?[0-9]*\.[0-9]+(e[+-]?[0-9]+)?", GenericToken.CreateTokenCreator(LispTokType.FLOAT, double.Parse)),
          CreateMatcher(@"[+-]?[0-9]+", GenericToken.CreateTokenCreator(LispTokType.INT, long.Parse)),
          CreateMatcher(@"[\p{L}_-+*/\.:][\p{L}_-+*/\.:\d]*", RawToken.CreateTokenCreator(LispTokType.IDENTIFIER)),
          CreateMatcher(@"(", RawToken.CreateTokenCreator(LispTokType.OPEN_PAREN)),
          CreateMatcher(@")", RawToken.CreateTokenCreator(LispTokType.CLOSE_PAREN)),
          CreateMatcher(@"'", RawToken.CreateTokenCreator(LispTokType.QUOTE)),
          CreateMatcher("\"", RawToken.CreateTokenCreator(LispTokType.STARTSTRING)),
          CreateMatcher(@";.*", (rv, i, l, c) => { throw new CommentSigil(); }),
          CreateMatcher(@"$", (rv, i, l, c) => { throw new EOLSigil(); }),
          CreateMatcher(@"\s", (rv, i, l, c) => { throw new SkipTextSigil(); })
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
            @"$", (rv, index, line, column) => {
              throw new UnexpectedEOLException("Unexpected end of line while parsing string.", index, line, column);
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

    public Tokenizer<LispTokType, LispLexMode> Lex (string source) {
      return new LispTokenizer(source, this);
    }

    /// <summary>
    /// Implementation of the tokenizer for the lisp lexer.
    /// </summary>
    private class LispTokenizer : Tokenizer<LispTokType, LispLexMode> {
      public string Source { get; }

      private readonly LispLexer conf;
      private int index = 0;
      private int line = 0, column = 0;
      private LispLexMode mode = LispLexMode.NORMAL;

      internal LispTokenizer (string source, LispLexer conf) {
        this.Source = source;
        this.conf = conf;
      }

      public Token<LispTokType> Next () {
        var lexList = conf.tokenizerConf[mode];
        return NextToken(lexList);
      }

      public void SwitchMode (LispLexMode mode) {
        this.mode = mode;
      }

      private Token<LispTokType> NextToken (ImmutableList<Tuple<Regex, TokenCreator<LispTokType>>> lexList) {
        retry_match:
        foreach (var matcher in lexList) {
          Match match = matcher.Item1.Match(Source, index);
          // Try the next matcher if this one fails.
          if (!match.Success) continue;
          try {
            var val = matcher.Item2(match.Value, index, line, column);
            index += match.Length;
            column += match.Length;
            return val;
          } catch (EOLSigil) {
            index += match.Length;
            line += 1;
            column = 0;
            goto retry_match;
          } catch (SkipTextSigil) {
            column += match.Length;
            index += match.Length;
            goto retry_match;
          } catch (LexerSigil sig) {
            throw new UnexpectedSigil(string.Format("Unexpected lexer sigil {0}", sig), index, line, column);
          }
        }
        throw new UnexpectedInput(string.Format("Unrecognized input"), index, line, column);
      }
    }
  }
}
