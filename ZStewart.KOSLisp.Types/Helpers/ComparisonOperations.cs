using ZStewart.KOSLisp.Types.Attributes;

namespace ZStewart.KOSLisp.Types.Helpers {
  public static class ComparisonOperations {
    private static readonly LispObject eqattr = SymbolType.Create("--eq--");
    private static readonly LispObject leattr = SymbolType.Create("--le--");
    private static readonly LispObject ltattr = SymbolType.Create("--lt--");
    private static readonly LispObject gtattr = SymbolType.Create("--gt--");
    private static readonly LispObject geattr = SymbolType.Create("--ge--");
    private static readonly LispObject hashattr = SymbolType.Create("--hash--");

    /// <summary>
    /// Returns true if the given lisp objects are equal. False otherwise. Null on error.
    /// </summary>
    /// <param name="target">First object to compare.</param>
    /// <param name="other">Second object to compare.</param>
    /// <returns>T/F</returns>
    public static LispObject Eq(
        [Required] this LispObject target,
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
        [Required] this LispObject target,
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
        [Required] this LispObject target,
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
        [Required] this LispObject target,
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
        [Required] this LispObject target,
        [Required] LispObject other) {
      return LookupHelpers.Lookup(
        target, other,
        t => t._comparison_methods != null && t._comparison_methods.__ge__ != null,
        t => t._comparison_methods.__ge__,
        geattr,
        () => ExceptionType.CreateTypeError(
          "\"{0}\" object is not comparable", target.__class__));
    }

    public static NumberType Hash(
        [Required] this LispObject target) {
      var val = LookupHelpers.Lookup(
        target,
        t => t._comparison_methods != null && t._comparison_methods.__hash__ != null,
        t => t._comparison_methods.__hash__,
        hashattr,
        () => ExceptionType.CreateTypeError(
          "\"{0}\" object is not hashable", target.__class__));
      if (!(val is NumberType)) {
        throw ExceptionType.ThrowTypeError(
          "result of hash must be a number, got {0}", val.__class__);
      }
      var num = (NumberType)val;
      if (num.Value % 1 != 0) {
        throw ExceptionType.ThrowValueError(
          "result of hash must be an integer, got {0}", num.Value);
      }
      return num;
    }

    public static BoolType Is(
        [Required] this LispObject first,
        [Required] LispObject second) {
      return BoolType.Create(ReferenceEquals(first, second));
    }
  }
}
