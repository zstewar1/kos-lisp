using static ZStewart.KOSLisp.Types.ExceptionType;
using static ZStewart.KOSLisp.Types.NotImplementedType;

using ZStewart.KOSLisp.Types.Attributes;

namespace ZStewart.KOSLisp.Types.Helpers {
  public static class NumberOperations {
    /// <summary>
    /// Returns the result of adding the given lisp objects.
    /// </summary>
    public static LispObject Add(
        [Required] LispObject target,
        [Required] LispObject other) {
      var res = LookupHelpers.Lookup(
        target, other, null, SymbolType.Create("--add--"), () => NotImplemented);
      if (!ReferenceEquals(res, NotImplemented)) {
        return res;
      }
      res = LookupHelpers.Lookup(
        other, target, null, SymbolType.Create("--radd--"), () => NotImplemented);
      if (!ReferenceEquals(res, NotImplemented)) {
        return res;
      }
      throw ThrowTypeError(
        "unsupported operands for add: {0} and {1}", target.__class__, other.__class__);
    }

    /// <summary>
    /// Returns the result of subtracting the given lisp objects.
    /// </summary>
    public static LispObject Sub(
        [Required] LispObject target,
        [Required] LispObject other) {
      var res = LookupHelpers.Lookup(
        target, other, null, SymbolType.Create("--sub--"), () => NotImplemented);
      if (!ReferenceEquals(res, NotImplemented)) {
        return res;
      }
      res = LookupHelpers.Lookup(
        other, target, null, SymbolType.Create("--rsub--"), () => NotImplemented);
      if (!ReferenceEquals(res, NotImplemented)) {
        return res;
      }
      throw ThrowTypeError(
        "unsupported operands for sub: {0} and {1}", target.__class__, other.__class__);
    }

    /// <summary>
    /// Returns the result of multiplying the given lisp objects.
    /// </summary>
    public static LispObject Mul(
        [Required] LispObject target,
        [Required] LispObject other) {
      var res = LookupHelpers.Lookup(
        target, other, null, SymbolType.Create("--mul--"), () => NotImplemented);
      if (!ReferenceEquals(res, NotImplemented)) {
        return res;
      }
      res = LookupHelpers.Lookup(
        other, target, null, SymbolType.Create("--rmul--"), () => NotImplemented);
      if (!ReferenceEquals(res, NotImplemented)) {
        return res;
      }
      throw ThrowTypeError(
        "unsupported operands for mul: {0} and {1}", target.__class__, other.__class__);
    }

    /// <summary>
    /// Returns the result of dividing the given lisp objects.
    /// </summary>
    public static LispObject Div(
        [Required] LispObject target,
        [Required] LispObject other) {
      var res = LookupHelpers.Lookup(
        target, other, null, SymbolType.Create("--div--"), () => NotImplemented);
      if (!ReferenceEquals(res, NotImplemented)) {
        return res;
      }
      res = LookupHelpers.Lookup(
        other, target, null, SymbolType.Create("--rdiv--"), () => NotImplemented);
      if (!ReferenceEquals(res, NotImplemented)) {
        return res;
      }
      throw ThrowTypeError(
        "unsupported operands for dev: {0} and {1}", target.__class__, other.__class__);
    }

    /// <summary>
    /// Returns the floor-result of dividing the given lisp objects.
    /// </summary>
    public static LispObject FloorDiv(
        [Required] LispObject target,
        [Required] LispObject other) {
      var res = LookupHelpers.Lookup(
        target, other, null, SymbolType.Create("--floordiv--"), () => NotImplemented);
      if (!ReferenceEquals(res, NotImplemented)) {
        return res;
      }
      res = LookupHelpers.Lookup(
        other, target, null, SymbolType.Create("--rfloordiv--"), () => NotImplemented);
      if (!ReferenceEquals(res, NotImplemented)) {
        return res;
      }
      throw ThrowTypeError(
        "unsupported operands for floordiv: {0} and {1}",
        target.__class__, other.__class__);
    }

    /// <summary>
    /// Returns the remainder of dividing the given lisp objects.
    /// </summary>
    public static LispObject Rem(
        [Required] LispObject target,
        [Required] LispObject other) {
      var res = LookupHelpers.Lookup(
        target, other, null, SymbolType.Create("--rem--"), () => NotImplemented);
      if (!ReferenceEquals(res, NotImplemented)) {
        return res;
      }
      res = LookupHelpers.Lookup(
        other, target, null, SymbolType.Create("--rrem--"), () => NotImplemented);
      if (!ReferenceEquals(res, NotImplemented)) {
        return res;
      }
      throw ThrowTypeError(
        "unsupported operands for rem: {0} and {1}", target.__class__, other.__class__);
    }

    /// <summary>
    /// Returns the result of negating the given lisp object.
    /// </summary>
    public static LispObject Neg(
        [Required] LispObject target) {
      var res = LookupHelpers.Lookup(
        target, null, SymbolType.Create("--neg--"), () => NotImplemented);
      if (!ReferenceEquals(res, NotImplemented)) {
        return res;
      }
      throw ThrowTypeError("unsupported operand for neg: {0}", target.__class__);
    }

    /// <summary>
    /// Returns the positive of the given lisp object.
    /// </summary>
    public static LispObject Pos(
        [Required] LispObject target) {
      var res = LookupHelpers.Lookup(
        target, null, SymbolType.Create("--pos--"), () => NotImplemented);
      if (!ReferenceEquals(res, NotImplemented)) {
        return res;
      }
      throw ThrowTypeError("unsupported operand for pos: {0}", target.__class__);
    }

    /// <summary>
    /// Returns the absolute value of the given lisp object.
    /// </summary>
    public static LispObject Abs(
        [Required] LispObject target) {
      var res = LookupHelpers.Lookup(
        target, null, SymbolType.Create("--abs--"), () => NotImplemented);
      if (!ReferenceEquals(res, NotImplemented)) {
        return res;
      }
      throw ThrowTypeError("unsupported operand for abs: {0}", target.__class__);
    }
  }
}
