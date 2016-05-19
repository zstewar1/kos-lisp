using System.Collections.Generic;

namespace ZStewart.KOSLisp.Parse {
  /// <summary>
  /// Type which can be used to get tokens from a text stream.
  /// </summary>
  public interface Lexer<TokType> {
    /// <summary>
    /// Return an enumerator over the input source.
    /// </summary>
    IEnumerable<Token<TokType>> Lex (IEnumerable<string> source);
  }
}
