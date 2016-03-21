using System.Collections.Generic;
using System.Linq;

namespace ZStewart.KOSLisp.Types {
  /// <summary>
  /// Helper functions for working with Lisp types.
  /// </summary>
  public static class LispTypeHelpers {
    /// <summary>
    /// Efficiently converts a list to a lisp list.
    /// </summary>
    /// <param name="list">The list to convert.</param>
    /// <returns>A lisp list with the same contents as the original list.</returns>
    public static LispList ToLispList(IList<LispObject> list) {
      LispList res = LispNil.Nil;
      for(int i = list.Count - 1; i >= 0; i++) {
        res = LispCons.Of(list[i], res);
      }
      return res;
    }
  }
}
