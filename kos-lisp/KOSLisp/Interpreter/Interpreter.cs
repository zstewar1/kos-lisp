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
    private readonly static LispInterpreter instance = new LispInterpreter();
    public static LispInterpreter Instance { get { return instance; } }
    private LispInterpreter () { }
    #endregion Singletonness

    /// <summary>
    /// The currently set exception.
    /// </summary>
    private LispObject exception = null;

    public LispObject ErrorOccurred() {
      return exception;
    }

    public void SetErrorString(
        LispObject errorType, string message, params object[] args) {
      throw new NotImplementedException();
      // TODO(zstewar1): Setting errors.
    }
  }
}
