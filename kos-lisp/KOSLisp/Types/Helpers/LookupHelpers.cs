using System;
using System.Collections.Generic;

using ZStewart.KOSLisp.Interpreter;

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
    /// <param name="hasBuiltin">
    /// A delegate that takes the type object and checks if it has the requested operation
    /// available as a builtin.
    /// </param>
    /// <param name="getBuiltin">
    /// A delegate that takes the type object and returns the requested builtin operation.
    /// </param>
    /// <param name="fallbackSymbol">
    /// The symbol to lookup the fallback function in the type's dictionary if hasBuiltin
    /// returns false
    /// </param>
    /// <param name="generateError">
    /// A function wich generate an error to be set when the lookup fails.
    /// </param>
    /// <returns>
    /// The result of the operation on the target object, throught the first builtin or
    /// fallback method found in the target type's method resolution order, or null (with
    /// an error set) if no appropriate method is found.
    /// </returns>
    internal static LispObject Lookup(
        LispObject target,
        Predicate<LispType> hasBuiltin,
        Func<LispType, Func<LispObject, LispObject>> getBuiltin,
        LispObject fallbackSymbol,
        Func<ExceptionType> generateError) {
      return InnerLookup(
        ListOperations.IterMro(target),
        f => f(target),
        () => IConsType.ToLispTuple(target),
        hasBuiltin,
        getBuiltin,
        fallbackSymbol,
        generateError);
    }

    /// <summary>
    /// Lookup and perform a unary operation on the target.
    /// </summary>
    /// <param name="target">
    /// The target of the lookup. The object whose MRO should be looped through.
    /// </param>
    /// <param name="hasBuiltin">
    /// A delegate that takes the type object and checks if it has the requested operation
    /// available as a builtin.
    /// </param>
    /// <param name="getBuiltin">
    /// A delegate that takes the type object and returns the requested builtin operation.
    /// </param>
    /// <param name="fallbackSymbol">
    /// The symbol to lookup the fallback function in the type's dictionary if hasBuiltin
    /// returns false
    /// </param>
    /// <param name="getFallbackArgs">
    /// A function which produces the argument list to be used for the fallback.
    /// </param>
    /// <param name="generateError">
    /// A function wich generate an error to be set when the lookup fails.
    /// </param>
    /// <returns>
    /// The result of the operation on the target object, throught the first builtin or
    /// fallback method found in the target type's method resolution order, or null (with
    /// an error set) if no appropriate method is found.
    /// </returns>
    internal static LispObject Lookup(
        LispObject target,
        Predicate<LispType> hasBuiltin,
        Func<LispType, Func<LispObject, LispObject>> getBuiltin,
        LispObject fallbackSymbol,
        Func<LispObject> getFallbackArgs,
        Func<ExceptionType> generateError) {
      return InnerLookup(
        ListOperations.IterMro(target),
        f => f(target),
        getFallbackArgs,
        hasBuiltin,
        getBuiltin,
        fallbackSymbol,
        generateError);
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
    /// <param name="hasBuiltin">
    /// A delegate that takes the type object and checks if it has the requested operation
    /// available as a builtin.
    /// </param>
    /// <param name="getBuiltin">
    /// A delegate that takes the type object and returns the requested builtin operation.
    /// </param>
    /// <param name="fallbackSymbol">
    /// The symbol to lookup the fallback function in the type's dictionary if hasBuiltin
    /// returns false
    /// </param>
    /// <param name="generateError">
    /// A function wich generate an error to be set when the lookup fails.
    /// </param>
    /// <returns>
    /// The result of the operation on the target object, throught the first builtin or
    /// fallback method found in the target type's method resolution order, or null (with
    /// an error set) if no appropriate method is found.
    /// </returns>
    internal static LispObject Lookup(
        LispObject target,
        LispObject arg1,
        Predicate<LispType> hasBuiltin,
        Func<LispType, Func<LispObject, LispObject, LispObject>> getBuiltin,
        LispObject fallbackSymbol,
        Func<ExceptionType> generateError) {
      return InnerLookup(
        ListOperations.IterMro(target),
        f => f(target, arg1),
        () => IConsType.ToLispTuple(target, arg1),
        hasBuiltin,
        getBuiltin,
        fallbackSymbol,
        generateError);
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
    /// <param name="hasBuiltin">
    /// A delegate that takes the type object and checks if it has the requested operation
    /// available as a builtin.
    /// </param>
    /// <param name="getBuiltin">
    /// A delegate that takes the type object and returns the requested builtin operation.
    /// </param>
    /// <param name="fallbackSymbol">
    /// The symbol to lookup the fallback function in the type's dictionary if hasBuiltin
    /// returns false
    /// </param>
    /// <param name="getFallbackArgs">
    /// A function which produces the argument list to be used for the fallback.
    /// </param>
    /// <param name="generateError">
    /// A function wich generate an error to be set when the lookup fails.
    /// </param>
    /// <returns>
    /// The result of the operation on the target object, throught the first builtin or
    /// fallback method found in the target type's method resolution order, or null (with
    /// an error set) if no appropriate method is found.
    /// </returns>
    internal static LispObject Lookup(
        LispObject target,
        LispObject arg1,
        Predicate<LispType> hasBuiltin,
        Func<LispType, Func<LispObject, LispObject, LispObject>> getBuiltin,
        LispObject fallbackSymbol,
        Func<LispObject> getFallbackArgs,
        Func<ExceptionType> generateError) {
      return InnerLookup(
        ListOperations.IterMro(target),
        f => f(target, arg1),
        getFallbackArgs,
        hasBuiltin,
        getBuiltin,
        fallbackSymbol,
        generateError);
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
    /// <param name="hasBuiltin">
    /// A delegate that takes the type object and checks if it has the requested operation
    /// available as a builtin.
    /// </param>
    /// <param name="getBuiltin">
    /// A delegate that takes the type object and returns the requested builtin operation.
    /// </param>
    /// <param name="fallbackSymbol">
    /// The symbol to lookup the fallback function in the type's dictionary if hasBuiltin
    /// returns false
    /// </param>
    /// <param name="generateError">
    /// A function wich generate an error to be set when the lookup fails.
    /// </param>
    /// <returns>
    /// The result of the operation on the target object, throught the first builtin or
    /// fallback method found in the target type's method resolution order, or null (with
    /// an error set) if no appropriate method is found.
    /// </returns>
    internal static LispObject Lookup(
        LispObject target,
        LispObject arg1,
        LispObject arg2,
        Predicate<LispType> hasBuiltin,
        Func<LispType, Func<LispObject, LispObject, LispObject, LispObject>>
          getBuiltin,
        LispObject fallbackSymbol,
        Func<ExceptionType> generateError) {
      return InnerLookup(
        ListOperations.IterMro(target),
        f => f(target, arg1, arg2),
        () => IConsType.ToLispTuple(target, arg1, arg2),
        hasBuiltin,
        getBuiltin,
        fallbackSymbol,
        generateError);
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
    /// <param name="hasBuiltin">
    /// A delegate that takes the type object and checks if it has the requested operation
    /// available as a builtin.
    /// </param>
    /// <param name="getBuiltin">
    /// A delegate that takes the type object and returns the requested builtin operation.
    /// </param>
    /// <param name="fallbackSymbol">
    /// The symbol to lookup the fallback function in the type's dictionary if hasBuiltin
    /// returns false
    /// </param>
    /// <param name="getFallbackArgs">
    /// A function which produces the argument list to be used for the fallback.
    /// </param>
    /// <param name="generateError">
    /// A function wich generate an error to be set when the lookup fails.
    /// </param>
    /// <returns>
    /// The result of the operation on the target object, throught the first builtin or
    /// fallback method found in the target type's method resolution order, or null (with
    /// an error set) if no appropriate method is found.
    /// </returns>
    internal static LispObject Lookup(
        LispObject target,
        LispObject arg1,
        LispObject arg2,
        Predicate<LispType> hasBuiltin,
        Func<LispType, Func<LispObject, LispObject, LispObject, LispObject>>
          getBuiltin,
        LispObject fallbackSymbol,
        Func<LispObject> getFallbackArgs,
        Func<ExceptionType> generateError) {
      return InnerLookup(
        ListOperations.IterMro(target),
        f => f(target, arg1, arg2),
        getFallbackArgs,
        hasBuiltin,
        getBuiltin,
        fallbackSymbol,
        generateError);
    }

    /// <summary>
    /// Inner implementation of Lookup designed to work for any number of arguments.
    /// </summary>
    /// <param name="typeIter">
    /// An iterator for the types to check the lookup in. Generally just IterMro of the
    /// target object.
    /// </param>
    /// <param name="doBuiltinCall">
    /// A delegate that takes the builtin function and calls it with appropriate
    /// arguments.
    /// </param>
    /// <param name="getFallbackArgs">
    /// A delegate that takes returns the arguments that should be passed to the fallback
    /// function call.
    /// </param>
    /// <param name="hasBuiltin">
    /// A delegate that takes the type object and checks if it has the requested operation
    /// available as a builtin.
    /// </param>
    /// <param name="getBuiltin">
    /// A delegate that takes the type object and returns the requested builtin operation.
    /// </param>
    /// <param name="fallbackSymbol">
    /// The symbol to lookup the fallback function in the type's dictionary if hasBuiltin
    /// returns false
    /// </param>
    /// <param name="generateError">
    /// A function which returns a formatted error message for when the approprate
    /// function is not found as a builtin or dict item in the MRO.
    /// </param>
    /// <returns>
    /// The result of the operation on the target object, throught the first builtin or
    /// fallback method found in the target type's method resolution order, or null (with
    /// an error set) if no appropriate method is found.
    /// </returns>
    private static LispObject InnerLookup<T>(
        IEnumerable<LispType> typeIter,
        Func<T, LispObject> doBuiltinCall,
        Func<LispObject> getFallbackArgs,
        Predicate<LispType> hasBuiltin,
        Func<LispType, T> getBuiltin,
        LispObject fallbackSymbol,
        Func<ExceptionType> generateError) {
      foreach (var targetType in typeIter) {
        if (targetType == null) return null;
        if (hasBuiltin(targetType)) {
          return doBuiltinCall(getBuiltin(targetType));
        } else {
          LispObject fallback = MappingOperations.GetItem(
            targetType.__dict__, fallbackSymbol);
          if (fallback == null) {
            if (LispInterpreter.CheckException(ExceptionType.KeyError)) {
              LispInterpreter.ClearException();
            } else {
              return null;
            }
          } else {
            return CallableOperations.Call(fallback, getFallbackArgs());
          }
        }
      }
      LispInterpreter.SetException(generateError());
      return null;
    }
  }
}
