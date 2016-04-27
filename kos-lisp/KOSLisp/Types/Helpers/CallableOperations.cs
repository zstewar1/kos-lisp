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
      return LookupHelpers.Lookup(
        callable, args,
        t => t.__call__ != null,
        t => t.__call__,
        callattr,
        () => IConsType.Create(callable, args),
        () => ExceptionType.CreateTypeError(
          "\"{0}\" object is not callable", callable.__class__));
    }
  }
}
