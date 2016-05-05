using System;
using System.Collections.Generic;

namespace ZStewart.KOSLisp.Types.Helpers {
  /// <summary>
  /// Helpers for working with callables.
  /// </summary>
  public static class CallableOperations {
    /// <summary>
    /// Call a Lisp object as a function.
    /// </summary>
    /// <param name="callable">The object to call.</param>
    /// <param name="pargs">The positional arguments of the function call.</param>
    /// <param name="kwargs">The keyword arguments of the function call.</param>
    /// <returns></returns>
    public static LispObject Call(
        LispObject callable,
        List<LispObject> pargs,
        Dictionary<SymbolType, LispObject> kwargs) {
      return LookupHelpers.Lookup(
        callable, pargs,
        t => t.__call__ != null,
        t => t.__call__,
        PropConsts.Call,
        () => ExceptionType.CreateTypeError(
          "\"{0}\" object is not callable", callable.__class__));
    }

    public static bool IsCallable(LispObject callable) {
      return LookupHelpers.Query(callable, t => t.__call__ != null, PropConsts.Call);
    }
  }
}
