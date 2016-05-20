namespace ZStewart.KOSLisp.Parse.Lisp {
  /// <summary>
  /// Enumeration of the various types of tokens available in this lisp.
  /// </summary>
  public enum LispTokType {
    // Normal Mode:
    // Symbols
    IDENTIFIER,
    NUMBER,
    STRING,

    // Syntax
    DOT,
    OPEN_PAREN,
    CLOSE_PAREN,
    QUOTE,
    BACKQUOTE,
    SPLICE,
    UNQUOTE,
  }
}
