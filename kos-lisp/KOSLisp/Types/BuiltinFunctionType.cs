using System;
using System.Reflection;

namespace ZStewart.KOSLisp.Types {
  public class BuiltinFunctionType {
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
    #endregion Static Type Setup

    #region Static Helpers
    public BuiltinFunctionType Create(MethodInfo boundMethod) {
      return new BuiltinFunctionType {
        boundMethod = boundMethod,
      };
    }
    #endregion Static Helper

    private MethodInfo boundMethod;
  }
}
