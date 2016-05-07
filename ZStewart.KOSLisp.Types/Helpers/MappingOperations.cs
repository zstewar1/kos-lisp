using ZStewart.KOSLisp.Types.Attributes;

namespace ZStewart.KOSLisp.Types.Helpers {
  public static class MappingOperations {
    private static readonly LispObject getitemattr = SymbolType.Create("--getitem--");
    private static readonly LispObject setitemattr = SymbolType.Create("--setitem--");

    public static LispObject GetItem(
        [PositionalArgument] LispObject target,
        [PositionalArgument] LispObject key) {
      return LookupHelpers.Lookup(
        target, key,
        t => t._map_methods != null && t._map_methods.__getitem__ != null,
        t => t._map_methods.__getitem__,
        getitemattr,
        () => ExceptionType.CreateTypeError(
          "\"{0}\" object is not subscriptable", target.__class__));
    }

    public static bool CanGet([PositionalArgument] LispObject target) {
      return LookupHelpers.Query(
        target,
        t => t._map_methods != null && t._map_methods.__getitem__ != null,
        getitemattr);
    }

    public static LispObject SetItem(
        [PositionalArgument] LispObject target,
        [PositionalArgument] LispObject key,
        [PositionalArgument] LispObject value) {
      LookupHelpers.Lookup(
        target, key, value,
        t => t._map_methods != null && t._map_methods.__setitem__ != null,
        t => t._map_methods.__setitem__,
        setitemattr,
        () => ExceptionType.CreateTypeError(
          "subscript of \"{0}\" object is not assignable", target.__class__));
      return value;
    }

    public static bool CanSet([PositionalArgument] LispObject target) {
      return LookupHelpers.Query(
        target,
        t => t._map_methods != null && t._map_methods.__setitem__ != null,
        setitemattr);
    }

    // TODO(zstewar1): Implement SetItem and DelItem.
  }
}
