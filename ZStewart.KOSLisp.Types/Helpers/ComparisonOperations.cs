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
    /// <returns>T/F</returns>
    public static LispObject Eq(
        [Required] LispObject target,
        [Required] LispObject other) {
      return LookupHelpers.Lookup(
        target, other,
        t => t._comparison_methods != null && t._comparison_methods.__eq__ != null,
        t => t._comparison_methods.__eq__,
        eqattr,
        () => ExceptionType.CreateTypeError(
          "\"{0}\" object is not comparable", target.__class__));
    }

    /// <summary>
    /// Returns true if the first object is less than or equal to the second. False
    /// otherwise. Null on error.
    /// </summary>
    /// <param name="target">First object to compare.</param>
    /// <param name="other">Second object to compare.</param>
    /// <returns>T/F</returns>
    public static LispObject Le(
        [Required] LispObject target,
        [Required] LispObject other) {
      return LookupHelpers.Lookup(
        target, other,
        t => t._comparison_methods != null && t._comparison_methods.__le__ != null,
        t => t._comparison_methods.__le__,
        leattr,
        () => ExceptionType.CreateTypeError(
          "\"{0}\" object is not comparable", target.__class__));
    }

    /// <summary>
    /// Returns true if the first object is less than the second. False otherwise. Null on
    /// error.
    /// </summary>
    /// <param name="target">First object to compare.</param>
    /// <param name="other">Second object to compare.</param>
    /// <returns>T/F</returns>
    public static LispObject Lt(
        [Required] LispObject target,
        [Required] LispObject other) {
      return LookupHelpers.Lookup(
        target, other,
        t => t._comparison_methods != null && t._comparison_methods.__lt__ != null,
        t => t._comparison_methods.__lt__,
        ltattr,
        () => ExceptionType.CreateTypeError(
          "\"{0}\" object is not comparable", target.__class__));
    }

    /// <summary>
    /// Returns true if the first object is greater than the second. False otherwise. Null
    /// on error.
    /// </summary>
    /// <param name="target">First object to compare.</param>
    /// <param name="other">Second object to compare.</param>
    /// <returns>T/F</returns>
    public static LispObject Gt(
        [Required] LispObject target,
        [Required] LispObject other) {
      return LookupHelpers.Lookup(
        target, other,
        t => t._comparison_methods != null && t._comparison_methods.__gt__ != null,
        t => t._comparison_methods.__gt__,
        gtattr,
        () => ExceptionType.CreateTypeError(
          "\"{0}\" object is not comparable", target.__class__));
    }

    /// <summary>
    /// Returns true if the first object is greater than or equal to the second. False
    /// otherwise. Null on error.
    /// </summary>
    /// <param name="target">First object to compare.</param>
    /// <param name="other">Second object to compare.</param>
    /// <returns>T/F</returns>
    public static LispObject Ge(
        [Required] LispObject target,
        [Required] LispObject other) {
      return LookupHelpers.Lookup(
        target, other,
        t => t._comparison_methods != null && t._comparison_methods.__ge__ != null,
        t => t._comparison_methods.__ge__,
        geattr,
        () => ExceptionType.CreateTypeError(
          "\"{0}\" object is not comparable", target.__class__));
    }

    // TODO(zstewar1): The rest of the comparison operations. (le,lt,gt,ge,hash,is)
  }
}
