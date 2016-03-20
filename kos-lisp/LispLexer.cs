using System;
using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace ZStewart.KOSLisp {
  public class LispLexer : Lexer {
    public static readonly Mode NORMAL = new StringMode("NORMAL");
    public static readonly Mode STRING = new StringMode("STRING");
    public static readonly ImmutableList<Mode> Modes = 
      ImmutableList.Create<Mode>(NORMAL, STRING);

    private static readonly ImmutableDictionary<Mode, ImmutableList<Tuple<Regex, TokenCreator>>> tokenizerConf;

    static LispLexer () {
      ImmutableDictionary<Mode, ImmutableList<Tuple<Regex, TokenCreator>>>.Builder db = 
        ImmutableDictionary.CreateBuilder<Mode, ImmutableList<Tuple<Regex, TokenCreator>>>();
      db.Add(
        NORMAL,
        ImmutableList.Create<Tuple<Regex, TokenCreator>>(
          Tuple.Create(new Regex(@"^[\p{L}_-+*/][\p{L}_-+*/\d]*"), GenericToken<String>.CreateTokenCreator(x => x))
        )
      );
      db.Add(
        STRING,
        ImmutableList.Create<Tuple<Regex, TokenCreator>>(
        )
      );
      tokenizerConf = db.ToImmutable();
    }

    public LispLexer () {
    }

    public Tokenizer Lex (string source) {
      return null;
    }
  }
}

