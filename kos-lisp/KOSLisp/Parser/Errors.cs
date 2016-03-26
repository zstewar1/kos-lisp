using System;

namespace ZStewart.KOSLisp.Parser {
  class LispCompileException : Exception {
    /// <summary>
    /// Index in the string where the error occured.
    /// </summary>
    public string SourceLine { get; }

    /// <summary>
    /// The line where the error occured. Counting from 1 = first line.
    /// </summary>
    public int LineNumber { get; }

    /// <summary>
    /// The column where the error occured. Counting from 0 = first character.
    /// </summary>
    public int ColumnIndex { get; }

    public LispCompileException (
        string message, string sourceLine, int lineNumber, int columnIndex) 
        : base(message) {
      SourceLine = sourceLine;
      LineNumber = lineNumber;
      ColumnIndex = columnIndex;
    }
    public LispCompileException (string message, Token<LispTokType> srcToken)
      : this(message, srcToken.SourceLine, srcToken.LineNumber, srcToken.ColumnIndex) { }

    public override string ToString () {
      return string.Format(
        "{0}: {1}\nSourceLine={2}, LineNumber={3}, ColumnIndex={4}",
        this.GetType(), Message, SourceLine, LineNumber, ColumnIndex);
    }
  }

  class UnexpectedEOLException : LispCompileException {
    public UnexpectedEOLException (
        string message, string sourceLine, int lineNumber, int columnIndex) 
        : base(message, sourceLine, lineNumber, columnIndex) { }
    public UnexpectedEOLException (
        string message, Token<LispTokType> srcToken)
        : base(message, srcToken) { }
  }

  class InvalidIdentifier : LispCompileException {
    public InvalidIdentifier (
        string message, string sourceLine, int lineNumber, int columnIndex) 
        : base(message, sourceLine, lineNumber, columnIndex) { }
    public InvalidIdentifier (string message, Token<LispTokType> srcToken)
      : base(message, srcToken) { }
  }

  class UnexpectedInput : LispCompileException {
    public UnexpectedInput (
        string message, string sourceLine, int lineNumber, int columnIndex) 
        : base(message, sourceLine, lineNumber, columnIndex) { }
    public UnexpectedInput (string message, Token<LispTokType> srcToken)
      : base(message, srcToken) { }
  }

  class UnexpectedToken : LispCompileException {
    public UnexpectedToken (
        string message, string sourceLine, int lineNumber, int columnIndex) 
        : base(message, sourceLine, lineNumber, columnIndex) { }
    public UnexpectedToken (string message, Token<LispTokType> srcToken)
      : base(message, srcToken) { }
  }

  class UnexpectedEndOfInput : LispCompileException {
    public UnexpectedEndOfInput (
        string message, string sourceLine, int lineNumber, int columnIndex) 
        : base(message, sourceLine, lineNumber, columnIndex) { }
    public UnexpectedEndOfInput (string message, Token<LispTokType> srcToken)
      : base(message, srcToken) { }
  }

  class IllegalDottedList : LispCompileException {
    public IllegalDottedList (
        string message, string sourceLine, int lineNumber, int columnIndex) 
        : base(message, sourceLine, lineNumber, columnIndex) { }
    public IllegalDottedList (string message, Token<LispTokType> srcToken)
      : base(message, srcToken) { }
  }

  class IllegalUnquote : LispCompileException {
    public IllegalUnquote (
        string message, string sourceLine, int lineNumber, int columnIndex) 
        : base(message, sourceLine, lineNumber, columnIndex) { }
    public IllegalUnquote (string message, Token<LispTokType> srcToken)
      : base(message, srcToken) { }
  }
}
