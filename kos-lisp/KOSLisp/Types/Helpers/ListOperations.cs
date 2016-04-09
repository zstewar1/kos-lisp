using System.Collections.Generic;

namespace ZStewart.KOSLisp.Types.Helpers {
  public static class ListOperations {
    private static readonly LispObject getcarattr = StringType.Create("--getcar--");
    private static readonly LispObject getcdrattr = StringType.Create("--getcdr--");
    private static readonly LispObject setcarattr = StringType.Create("--setcar--");
    private static readonly LispObject setcdrattr = StringType.Create("--setcdr--");

    public static LispObject GetCar(LispObject target) {
      LispTypeObject targetType = target.__class__;
      LispObject __mro__ = targetType.__mro__;
      for (;;) {
        if (targetType._list_methods != null
            && targetType._list_methods.__getcar__ != null) {
          return targetType._list_methods.__getcar__(target);
        } else {
          LispObject __getcar__ = MappingOperations.GetItem(targetType.__dict__, getcarattr);
          if (__getcar__ == null) {
            // TODO(zstewar1): Check the error. Continue for KeyError, abort for all else.
          } else {
            return CallableOperations.Call(__getcar__, IConsType.ToLispTuple(target));
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
          LispObject __getcdr__ = MappingOperations.GetItem(
            targetType.__dict__, getcdrattr);
          if (__getcdr__ == null) {
            // TODO(zstewar1): Check the error. Continue for KeyError, abort for all else.
          } else {
            return CallableOperations.Call(__getcdr__, IConsType.ToLispTuple(target));
          }
        }
        if (__mro__ == NilType.Nil) {
          // TODO(zstewar1): __getcdr__ not found error.
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

    public static LispObject SetCar(LispObject target, LispObject value) {
      LispTypeObject targetType = target.__class__;
      LispObject __mro__ = targetType.__mro__;
      for (;;) {
        if (targetType._list_methods != null
            && targetType._list_methods.__setcar__ != null) {
          return targetType._list_methods.__setcar__(target, value);
        } else {
          LispObject __setcar__ = MappingOperations.GetItem(targetType.__dict__, setcarattr);
          if (__setcar__ == null) {
            // TODO(zstewar1): Check the error. Continue for KeyError, abort for all else.
          } else {
            return CallableOperations.Call(
              __setcar__, IConsType.ToLispTuple(target, value));
          }
        }
        if (__mro__ == NilType.Nil) {
          // TODO(zstewar1): __setcar__ not found error.
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

    public static LispObject SetCdr(LispObject target, LispObject value) {
      LispTypeObject targetType = target.__class__;
      LispObject __mro__ = targetType.__mro__;
      for (;;) {
        if (targetType._list_methods != null
            && targetType._list_methods.__setcdr__ != null) {
          return targetType._list_methods.__setcdr__(target, value);
        } else {
          LispObject __setcdr__ = MappingOperations.GetItem(
            targetType.__dict__, setcdrattr);
          if (__setcdr__ == null) {
            // TODO(zstewar1): Check the error. Continue for KeyError, abort for all else.
          } else {
            return CallableOperations.Call(
              __setcdr__, IConsType.ToLispTuple(target, value));
          }
        }
        if (__mro__ == NilType.Nil) {
          // TODO(zstewar1): __setcdr__ not found error.
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

    /// <summary>
    /// Returns the number of elements in a lisp list. (This will traverse the list).
    /// </summary>
    /// <param name="list">The list.</param>
    /// <returns>The number of elements or null if there is an error counting.</returns>
    public static int? Count(LispObject list) {
      int cnt = 0;
      while (list != NilType.Nil) {
        cnt++;
        list = GetCdr(list);
        if (list == null) return null;
      }
      return cnt;
    }

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
