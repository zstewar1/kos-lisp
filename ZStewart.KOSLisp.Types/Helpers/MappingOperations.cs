using ZStewart.KOSLisp.Types.Attributes;

namespace ZStewart.KOSLisp.Types.Helpers {
  public static class MappingOperations {
    public static LispObject GetItem(
        [Required] LispObject target,
        [Required] LispObject key) {
      return LookupHelpers.Lookup(
        target, key,
        t => t._map_methods != null && t._map_methods.__getitem__ != null,
        t => t._map_methods.__getitem__,
        PropConsts.GetItem,
        () => ExceptionType.CreateTypeError(
          "\"{0}\" object is not subscriptable", target.__class__));
    }

    public static bool CanGet([Required] LispObject target) {
      return LookupHelpers.Query(
        target,
        t => t._map_methods != null && t._map_methods.__getitem__ != null,
        PropConsts.GetItem);
    }

    public static LispObject SetItem(
        [Required] LispObject target,
        [Required] LispObject key,
        [Required] LispObject value) {
      LookupHelpers.Lookup(
        target, key, value,
        t => t._map_methods != null && t._map_methods.__setitem__ != null,
        t => t._map_methods.__setitem__,
        PropConsts.SetItem,
        () => ExceptionType.CreateTypeError(
          "subscript of \"{0}\" object is not assignable", target.__class__));
      return value;
    }

    public static bool CanSet([Required] LispObject target) {
      return LookupHelpers.Query(
        target,
        t => t._map_methods != null && t._map_methods.__setitem__ != null,
        PropConsts.SetItem);
    }

    // TODO(zstewar1): Implement SetItem and DelItem.
  }
}
