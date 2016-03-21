namespace ZStewart.Compilers {
  /// <summary>
  /// A type that can parse a sequence of tokens from a tokenizer into a parse tree.
  /// </summary>
  /// <typeparam name="TokType">The type of token this parser takes.</typeparam>
  /// <typeparam name="Mode">The mode type that the parser uses.</typeparam>
  public interface Parser<in TokType, out Mode> {
    CodeGenerator Parse (Tokenizer<TokType, Mode> tokenizer);
  }

  public interface CodeGenerator {
    string Generate ();
  }
}
