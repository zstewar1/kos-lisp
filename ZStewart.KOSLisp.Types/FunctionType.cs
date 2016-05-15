using System;
using System.Collections.Generic;

using ZStewart.KOSLisp.Types.Attributes;
using ZStewart.KOSLisp.Types.Helpers;

namespace ZStewart.KOSLisp.Types {
  public class FunctionType : LispObject {
    /// <summary>
    /// Function inner implementation delegate.
    /// </summary>
    public delegate LispObject Impl(
        List<LispObject> pargs,
        Dictionary<SymbolType, LispObject> kwargs);

    #region Static Type Setup
    private static LispType _function;
    public static LispType Function {
      get {
        if (_function != null) return _function;

        _function = new LispType {
          __name__ = "Function",
          __call__ = Call,
          __get__ = Get,
          _instance_type = typeof(FunctionType),
        };
        _function.__class__ = LispType.Type;
        _function.__bases__ = IConsType.ToLispTuple(LispObject.Object);
        _function.__mro__ = IConsType.ToLispTuple(_function, LispObject.Object);
        LispType.ConfigureType(_function);

        LispType.AddDataProperty(_function, "Name", PropConsts.Name);

        return _function;
      }
    }

    [BuiltinFunction(Name = "--call--")]
    private static LispObject Call (
        [Required] LispObject self,
        [RestCapture] List<LispObject> pargs,
        [RestKwCapture] Dictionary<SymbolType, LispObject> kwargs) {
      if (self is FunctionType) {
        return ((FunctionType)self).Call(pargs, kwargs);
      }
      throw ExceptionType.ThrowTypeError("first argument must be a Function");
    }

    [BuiltinFunction(Name = "--get--")]
    private static LispObject Get (
        [Required] LispObject self,
        [Required] LispObject instance,
        [Optional(null)] LispObject type) {
      if (!(self is FunctionType)) {
        throw ExceptionType.ThrowTypeError("self must be a Function");
      } else if (ReferenceEquals(instance, NilType.Nil)
          && !ReferenceEquals(type, NilType.NilClass)) {
        return self;
      } else {
        return MethodType.Create(((FunctionType)self).Name, instance, self);
      }
    }

    [BuiltinFunction(Name = "--repr--")]
    private static LispObject ToRepr([Required] FunctionType self) {
      return StringType.Format("[function {0}]", self.Name);
    }
    #endregion Static Type Setup

    #region Static Helper Methods
    public static FunctionType Create(
        SymbolType name, Impl impl) {
      return new FunctionType(name, impl) {
        __class__ = Function,
      };
    }
    #endregion Static Helper Methods

    public SymbolType Name { get; }
    private readonly Impl impl;

    protected FunctionType(
        SymbolType name, Impl impl) {
      Name = name;
      this.impl = impl;
    }

    private LispObject Call(
        List<LispObject> pargs,
        Dictionary<SymbolType, LispObject> kwargs) {
      return impl(pargs, kwargs);
    }
  }
}
