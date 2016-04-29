using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

using ZStewart.KOSLisp.Interpreter;
using ZStewart.KOSLisp.Types.Attributes;
using ZStewart.KOSLisp.Types.Helpers;

namespace ZStewart.KOSLisp.Types {
  public class BuiltinFunctionType : LispObject {
    #region Static Type Setup
    private static LispType _builtinFunction;
    public static LispType BuiltinFunction {
      get {
        if (_builtinFunction != null) return _builtinFunction;

        _builtinFunction = new LispType {
          __name__ = "BuiltinFunction",
          __call__ = Call,
          __get__ = Get,
          _instance_type = typeof(BuiltinFunctionType),
          // TODO(zstewar1): New/Call
        };
        _builtinFunction.__class__ = LispType.Type;
        _builtinFunction.__bases__ = IConsType.ToLispTuple(LispObject.Object);
        _builtinFunction.__mro__ = IConsType.ToLispTuple(
          _builtinFunction, LispObject.Object);
        _builtinFunction = LispType.ConfigureType(_builtinFunction);
        if (_builtinFunction == null) throw new InvalidOperationException();

        return _builtinFunction;
      }
    }

    private static LispObject Call (LispObject self, LispObject args) {
      if (self is BuiltinFunctionType) {
        return ((BuiltinFunctionType)self).caller.Call(args);
      } else {
        LispInterpreter.SetException(ExceptionType.CreateTypeError(
          "first argument must be a BuiltinFunction"));
        return null;
      }
    }

    private static LispObject Get (
        LispObject self,
        LispObject instance,
        LispObject type) {
      if (!(self is BuiltinFunctionType)) {
        LispInterpreter.SetException(ExceptionType.CreateTypeError(
          "self must be a BuiltinFunction"));
        return null;
      }
      if (instance == NilType.Nil && type != NilType.NilClass) {
        return self;
      } else {
        return MethodType.Create(instance, self);
      }
    }
    #endregion Static Type Setup

    #region Static Helpers
    /// <summary>
    /// Create a bound method for the given static method name to be found on the given
    /// type.
    /// </summary>
    /// <typeparam name="T">
    /// The type to lookup methodName on.
    /// </param>
    /// <param name="methodName">
    /// The name of the C# static method to look up.
    /// </param>
    /// <param name="lispName">
    /// The name the function should have in lisp.
    /// </param>
    /// <returns>
    /// A Builtin Function that calls the specfied C# static method.
    /// </returns>
     public static BuiltinFunctionType Create<T>(string methodName, SymbolType lispName) {
      return Create(typeof(T), methodName, lispName);
    }

    /// <summary>
    /// Create a bound method for the given static method name to be found on the given
    /// type.
    /// </summary>
    /// <param name="type">
    /// The type to lookup methodName on.
    /// </param>
    /// <param name="methodName">
    /// The name of the C# static method to look up.
    /// </param>
    /// <param name="lispName">
    /// The name the function should have in lisp.
    /// </param>
    /// <returns>
    /// A Builtin Function that calls the specfied C# static method.
    /// </returns>
    public static BuiltinFunctionType Create(
        Type type, string methodName, SymbolType lispName) {
      return Create(
        type.GetMethod(
          methodName,
          BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static |
          BindingFlags.FlattenHierarchy),
        lispName);
    }

    /// <summary>
    /// Create a bound method for the specified methodinfo.
    /// </summary>
    /// <param name="boundMethod">
    /// The method to be called by this builtin function. This must be static.
    /// </param>
    /// <param name="lispName">
    /// The name of this function (for lisp)
    /// </param>
    /// <returns>
    /// A BuiltinFunction which calls the specified function.
    /// </returns>
    private static BuiltinFunctionType Create(
        MethodInfo boundMethod, SymbolType lispName) {
      return new BuiltinFunctionType(boundMethod, lispName) {
        __class__ = BuiltinFunction,
      };
    }
    #endregion Static Helpers

    protected BuiltinFunctionType(MethodInfo boundMethod, SymbolType lispName) {
      caller = new CallMagic(boundMethod);
      Name = lispName;
    }

    private readonly CallMagic caller;
    public SymbolType Name { get; }
  }
}
