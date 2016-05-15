using System;
using System.Collections.Generic;

namespace ZStewart.KOSLisp.Types.Helpers {
  /// <summary>
  /// A set of helper functions which make it easier to implement looping lookup
  /// functions. These are macros for poor people.
  /// </summary>
  internal static class LookupHelpers {
    /// <summary>
    /// Lookup and perform a unary operation on the target.
    /// </summary>
    /// <param name="target">
    /// The target of the lookup. The object whose MRO should be looped through.
    /// </param>
    /// <param name="getBuiltinOrNull">
    /// A delegate that takes the type object and returns the requested builtin operation,
    /// or null if the operation is not defined as a builtin.
    /// </param>
    /// <param name="fallbackSymbol">
    /// The symbol to lookup the fallback function in the type's dictionary if hasBuiltin
    /// returns false
    /// </param>
    /// <param name="notFoundResult">
    /// A function wich provide a final result or generate an error to be set when the
    /// lookup fails.
    /// </param>
    /// <returns>
    /// The result of the operation on the target object, throught the first builtin or
    /// fallback method found in the target type's method resolution order, or null (with
    /// an error set) if no appropriate method is found.
    /// </returns>
    internal static LispObject Lookup(
        LispObject target,
        Func<LispType, Func<LispObject, LispObject>> getBuiltinOrNull,
        LispObject fallbackSymbol,
        Func<LispObject> notFoundResult) {
      return InnerLookup(
        target,
        getBuiltinOrNull,
        builtin => builtin(target),
        fallbackSymbol,
        fallback => CallableOperations.Call(fallback),
        notFoundResult);
    }

    /// <summary>
    /// Lookup and perform a binary operation on the target.
    /// </summary>
    /// <param name="target">
    /// The target of the lookup. The object whose MRO should be looped through.
    /// </param>
    /// <param name="arg1">
    /// The first argument (besides target) to the function call.
    /// </param>
    /// <param name="getBuiltinOrNull">
    /// A delegate that takes the type object and returns the requested builtin operation,
    /// or null if the operation is not defined as a builtin.
    /// </param>
    /// <param name="fallbackSymbol">
    /// The symbol to lookup the fallback function in the type's dictionary if hasBuiltin
    /// returns false
    /// </param>
    /// <param name="notFoundResult">
    /// A function wich provide a final result or generate an error to be set when the
    /// lookup fails.
    /// </param>
    /// <returns>
    /// The result of the operation on the target object, throught the first builtin or
    /// fallback method found in the target type's method resolution order, or null (with
    /// an error set) if no appropriate method is found.
    /// </returns>
    internal static LispObject Lookup<TArg1>(
        LispObject target,
        TArg1 arg1,
        Func<LispType, Func<LispObject, TArg1, LispObject>> getBuiltinOrNull,
        LispObject fallbackSymbol,
        Func<LispObject> notFoundResult) {
      return InnerLookup(
        target,
        getBuiltinOrNull,
        builtin => builtin(target, arg1),
        fallbackSymbol,
        fallback => CallableOperations.Call(fallback, Arguments.Unmarshal(arg1)),
        notFoundResult);
    }

    /// <summary>
    /// Lookup and perform a ternary operation on the target.
    /// </summary>
    /// <param name="target">
    /// The target of the lookup. The object whose MRO should be looped through.
    /// </param>
    /// <param name="arg1">
    /// The first argument (besides target) to the function call.
    /// </param>
    /// <param name="arg2">
    /// The second argument (besides target) to the function call.
    /// </param>
    /// <param name="getBuiltinOrNull">
    /// A delegate that takes the type object and returns the requested builtin operation,
    /// or null if the operation is not defined as a builtin.
    /// </param>
    /// <param name="fallbackSymbol">
    /// The symbol to lookup the fallback function in the type's dictionary if hasBuiltin
    /// returns false
    /// </param>
    /// <param name="notFoundResult">
    /// A function wich provide a final result or generate an error to be set when the
    /// lookup fails.
    /// </param>
    /// <returns>
    /// The result of the operation on the target object, throught the first builtin or
    /// fallback method found in the target type's method resolution order, or null (with
    /// an error set) if no appropriate method is found.
    /// </returns>
    internal static LispObject Lookup<TArg1, TArg2>(
        LispObject target,
        TArg1 arg1,
        TArg2 arg2,
        Func<LispType, Func<LispObject, TArg1, TArg2, LispObject>> getBuiltinOrNull,
        LispObject fallbackSymbol,
        Func<LispObject> notFoundResult) {
      return InnerLookup(
        target,
        getBuiltinOrNull,
        builtin => builtin(target, arg1, arg2),
        fallbackSymbol,
        fallback => CallableOperations.Call(
          fallback, Arguments.Unmarshal(arg1), Arguments.Unmarshal(arg2)),
        notFoundResult);
    }

