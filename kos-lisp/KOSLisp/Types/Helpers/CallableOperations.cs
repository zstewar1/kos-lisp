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
            // TODO(zstewar1): Check the error. Continue for KeyError, abort for all else.
          } else {
            return Call(__call__, IConsType.Create(callable, args));
          }
        }
      }
      // TODO(zstewar1): __call__ not found error.
      return null;
    }
  }
}
