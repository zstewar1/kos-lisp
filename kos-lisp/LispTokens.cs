namespace ZStewart.KOSLisp {
  /// <summary>
  /// Enumeration of the various types of tokens available in this lisp.
  /// </summary>
  public enum LispTokType {
    // Normal Mode:
    // Symbols
    IDENTIFIER,
    FLOAT,
    INT,

    // Syntax
    OPEN_PAREN,
    CLOSE_PAREN,
    QUOTE,
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