using System;

namespace ZStewart.KOSLisp.Interpreter {
  /// <summary>
  /// Exceptions that can occur within the interpreter. Should be handled by the interpreter.
  /// </summary>
  public class LispException : Exception {
    public LispException (string message) : base(message) { }
  }
  /// <summary>
  /// Thrown when something illegal happens in a lisp function.
  /// </summary>
  public class InvalidOperationException : LispException {
    public InvalidOperationException (string message) : base(message) { }
  }
  /// <summary>
  /// Thrown when a lisp function encounters an invalid argument.
  /// </summary>
  public class IllegalArgumentException : LispException {
    public IllegalArgumentException (string message) : base(message) { }
  }

  /// <summary>
  /// Exceptions that occur when something goes wrong with the interpreter. Will not be handled by the interpreter.
  /// </summary>
  public class LispSystemException : Exception {
    public LispSystemException (string message) : base(message) { }
  }
}
