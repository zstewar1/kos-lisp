using ZStewart.KOSLisp.Types;

namespace ZStewart.KOSLisp.Parse {
  /// <summary>
  /// Type which can be used to get lisp expressions from some source.
  /// </summary>
  public interface Parser {
    /// <summary>
    /// Reads the next whole expression and returns the expression as a lisp object,
    /// usually a "primitive" type or cons-list of "primitive" types. Here "primitive"
    /// means, generally, simple types that have a syntactic representation in the lexer,
    /// e.g. numbers, strings, and symbols.
    ///
    /// Returns null when the end-of-input has been reached.
    /// </summary>
    LispObject ParseNext();
  }
}
