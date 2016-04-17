using System;
using System.Collections.Generic;

using ZStewart.KOSLisp.Interpreter;
using ZStewart.KOSLisp.Types.Attributes;

namespace ZStewart.KOSLisp.Types.Helpers {
  public static class ListOperations {
    private static readonly LispObject getcarattr = SymbolType.Create("--getcar--");
    private static readonly LispObject getcdrattr = SymbolType.Create("--getcdr--");
    private static readonly LispObject setcarattr = SymbolType.Create("--setcar--");
    private static readonly LispObject setcdrattr = SymbolType.Create("--setcdr--");

    public static LispObject GetCar([PositionalArgument] LispObject target) {
      LispTypeObject targetType = target.__class__;
      // __class__ should be the first element of __mro__, but we can't use IterMro in
      // this method, because IterMro depends on being able to call GetCar, so trying to
      // use it would cause an infinite recursion.
      LispObject __mro__ = targetType.__mro__;
      for (;;) {
        if (targetType._list_methods != null
            && targetType._list_methods.__getcar__ != null) {
          return targetType._list_methods.__getcar__(target);
        } else {
          LispObject __getcar__ = MappingOperations.GetItem(targetType.__dict__, getcarattr);
          if (__getcar__ == null) {
            if (LispInterpreter.CheckException(ExceptionType.KeyError))
              LispInterpreter.ClearException();
            else return null;
          } else {
            return CallableOperations.Call(__getcar__, IConsType.ToLispTuple(target));
          }
        }
        // Advance to CDR before continuing, since we have to force-skip the first
        // element.
        __mro__ = GetCdr(__mro__);
        if (__mro__ == null) return null; // Propagate errors.
        if (__mro__ == NilType.Nil) {
          LispInterpreter.SetException(ExceptionType.CreateTypeError(
            "cannot get car of \"{0}\" object", target.__class__));
          return null;
        }
        LispObject nextType = GetCar(__mro__);
        if (nextType == null) return null; // Propagate errors.
        // Just accept cast errors. It should be impossible for any object in the MRO to
        // be assigned to a non-type and impossible for a type not to inheirt from
        // LispTypeObject.
        targetType = (LispTypeObject)nextType;
      }
    }

    public static LispObject GetCdr([PositionalArgument] LispObject target) {
      // It should be possible to iterate over the MRO of the target this way because
      // GetCdr is not called inside of the iterator until MoveNext is called for the
      // first time.
      foreach (var targetType in IterMro(target)) {
        // Propagate errors.
        if (targetType == null) return null;
        if (targetType._list_methods != null
            && targetType._list_methods.__getcdr__ != null) {
          return targetType._list_methods.__getcdr__(target);
        } else {
          LispObject __getcdr__ = MappingOperations.GetItem(
            targetType.__dict__, getcdrattr);
          if (__getcdr__ == null) {
            if (LispInterpreter.CheckException(ExceptionType.KeyError))
              LispInterpreter.ClearException();
            else return null;
          } else {
            return CallableOperations.Call(__getcdr__, IConsType.ToLispTuple(target));
          }
        }
      }
      LispInterpreter.SetException(ExceptionType.CreateTypeError(
        "cannot get cdr of \"{0}\" object", target.__class__));
      return null;
    }

    public static LispObject SetCar(
        [PositionalArgument] LispObject target, 
        [PositionalArgument] LispObject value) {
      foreach (var targetType in IterMro(target)) {
        // Propagate errors.
        if (targetType == null) return null;
        if (targetType._list_methods != null
            && targetType._list_methods.__setcar__ != null) {
          return targetType._list_methods.__setcar__(target, value);
        } else {
          LispObject __setcar__ = MappingOperations.GetItem(targetType.__dict__, setcarattr);
          if (__setcar__ == null) {
            if (LispInterpreter.CheckException(ExceptionType.KeyError))
              LispInterpreter.ClearException();
            else return null;
          } else {
            return CallableOperations.Call(
              __setcar__, IConsType.ToLispTuple(target, value));
          }
        }
      }
      LispInterpreter.SetException(ExceptionType.CreateTypeError(
        "cannot set car of \"{0}\" object", target.__class__));
      return null;
    }

    public static LispObject SetCdr(
        [PositionalArgument] LispObject target,
        [PositionalArgument] LispObject value) {
      foreach (var targetType in IterMro(target)) {
        // Propagate errors.
        if (targetType == null) return null;
        if (targetType._list_methods != null
            && targetType._list_methods.__setcdr__ != null) {
          return targetType._list_methods.__setcdr__(target, value);
        } else {
          LispObject __setcdr__ = MappingOperations.GetItem(
            targetType.__dict__, setcdrattr);
          if (__setcdr__ == null) {
            if (LispInterpreter.CheckException(ExceptionType.KeyError))
              LispInterpreter.ClearException();
            else return null;
          } else {
            return CallableOperations.Call(
              __setcdr__, IConsType.ToLispTuple(target, value));
          }
        }
      }
      LispInterpreter.SetException(ExceptionType.CreateTypeError(
        "cannot set cdr of \"{0}\" object", target.__class__));
      return null;
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
        if (list == null) {
          if (LispInterpreter.CheckException(ExceptionType.TypeError)) {
            var cause = LispInterpreter.Exception;
            LispInterpreter.ClearException();
            var extended = ExceptionType.CreateTypeError(
              "Count list failed: not a proper list.");
            extended.__cause__ = cause;
            LispInterpreter.SetException(extended);
          }
          return null;
        }
      }
      return cnt;
    }

    /// <summary>
    /// Lisp-enabled version of Count.
    /// </summary>
    public static LispObject LispCount([PositionalArgument] LispObject list) {
      var cnt = Count(list);
      if (!cnt.HasValue) return null;
      return NumberType.Create(cnt.Value);
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

    /// <summary>
    /// Iterates over a lisp list of type T. Generates a Lisp error if any of the items in
    /// the list are not of type T.
    /// </summary>
    /// <typeparam name="T">The type of objects to return.</typeparam>
    /// <param name="list">The lisp list to iterate over.</param>
    /// <returns>An enumerable of the list that returns elements as type T.</returns>
    public static IEnumerable<T> IterList<T>(LispObject list) where T : LispObject {
      foreach (var next in IterList(list)) {
        // Propagate errors.
        if (next == null) {
          yield return null;
          yield break;
        }
        if (next is T) {
          yield return next as T;
        } else {
          LispInterpreter.SetException(ExceptionType.CreateTypeError(string.Format(
            "A builtin attempted to iterate the list {0} as C# type IEnumerable<{1}>, " +
            "but there was an element of C# type {2}, which could not be cast to {1}",
            list, typeof(T).Name, next.GetType().Name)));
          yield return null;
          yield break;
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
    public static IEnumerable<LispTypeObject> IterMro(LispObject target) {
      return IterList<LispTypeObject>(target.__class__.__mro__);
    }
  }
}
