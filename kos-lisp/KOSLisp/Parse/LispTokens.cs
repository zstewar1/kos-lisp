namespace ZStewart.KOSLisp.Parse {
  /// <summary>
  /// Enumeration of the various types of tokens available in this lisp.
  /// </summary>
  public enum LispTokType {
    // Normal Mode:
    // Symbols
    IDENTIFIER,
    NUMBER,

    // Syntax
    DOT,
    OPEN_PAREN,
    CLOSE_PAREN,
    QUOTE,
    BACKQUOTE,
    SPLICE,
    UNQUOTE,
    STARTSTRING,

    // String Mode
    ENDSTRING,
    CHARACTER,
  }

  /// <summary>
  /// Enumeration of the various matcher modes available in this lisp.
  /// </summary>
  public enum LispLexMode {
    NORMAL,
    STRING,
  }
}