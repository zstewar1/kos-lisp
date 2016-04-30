using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZStewart.KOSLisp.Types;

namespace ZStewart.KOSLisp.Interpreter {
  /// <summary>
  /// Class storing the state of the interpreter as it executes code.
  ///
  /// This is a singleton currently.
  /// </summary>
  public static class LispInterpreter {
    #region Exception Handling
    /// <summary>
    /// The currently set exception.
    /// </summary>
    private static ExceptionType exception = null;

    /// <summary>
    /// Checks the current exception value.
    /// </summary>
    public static ExceptionType Exception { get { return exception; } }

    /// <summary>
    /// Tells whether there is a currently set exception.
    /// </summary>
    public static bool ExceptionOccurred { get { return exception != null; } }

    /// <summary>
    /// Resets the exception status
    /// </summary>
    public static void ClearException() {
      exception = null;
    }

    /// <summary>
    /// Checks if the set exception is of the given type.
    /// Explodes if there is no exception set.
    /// May also explode if stuff is too messed up.
    /// </summary>
    public static bool CheckException(LispType type) {
      if (exception == null)
        throw new InvalidOperationException(
          "Cannot check exception type -- no exception");
      // TODO(zstewar1): Save the exception and throw a new exception if there's an
      // exception while exception handling.

      // Because setting an exception fails if there is already an exception, this call
      // will just auto-explode if there is an error while type-checking. This may not be
      // the desired final behavior, but for now it does mean we're pretty much guaranteed
      // that instance != null.
      var instance = LispType.IsInstance(exception, type);
      return instance.Value;
    }

    /// <summary>
    /// Sets the exception to the given exception. Guaranteed to always set *an*
    /// exception, though it can set a different exception if there is an error with
    /// setting the given one.
    /// </summary>
    public static void SetException(LispObject exc) {
      if (exception != null) {
        throw new InvalidOperationException(
            string.Format(
              "Attempting to throw new exception:\n{0}\nbut the exception was already " +
              "set to:\n{1}.", exc, exception));
      }
      var instance = LispType.IsInstance(exc, ExceptionType.Exception);
      // If there is no value, then just keep the exception set by isinstance.
      if (instance.HasValue) {
        if (instance.Value) {
          exception = (ExceptionType)exc;
        } else {
          exception = ExceptionType.CreateTypeError(
            "Exceptions must derive from Exception");
        }
      }
    }

    /// <summary>
    /// Retrieve and clear the exception. Raises an error if no exception is set.
    /// </summary>
    public static ExceptionType SaveException() {
      if (exception == null)
        throw new InvalidOperationException("Cannot save exception -- no exception");
      var exc = exception;
      exception = null;
      return exc;
    }
    #endregion Exception Handling

    #region Builtins Module
    private static ModuleType _builtins;
    public static ModuleType Builtins {
      get {
        if (_builtins != null) return _builtins;

        _builtins = ModuleType.Create(SymbolType.Create("builtins"));

        return _builtins;
      }
    }
    #endregion
  }
}
