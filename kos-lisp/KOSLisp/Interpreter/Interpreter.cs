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
    #region Singletonness
    private static LispInterpreter _instance;
    public static LispInterpreter Instance {
      get {
        if (_instance != null) return _instance;

        _instance = new LispInterpreter();
        return _instance;
      }
    }
    private LispInterpreter () { }
    #endregion Singletonness

    /// <summary>
    /// The currently set exception.
    /// </summary>
    private ExceptionType exception = null;

    /// <summary>
    /// Checks the current exception value.
    /// </summary>
    public ExceptionType Exception { get { return exception; } }

    /// <summary>
    /// Tells whether there is a currently set exception.
    /// </summary>
    public bool ExceptionOccurred { get { return exception != null; } }

    /// <summary>
    /// Resets the exception status
    /// </summary>
    public void ClearException() {
      exception = null;
    }

    public void SetExceptionString(
        LispTypeObject exceptionType, string message, params object[] args) {
      throw new NotImplementedException();
      // TODO(zstewar1): Setting errors.
    }
  }
}
