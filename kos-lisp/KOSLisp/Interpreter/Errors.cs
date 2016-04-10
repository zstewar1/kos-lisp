using System;

namespace ZStewart.KOSLisp.Interpreter {
  /// <summary>
  /// Exceptions that occur when something goes wrong with the interpreter. Will not be handled by the interpreter.
  /// </summary>
  public class LispSystemException : Exception {
    public LispSystemException (string message) : base(message) { }
  }
}
