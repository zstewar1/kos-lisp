using System;

namespace ZStewart.KOSLisp.Parser {
  class LispCompileException : Exception {

    /// <summary>
    /// Information about the location in the source file where this exception originates.
    /// </summary>
    SourceInformation SourceInformation { get; }

    public LispCompileException (
        string message, SourceInformation sourceInformation)
        : base(message) {
      SourceInformation = sourceInformation;
    }

    public override string ToString () {
      return string.Format(
        "{0}: {1}\n{2}",
        this.GetType(), Message, SourceInformation);
    }
  }

  class UnexpectedEOLException : LispCompileException {
    public UnexpectedEOLException (
        string message, SourceInformation sourceInformation)
        : base(message, sourceInformation) { }
  }

  class InvalidIdentifier : LispCompileException {
    public InvalidIdentifier (
        string message, SourceInformation sourceInformation)
        : base(message, sourceInformation) { }
  }

  class UnexpectedInput : LispCompileException {
    public UnexpectedInput (
        string message, SourceInformation sourceInformation)
        : base(message, sourceInformation) { }
  }

  class UnexpectedToken : LispCompileException {
    public UnexpectedToken (
        string message, SourceInformation sourceInformation)
        : base(message, sourceInformation) { }
  }

  class UnexpectedEndOfInput : LispCompileException {
    public UnexpectedEndOfInput (
        string message, SourceInformation sourceInformation)
        : base(message, sourceInformation) { }  }

  class IllegalDottedList : LispCompileException {
    public IllegalDottedList (
        string message, SourceInformation sourceInformation)
        : base(message, sourceInformation) { }
  }

  class IllegalUnquote : LispCompileException {
    public IllegalUnquote (
        string message, SourceInformation sourceInformation)
        : base(message, sourceInformation) { }
  }
}
