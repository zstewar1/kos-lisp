using static ZStewart.KOSLisp.Types.ExceptionType;
using static ZStewart.KOSLisp.Types.NotImplementedType;

using ZStewart.KOSLisp.Types.Attributes;

namespace ZStewart.KOSLisp.Types.Helpers {
  public static class ComparisonOperations {
    /// <summary>
    /// Returns true if the given lisp objects are equal. False otherwise. Null on error.
    /// </summary>
    /// <param name="target">First object to compare.</param>
    /// <param name="other">Second object to compare.</param>
    /// <returns>T/F</returns>
    public static LispObject Eq(
        [Required] LispObject target,
        [Required] LispObject other) {
      var res = EqInner(target, other);
      if (!res.RefEq(NotImplemented)) {
        return res;
      }
      res = EqInner(other, target);
      if (!res.RefEq(NotImplemented)) {
        return res;
      }
      // if neither supports comparisons to the other, they aren't equal.
      return BoolType.F;
    }

    /// <summary>
    /// Inner helper for repeated equality check inner operation.
    /// </summary>
    private static LispObject EqInner(LispObject a, LispObject b) {
      return LookupHelpers.Lookup(
        a, b,
        t => t._comparison_methods?.__eq__,
        PropConsts.Eq,
        () => NotImplemented);
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
      var res = LeInner(target, other);
      if (!res.RefEq(NotImplemented)) {
        return res;
      }
      res = GeInner(other, target);
      if (!res.RefEq(NotImplemented)) {
        return res;
      }
      throw ThrowTypeError(
        "incomparable types: {0}, {1}", target.__class__, other.__class__);
    }

    private static LispObject LeInner(LispObject a, LispObject b) {
      return LookupHelpers.Lookup(
        a, b,
        t => t._comparison_methods?.__le__,
        PropConsts.Le,
        () => NotImplemented);
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
      var res = LtInner(target, other);
      if (!res.RefEq(NotImplemented)) {
        return res;
      }
      res = GtInner(other, target);
      if (!res.RefEq(NotImplemented)) {
        return res;
      }
      throw ThrowTypeError(
        "incomparable types: {0}, {1}", target.__class__, other.__class__);
    }

    private static LispObject LtInner(LispObject a, LispObject b) {
      return LookupHelpers.Lookup(
        a, b,
        t => t._comparison_methods?.__lt__,
        PropConsts.Lt,
        () => NotImplemented);
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
      var res = GtInner(target, other);
      if (!res.RefEq(NotImplemented)) {
        return res;
      }
      res = LtInner(other, target);
      if (!res.RefEq(NotImplemented)) {
        return res;
      }
      throw ThrowTypeError(
        "incomparable types: {0}, {1}", target.__class__, other.__class__);
    }

    private static LispObject GtInner(LispObject a, LispObject b) {
      return LookupHelpers.Lookup(
        a, b,
        t => t._comparison_methods?.__gt__,
        PropConsts.Gt,
        () => NotImplemented);
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
      var res = GeInner(target, other);
      if (!res.RefEq(NotImplemented)) {
        return res;
      }
      res = LeInner(other, target);
      if (!res.RefEq(NotImplemented)) {
        return res;
      }
      throw ThrowTypeError(
        "incomparable types: {0}, {1}", target.__class__, other.__class__);
    }

    private static LispObject GeInner(LispObject a, LispObject b) {
      return LookupHelpers.Lookup(
        a, b,
        t => t._comparison_methods?.__ge__,
        PropConsts.Ge,
        () => NotImplemented);
    }

    public static NumberType Hash(
        [Required] LispObject target) {
      var val = LookupHelpers.Lookup(
        target,
        t => t._comparison_methods?.__hash__,
        PropConsts.Hash,
        () => ThrowTypeError(
          "\"{0}\" object is not hashable", target.__class__));
      if (val.RefEq(NotImplemented)) {
        throw ThrowTypeError(
          "\"{0}\" object is not hashable", target.__class__);
      }
      if (!(val is NumberType)) {
        throw ThrowTypeError(
          "result of hash must be a number, got {0}", val.__class__);
      }
      var num = (NumberType)val;
      if (num.Value % 1 != 0) {
        throw ThrowValueError(
          "result of hash must be an integer, got {0}", num.Value);
      }
      return num;
    }

    public static BoolType Is(
        [Required] LispObject first,
        [Required] LispObject second) {
      return BoolType.Create(ReferenceEquals(first, second));
    }

    /// <summary>
    /// A convenience method to more easily check reference equality.
    /// </summary>
    public static bool RefEq(this object first, object second) {
      return ReferenceEquals(first, second);
    }
  }
}
