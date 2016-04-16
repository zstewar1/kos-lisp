using System;
using System.Reflection;
using ZStewart.KOSLisp.Interpreter;

namespace ZStewart.KOSLisp.Types {
  public class BuiltinFunctionType : LispObject {
    #region Static Type Setup
    private static LispTypeObject _builtinFunction;
    public static LispTypeObject BuiltinFunction {
      get {
        if (_builtinFunction != null) return _builtinFunction;

        _builtinFunction = new LispTypeObject {
          __name__ = "BuiltinFunction",
          // TODO(zstewar1): New/Call
        };
        _builtinFunction.__class__ = TypeType.Type;
        _builtinFunction.__bases__ = IConsType.ToLispTuple(ObjectType.Object);
        _builtinFunction.__mro__ = IConsType.ToLispTuple(
          _builtinFunction, ObjectType.Object);
        _builtinFunction = LispTypeObject.ConfigureType(_builtinFunction);
        if (_builtinFunction == null) throw new InvalidOperationException();
        return _builtinFunction;
      }
    }

    private static LispObject Call (LispObject self, LispObject args) {
      if (self is BuiltinFunctionType) {
        return ((BuiltinFunctionType)self).Call(args);
      } else {
        LispInterpreter.SetException(ExceptionType.CreateTypeError(
          "First argument must be a BuiltinFunction"));
        return null;
      }
    }
    #endregion Static Type Setup

    #region Static Helpers
    /// <summary>
    /// Create a bound method for the specified methodinfo.
    /// </summary>
    /// <param name="boundMethod">
    /// The method to be called by this builtin function. This must be static.
    /// </param>
    /// <returns>
    /// A BuiltinFunction which calls the specified function.
    /// </returns>
    public BuiltinFunctionType Create(MethodInfo boundMethod) {
      return new BuiltinFunctionType(boundMethod) {
        __class__ = BuiltinFunction,
      };
    }
    #endregion Static Helpers

    protected BuiltinFunctionType(MethodInfo boundMethod) {
      if (!(boundMethod.IsStatic)) throw new ArgumentException();
      this.boundMethod = boundMethod;
    }

    private MethodInfo boundMethod;

    private LispObject Call (LispObject args) {
      LispInterpreter.SetException(ExceptionType.CreateNotImplemented(""));
      return null;
    }
  }
}
