using System;
using System.Collections.Generic;

using ZStewart.KOSLisp.Types.Attributes;

namespace ZStewart.KOSLisp.Types.Helpers {
  public static class ListOperations {
    private static readonly LispObject getcarattr = SymbolType.Create("--getcar--");
    private static readonly LispObject getcdrattr = SymbolType.Create("--getcdr--");
    private static readonly LispObject setcarattr = SymbolType.Create("--setcar--");
    private static readonly LispObject setcdrattr = SymbolType.Create("--setcdr--");

    public static LispObject GetCar([Required] LispObject target) {
      LispType targetType = target.__class__;
      // __class__ should be the first element of __mro__, but we can't use IterMro in
      // this method, because IterMro depends on being able to call GetCar, so trying to
      // use it would cause an infinite recursion.
      LispObject __mro__ = targetType.__mro__;
      for (;;) {
        if (targetType._list_methods != null
            && targetType._list_methods.__getcar__ != null) {
          return targetType._list_methods.__getcar__(target);
        } else {
          LispObject getcar = null;
          try {
            getcar = MappingOperations.GetItem(targetType.__dict__, getcarattr);
          } catch (ExceptionWrapper ex) {
            if (!ExceptionType.Check(ex, ExceptionType.KeyError)) throw;
          }
          if (getcar != null) {
            return CallableOperations.Call(getcar, IConsType.ToLispTuple(target));
          }
        }
        // Advance to CDR before continuing, since we have to force-skip the first
        // element.
        __mro__ = GetCdr(__mro__);
        if (__mro__ == NilType.Nil) {
          throw ExceptionType.ThrowTypeError(
            "cannot get car of \"{0}\" object", target.__class__);
        }
        LispObject nextType = GetCar(__mro__);
        // Just accept cast errors. It should be impossible for any object in the MRO to
        // be assigned to a non-type and impossible for a type not to inheirt from
        // LispTypeObject.
        targetType = (LispType)nextType;
      }
    }

    public static LispObject GetCdr([Required] LispObject target) {
      // It should be possible to iterate over the MRO of the target this way because
      // GetCdr is not called inside of the iterator until MoveNext is called for the
      // first time, so this method shouldn't produce an infinite recursion.
      return LookupHelpers.Lookup(
        target,
        t => t._list_methods != null && t._list_methods.__getcdr__ != null,
        t => t._list_methods.__getcdr__,
        getcdrattr,
        () => ExceptionType.CreateTypeError(
          "cannot get cdr of \"{0}\" object", target.__class__));
    }

    public static LispObject SetCar(
        [Required] LispObject target,
        [Required] LispObject value) {
      LookupHelpers.Lookup(
        target, value,
        t => t._list_methods != null && t._list_methods.__setcar__ != null,
        t => t._list_methods.__setcar__,
        setcarattr,
        () => ExceptionType.CreateTypeError(
          "cannot set car of \"{0}\" object", target.__class__));
      return value;
    }

    public static LispObject SetCdr(
        [Required] LispObject target,
        [Required] LispObject value) {
      LookupHelpers.Lookup(
        target, value,
        t => t._list_methods != null && t._list_methods.__setcdr__ != null,
        t => t._list_methods.__setcdr__,
        setcdrattr,
        () => ExceptionType.CreateTypeError(
          "cannot set cdr of \"{0}\" object", target.__class__));
      return value;
    }

    /// <summary>
    /// Returns the number of elements in a lisp list. (This will traverse the list).
    /// </summary>
    /// <param name="list">The list.</param>
    /// <returns>The number of elements or null if there is an error counting.</returns>
    public static int Count(LispObject list) {
      int cnt = 0;
      while (list != NilType.Nil) {
        cnt++;
        try {
          list = GetCdr(list);
        } catch (ExceptionWrapper ex) {
          if (!ExceptionType.Check(ex, ExceptionType.TypeError)) throw;

          var ne = ExceptionType.CreateTypeError("Count list failed: not a proper list.");
          ne.__cause__ = ex;
          throw new ExceptionWrapper(ne);
        }
      }
      return cnt;
    }

    /// <summary>
    /// Checks if a lisp list is a proper list (i.e. not dotted)
    /// </summary>
    public static bool Proper(LispObject list) {
      try {
        Count(list);
        return true;
      } catch (ExceptionWrapper ex) {
        if (!ExceptionType.Check(ex, ExceptionType.TypeError)) throw;
      }
      return false;
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
        yield return GetCar(list);
        list = GetCdr(list);
      }
    }

    /// <summary>
    /// Iterates over a lisp list of type T. Generates a Lisp error if any of the items in
    /// the list are not of type T.
    /// </summary>
    /// <typeparam name="T">The type of objects to return.</typeparam>
    /// <param name="list">The lisp list to iterate over.</param>
    /// <returns>An enumerable of the list that returns elements as type T.</returns>
    public static IEnumerable<T> IterList<T>(LispObject list) where T : LispObject {
      foreach (var next in IterList(list)) {
        if (next is T) {
          yield return next as T;
        } else {
          throw ExceptionType.ThrowTypeError(
            "A builtin attempted to iterate the list {0} as C# type IEnumerable<{1}>, " +
            "but there was an element of C# type {2}, which could not be cast to {1}",
            list, typeof(T).Name, next.GetType().Name);
        }
      }
    }

    /// <summary>
    /// Creates an iterator that iterates over the types in the method resolution order
    /// for the class of the target.
    /// </summary>
    /// <param name="target">
    /// The object to resolve on. This method iterates over target.__class__.__mro__
    /// </param>
    /// <returns>A C# iterator that iterates over the given Lisp object's MRO</returns>
    public static IEnumerable<LispType> IterMro(LispObject target) {
      return IterList<LispType>(target.__class__.__mro__);
    }
  }
}
