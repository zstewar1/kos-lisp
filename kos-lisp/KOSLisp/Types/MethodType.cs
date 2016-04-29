using System;

using ZStewart.KOSLisp.Interpreter;
using ZStewart.KOSLisp.Types.Attributes;
using ZStewart.KOSLisp.Types.Helpers;

namespace ZStewart.KOSLisp.Types {
  public class MethodType : LispObject {
    #region Static Type Setup
    private static LispType _method;
    public static LispType Method {
      get {
        if (_method != null) return _method;

        _method = new LispType {
          __name__ = "Method",
          __call__ = Call,
          __get__ = Get,
          _instance_type = typeof(MethodType),
        };
        _method.__class__ = LispType.Type;
        _method.__bases__ = IConsType.ToLispTuple(LispObject.Object);
        _method.__mro__ = IConsType.ToLispTuple(_method, LispObject.Object);
        _method = LispType.ConfigureType(_method);
        if (_method == null) throw new InvalidOperationException();

        return _method;
      }
    }

    private static LispObject Call (LispObject self, LispObject args) {
      if (self is MethodType) {
        return ((MethodType)self).Call(args);
      } else {
        LispInterpreter.SetException(ExceptionType.CreateTypeError(
          "first argument must be a Method"));
        return null;
      }
    }

    private static LispObject Get (
        LispObject self,
        LispObject instance,
        LispObject type) {
      if (!(self is MethodType)) {
        LispInterpreter.SetException(ExceptionType.CreateTypeError(
          "self must be a Method"));
        return null;
      }
      return self;
    }
    #endregion

    #region Static Helper Methods
    public static MethodType Create(LispObject receiver, LispObject function) {
      var isCallable = CallableOperations.IsCallable(function);
      if (!isCallable.HasValue) return null;
      if (!isCallable.Value) {
        LispInterpreter.SetException(ExceptionType.CreateTypeError(
          "function must be callable"));
        return null;
      }
      return new MethodType(receiver, function) {
        __class__ = Method,
      };
    }
    #endregion

    protected MethodType(LispObject receiver, LispObject function) {
      this.receiver = receiver;
      this.function = function;
    }

    private LispObject receiver;
    private LispObject function;

    public LispObject Call(LispObject args) {
      return CallableOperations.Call(function, IConsType.Create(receiver, args));
    }
  }
}
