using ZStewart.KOSLisp.Interpreter;
using ZStewart.KOSLisp.Types.Attributes;

namespace ZStewart.KOSLisp.Types.Helpers {
  public static class ComparisonOperations {
    private static readonly LispObject eqattr = SymbolType.Create("--eq--");
    private static readonly LispObject leattr = SymbolType.Create("--le--");
    private static readonly LispObject ltattr = SymbolType.Create("--lt--");
    private static readonly LispObject gtattr = SymbolType.Create("--gt--");
    private static readonly LispObject geattr = SymbolType.Create("--ge--");

    /// <summary>
    /// Returns true if the given lisp objects are equal. False otherwise. Null on error.
    /// </summary>
    /// <param name="target">First object to compare.</param>
    /// <param name="other">Second object to compare.</param>
    /// <returns>T/F/null</returns>
    public static LispObject Eq(
        [PositionalArgument] LispObject target, 
        [PositionalArgument] LispObject other) {
      foreach (var targetType in ListOperations.IterMro(target)) {
        if (targetType == null) return null;
        if (targetType._comparison_methods != null
            && targetType._comparison_methods.__eq__ != null) {
          return targetType._comparison_methods.__eq__(target, other);
        } else {
          LispObject __eq__ = MappingOperations.GetItem(targetType.__dict__, eqattr);
          if (__eq__ == null) {
            if (LispInterpreter.CheckException(ExceptionType.KeyError))
              LispInterpreter.ClearException();
            else return null;
          } else {
            return CallableOperations.Call(
              __eq__, IConsType.ToLispTuple(target, other));
          }
        }
      }
      LispInterpreter.SetException(ExceptionType.CreateTypeError(
        "\"{0}\" object is not comparable", target.__class__));
      return null;
    }

    // TODO(zstewar1): The rest of the comparison operations. (le,lt,gt,ge,hash,is)
  }
}
