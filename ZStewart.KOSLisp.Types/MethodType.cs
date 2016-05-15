using System;
using System.Collections.Generic;

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
        LispType.ConfigureType(_method);

        LispType.AddDataProperty(_method, "Name", PropConsts.Name);

        return _method;
      }
    }

    [BuiltinFunction(Name = "--call--")]
    private static LispObject Call (
        [Required] LispObject self,
        [RestCapture] List<LispObject> pargs,
        [RestKwCapture] Dictionary<SymbolType, LispObject> kwargs) {
      if (self is MethodType) {
        return ((MethodType)self).Call(pargs, kwargs);
      }
      throw ExceptionType.ThrowTypeError("first argument must be a Method");
    }

    [BuiltinFunction(Name = "--get--")]
    private static LispObject Get (
        [Required] LispObject self,
        [Required] LispObject instance,
        [Optional(null)] LispObject type) {
      if (!(self is MethodType)) {
        throw ExceptionType.ThrowTypeError("self must be a Method");
      }
      return self;
    }

    [BuiltinFunction(Name = "--repr--")]
    private static LispObject Repr([Required] MethodType method) {
      return StringType.Format(
        "[method {0} of {1} object]", method.Name, method.receiver.__class__);
    }
    #endregion

    #region Static Helper Methods
    public static MethodType Create(
        SymbolType name, LispObject receiver, LispObject function) {
      if (!CallableOperations.IsCallable(function)) {
        throw ExceptionType.ThrowTypeError("function must be callable");
      }
      return new MethodType(name, receiver, function) {
        __class__ = Method,
      };
    }
    #endregion

    public SymbolType Name { get; }
    private LispObject receiver;
    private LispObject function;

    protected MethodType(SymbolType name, LispObject receiver, LispObject function) {
      Name = name;
      this.receiver = receiver;
      this.function = function;
    }

    public LispObject Call(
        List<LispObject> pargs,
        Dictionary<SymbolType, LispObject> kwargs) {
      pargs.Insert(0, receiver);
      return CallableOperations.Call(function, pargs, kwargs);
    }
  }
}
