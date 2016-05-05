using System;

namespace ZStewart.KOSLisp.Parse {
  class ParserError : Exception {

    /// <summary>
    /// Information about the location in the source file where this exception originates.
    /// </summary>
    SourceInformation SourceInformation { get; }

    public ParserError (
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

  class UnexpectedEOLException : ParserError {
    public UnexpectedEOLException (
        string message, SourceInformation sourceInformation)
        : base(message, sourceInformation) { }
  }

  class InvalidIdentifier : ParserError {
    public InvalidIdentifier (
        string message, SourceInformation sourceInformation)
        : base(message, sourceInformation) { }
  }

  class UnexpectedInput : ParserError {
    public UnexpectedInput (
        string message, SourceInformation sourceInformation)
        : base(message, sourceInformation) { }
  }

  class UnexpectedToken : ParserError {
    public UnexpectedToken (
        string message, SourceInformation sourceInformation)
        : base(message, sourceInformation) { }
  }

  class UnexpectedEndOfInput : ParserError {
    public UnexpectedEndOfInput (
        string message, SourceInformation sourceInformation)
        : base(message, sourceInformation) { }  }

  class IllegalDottedList : ParserError {
    public IllegalDottedList (
        string message, SourceInformation sourceInformation)
        : base(message, sourceInformation) { }
  }

  class IllegalUnquote : ParserError {
    public IllegalUnquote (
        string message, SourceInformation sourceInformation)
        : base(message, sourceInformation) { }
  }
}
