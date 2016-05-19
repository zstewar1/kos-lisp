using System.Collections.Generic;

using ZStewart.KOSLisp.Types;

namespace ZStewart.KOSLisp.Parse {
  /// <summary>
  /// Type which can be used to get lisp expressions from some source.
  /// </summary>
  public interface Parser {
    /// <summary>
    /// Returns an iterator which can be used read expressions from some lisp object
    /// source.
    ///
    /// The returned Enumerable should be lazy and only read from the source when MoveNext
    /// is called.
    /// </summary>
    /// <param name="source">
    /// An iterator over the lines of the file to read. This is an enumerable of string to
    /// give implementor more flexibility rather than requiring something more strict like
    /// a TextReader.
    /// </param>
    /// <returns>
    /// A lazy iterator which reads lisp objects from the source as needed.
    /// </returns>
    IEnumerable<LispObject> Parse(IEnumerable<string> source);
  }
}
