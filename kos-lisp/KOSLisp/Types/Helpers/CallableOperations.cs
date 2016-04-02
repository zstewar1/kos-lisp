using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZStewart.KOSLisp.Types.Helpers {
  /// <summary>
  /// Helpers for working with callables.
  /// </summary>
  public static class CallableOperations {
    private static readonly LispObject callattr = StringType.Create("--call--");

    /// <summary>
    /// Call a Lisp object as a function.
    /// </summary>
    /// <param name="callable">The object to call.</param>
    /// <param name="args">The arguments to the callable.</param>
    /// <returns></returns>
    public static LispObject Call(LispObject callable, LispObject args) {
      args = IConsType.AsICons(args);
      if (args == null) return null;
      LispTypeObject targetType = callable.__class__;
      LispObject __mro__ = targetType.__mro__;
      for (;;) {
        if (targetType.__call__ != null) {
          return targetType.__call__(callable, args);
        } else {
          LispObject __call__ = MappingOperations.GetItem(
            targetType.__dict__, callattr);
          if (__call__ == null) {
            // TODO(zstewar1): Check the error. Continue for KeyError, abort for all else.
          } else {
            return Call(__call__, IConsType.Create(callable, args));
          }
        }
        if (__mro__ == NilType.Nil) {
          // TODO(zstewar1): __call__ not found error.
          return null;
        }
        LispObject nextType = ListOperations.GetCar(__mro__);
        if (nextType == null) return null; // Propagate errors.
        targetType = nextType as LispTypeObject;
        if (targetType == null) {
          // TODO(zstewar1): set a type error: types must be type type. (This should be
          // impossible anyway, since we should prevent setting arbitrary types).
          return null;
        }
        __mro__ = ListOperations.GetCdr(__mro__);
        if (__mro__ == null) return null; // Propagate errors.      
      }
    }
  }
}
