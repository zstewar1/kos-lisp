namespace ZStewart.KOSLisp.Types.Helpers {
  public static class MappingOperations {
    private static readonly LispObject getitemattr = StringType.Create("--getitem--");

    public static LispObject GetItem(LispObject target, LispObject key) {
      LispTypeObject targetType = target.__class__;
      LispObject __mro__ = targetType.__mro__;
      for (;;) {
        if (targetType._map_methods != null
            && targetType._map_methods.__getitem__ != null) {
          return targetType._map_methods.__getitem__(target, key);
        } else {
          LispObject __getitem__ = GetItem(targetType.__dict__, getitemattr);
          if (__getitem__ == null) {
            // TODO(zstewa1): Check the error. Continue for KeyError, abort for all else.
          } else {
            // TODO(zstewar1): Call the retrieved object.
            return null;
          }
        }
        if (__mro__ == NilType.Nil) {
          // TODO(zstewar1): __getitem__ not found error.
          return null;
        }
        LispObject nextType = ListOperations.GetCar(__mro__);
        if (nextType == null) return null; // Propagate errors.
        targetType = nextType as LispTypeObject;
        if (targetType == null) {
          // TODO(zstewar1): set a type error: types must be type type. (This should be
          // impossible anyway, since we should prevent setting arbitrary types).
          return null;
        }
        __mro__ = ListOperations.GetCdr(__mro__);
        if (__mro__ == null) return null; // Propagate errors.
      }
    }

    // TODO(zstewar1): Implement SetItem and DelItem.
  }
}
