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
  public sealed class LispInterpreter {
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
    public static bool CheckException(LispTypeObject type) {
      if (exception == null)
        throw new InvalidOperationException(
          "Cannot check exception type -- no exception");
      // Because setting an exception fails if there is already an exception, this call
      // will just auto-explode if there is an error while type-checking. This may not be
      // the desired final behavior, but for now it does mean we're pretty much guaranteed
      // that instance != null.
      var instance = TypeType.IsInstance(exception, type);
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
      var instance = TypeType.IsInstance(exc, ExceptionType.Exception);
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
  }
}
