namespace ZStewart.KOSLisp.Types.Helpers {
  public static class ListOperations {
    private static readonly LispObject getcarattr = StringType.Create("--getcar--");
    private static readonly LispObject getcdrattr = StringType.Create("--getcdr--");

    public static LispObject GetCar(LispObject target) {
      LispTypeObject targetType = target.__class__;
      LispObject __mro__ = targetType.__mro__;
      for (;;) {
        if (targetType._list_methods != null
            && targetType._list_methods.__getcar__ != null) {
          return targetType._list_methods.__getcar__(target);
        } else {
          LispObject __getcar__ = DictOperations.GetItem(targetType.__dict__, getcarattr);
          if (__getcar__ == null) {
            // TODO(zstewar1): Check the error. Continue for KeyError, abort for all else.
          } else {
            // TODO(zstewar1): Call the retrieved object.
            return null;
          }
        }
        if (__mro__ == NilType.Nil) {
          // TODO(zstewar1): __getcar__ not found error.
          return null;
        }
        LispObject nextType = GetCar(__mro__);
        if (nextType == null) return null; // Propagate errors.
        targetType = nextType as LispTypeObject;
        if (targetType == null) {
          // TODO(zstewar1): set a type error: types must be type type. (This should be
          // impossible anyway, since we should prevent setting arbitrary types).
          return null;
        }
        __mro__ = GetCdr(__mro__);
        if (__mro__ == null) return null; // Propagate errors.      
      }
    }

    public static LispObject GetCdr(LispObject target) {
      LispTypeObject targetType = target.__class__;
      LispObject __mro__ = targetType.__mro__;
      for (;;) {
        if (targetType._list_methods != null
            && targetType._list_methods.__getcdr__ != null) {
          return targetType._list_methods.__getcdr__(target);
        } else {
          LispObject __getcdr__ = DictOperations.GetItem(targetType.__dict__, getcarattr);
          if (__getcdr__ == null) {
            // TODO(zstewar1): Check the error. Continue for KeyError, abort for all else.
          } else {
            // TODO(zstewar1): Call the retrieved object.
            return null;
          }
        }
        if (__mro__ == NilType.Nil) {
          // TODO(zstewar1): __getcar__ not found error.
          return null;
        }
        LispObject nextType = GetCar(__mro__);
        if (nextType == null) return null; // Propagate errors.
        targetType = nextType as LispTypeObject;
        if (targetType == null) {
          // TODO(zstewar1): set a type error: types must be type type. (This should be
          // impossible anyway, since we should prevent setting arbitrary types).
          return null;
        }
        __mro__ = GetCdr(__mro__);
        if (__mro__ == null) return null; // Propagate errors.      
      }
    }

    // TODO(zstewar1): Implement SetCar and SetCdr.
  }
}
