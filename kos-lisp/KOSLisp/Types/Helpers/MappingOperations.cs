using ZStewart.KOSLisp.Interpreter;
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
        () => string.Format("\"{0}\" object is not subscriptable", target.__class__));
    }

    public static LispObject SetItem(
        [PositionalArgument] LispObject target,
        [PositionalArgument] LispObject key,
        [PositionalArgument] LispObject value) {
      return LookupHelpers.Lookup(
        target, key, value,
        t => t._map_methods != null && t._map_methods.__setitem__ != null,
        t => t._map_methods.__setitem__,
        setitemattr,
        () => string.Format(
          "subscript of \"{0}\" object is not assignable", target.__class__));
    }

    // TODO(zstewar1): Implement SetItem and DelItem.
  }
}
