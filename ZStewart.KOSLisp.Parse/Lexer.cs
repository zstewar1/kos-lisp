using System.Collections.Generic;

namespace ZStewart.KOSLisp.Parse {
  /// <summary>
  /// Type which can be used to get tokens from a text stream.
  /// </summary>
  public interface Lexer<out TokType> {
    /// <summary>
    /// Return an enumerator over the input source.
    /// </summary>
    /// <param name="sourceName">
    /// The name of the source this lexer is reading from.
    /// </param>
    /// <param name="source">
    /// An iterator over the lines of the file to read. This is an enumerable of string to
    /// give implementor more flexibility rather than requiring something more strict like
    /// a TextReader.
    /// </param>
    IEnumerable<Token<TokType>> Lex (string sourceName, IEnumerable<string> source);
  }
}
