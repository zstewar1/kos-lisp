using System.Collections.Generic;

using ZStewart.KOSLisp.Types;

namespace ZStewart.KOSLisp.Parse {
  /// <summary>
  /// Type which can be used to get lisp expressions from some source.
  /// </summary>
  public interface Parser<in TokType> {
    /// <summary>
    /// Returns an iterator which can be used read expressions from some lisp object
    /// source.
    ///
    /// The returned Enumerable should be lazy and only read from the source when MoveNext
    /// is called.
    /// </summary>
    /// <param name="source">
    /// </param>
    /// <returns>
    /// A lazy iterator which reads lisp objects from the source as needed.
    /// </returns>
    IEnumerable<LispObject> Parse(IEnumerable<Token<TokType>> source);
  }
}
