using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ZStewart.KOSLisp.Interpreter;

namespace ZStewart.KOSLisp.Types.Helpers {
  /// <summary>
  /// Helpers for working with callables.
  /// </summary>
  public static class CallableOperations {
    private static readonly LispObject callattr = SymbolType.Create("--call--");

    /// <summary>
    /// Call a Lisp object as a function.
    /// </summary>
    /// <param name="callable">The object to call.</param>
    /// <param name="args">The arguments to the callable.</param>
    /// <returns></returns>
    public static LispObject Call(LispObject callable, LispObject args) {
      args = IConsType.Copy(args);
      if (args == null) return null;
      foreach (var targetType in ListOperations.IterMro(callable)) {
        // Propagate errors.
        if (targetType == null) return null;
        if (targetType.__call__ != null) {
          return targetType.__call__(callable, args);
        } else {
          LispObject __call__ = MappingOperations.GetItem(
            targetType.__dict__, callattr);
          if (__call__ == null) {
            if (LispInterpreter.CheckException(ExceptionType.KeyError))
              LispInterpreter.ClearException();
            else return null;
          } else {
            return Call(__call__, IConsType.Create(callable, args));
          }
        }
      }
      LispInterpreter.SetException(ExceptionType.CreateTypeError(
        "\"{0}\" object is not callable", callable.__class__));
      return null;
    }
  }
}
