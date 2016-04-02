using System.Collections.Generic;

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
            CallableOperations.Call(__getcar__, IConsType.Create(target, NilType.Nil));
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

    /// <summary>
    /// Creates an iterator that iterates over an object that implements the lisp list 
    /// protocol. The iterator returns null after any failure getting the next item, and 
    /// then after any failure advancing to the next cons.
    /// </summary>
    /// <param name="list">
    /// The lisp list to iterate. Can be any object that has a class level 
    /// --getcar--/--getcdr--
    /// </param>
    /// <returns>A C# iterator that iterates over the given Lisp List.</returns>
    public static IEnumerable<LispObject> IterList(LispObject list) {
      while (list != NilType.Nil) {
        LispObject value = GetCar(list);
        yield return value;
        if (value == null) yield break;
        list = GetCdr(list);
        if (list == null) {
          yield return null;
          yield break;
        }
      }
    }
  }
}