    /// <summary>
    /// Inner implementation of Lookup designed to work for any number of arguments.
    /// </summary>
    /// <param name="obj">
    /// The object being searched through.
    /// </param>
    /// <param name="getBuiltinOrNull">
    /// A delegate that takes the type object and returns the requested builtin operation,
    /// or null if no builtin operation is defined.
    /// </param>
    /// <param name="doBuiltinCall">
    /// A delegate that takes the builtin function and calls it with appropriate
    /// arguments.
    /// </param>
    /// <param name="fallbackSymbol">
    /// The symbol to lookup the fallback function in the type's dictionary if hasBuiltin
    /// returns false
    /// </param>
    /// <param name="doFallbackCall">
    /// Delegate which takes the found fallbakc callable and calls it with appropriate
    /// arguments.
    /// </param>
    /// <param name="notFoundResult">
    /// A function which retuns a result to be used when the requested method is not found
    /// on the type. Can also just generate an error message.
    /// </param>
    /// <returns>
    /// The result of the operation on the target object, throught the first builtin or
    /// fallback method found in the target type's method resolution order, or the result
    /// of notFoundResult if no builtin is found.
    /// </returns>
    private static LispObject InnerLookup<T>(
        LispObject obj,
        Func<LispType, T> getBuiltinOrNull,
        Func<T, LispObject> doBuiltinCall,
        LispObject fallbackSymbol,
        Func<LispObject, LispObject> doFallbackCall,
        Func<LispObject> notFoundResult) {
      foreach (var targetType in ListOperations.IterMro(obj)) {
        var builtin = getBuiltinOrNull(targetType);
        if (builtin != null) {
          return doBuiltinCall(builtin);
        } else {
          LispObject fallback = null;
          try {
            fallback = MappingOperations.GetItem(targetType.__dict__, fallbackSymbol);
          } catch (ExceptionWrapper ex) {
            if (!ExceptionType.CheckException(ex, ExceptionType.KeyError)) throw;
          }
          if (fallback != null) {
            if (!DescriptorOperations.IsDescriptor(fallback)) {
              return doFallbackCall(fallback);
            } else {
              return doFallbackCall(
                DescriptorOperations.Get(fallback, obj, obj.__class__));
            }
          }
        }
      }
      return notFoundResult();
    }

    public static bool Query(
        LispObject target, Predicate<LispType> hasBuiltin, LispObject fallbackSymbol) {
      foreach (var targetType in ListOperations.IterMro(target)) {
        if (hasBuiltin(targetType)) return true;
        try {
          MappingOperations.GetItem(targetType.__dict__, fallbackSymbol);
          return true;
        } catch (ExceptionWrapper ex) {
          if (!ExceptionType.CheckException(ex, ExceptionType.KeyError)) throw;
        }
      }
      return false;
    }

    /// <summary>
    /// Check if the given type defines its own version of the specified method before
    /// type in the MRO or is not a subtype of the given type.
    ///
    /// The purpose of this method is to allow certain comparison operations and similar
    /// to check whether the object they are being checked against is one they know how to
    /// compare to.
    ///
    /// For example, object knows how to compare to other objects. But for subtypes of
    /// object, it doesn't want to replace the subtype's equals method if the subtype is
    /// on the right side, i.e. for (eq a b) where a is an object and b is an instance of
    /// a subtype of object, if (type b) defines its own --eq-- method,
    /// </summary>
    public static bool NotSubtypeOrRedefines(
        this LispObject other, LispType type,
        Predicate<LispType> hasBuiltin,
        LispObject fallbackSymbol) {
      foreach(var targetType in ListOperations.IterMro(other)) {
        if (ReferenceEquals(targetType, type)) {
          // It is a subtype, and does not redefine the given operation.
          return false;
        } else if (hasBuiltin(targetType)) {
          // It does redefine the given operation.
          return true;
        } else {
          try {
            MappingOperations.GetItem(targetType.__dict__, fallbackSymbol);
            // It does redefine the given operation.
            return true;
          } catch (ExceptionWrapper ex) {
            if (!ExceptionType.CheckException(ex, ExceptionType.KeyError)) throw;
          }
        }
      }
      // It is not a subtype.
      return true;
    }
  }
}
