using System.Collections.Generic;

namespace ZStewart.KOSLisp.Types {
  /// <summary>
  /// The delegate type used for buitin __call__ methods.
  /// </summary>
  public delegate LispObject CallFunc(
      LispObject self,
      List<LispObject> pargs,
      Dictionary<SymbolType, LispObject> kwargs);
}
