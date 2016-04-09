namespace ZStewart.KOSLisp.Types.Helpers {
  public static class MappingOperations {
    private static readonly LispObject getitemattr = StringType.Create("--getitem--");

    public static LispObject GetItem(LispObject target, LispObject key) {
      foreach (var targetType in ListOperations.IterMro(target)) {
        // Propagate Errors.
        if (targetType == null) return null;
        if (targetType._map_methods != null
            && targetType._map_methods.__getitem__ != null) {
          return targetType._map_methods.__getitem__(target, key);
        } else {
          LispObject __getitem__ = GetItem(targetType.__dict__, getitemattr);
          if (__getitem__ == null) {
            // TODO(zstewa1): Check the error. Continue for KeyError, abort for all else.
          } else {
            return CallableOperations.Call(
              __getitem__, IConsType.ToLispTuple(target, key));
          }
        }
      }
      // TODO(zstewar1): Set error for missing __getitem__
      return null;
    }

    // TODO(zstewar1): Implement SetItem and DelItem.
  }
}
