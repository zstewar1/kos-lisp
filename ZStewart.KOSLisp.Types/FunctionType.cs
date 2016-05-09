using System;
using System.Collections.Generic;

using ZStewart.KOSLisp.Types.Attributes;
using ZStewart.KOSLisp.Types.Helpers;

namespace ZStewart.KOSLisp.Types {
  public class FunctionType : LispObject {
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
        _function = LispType.ConfigureType(_function);
        if (_function == null) throw new InvalidOperationException();

        LispType.AddDataProperty(_function, "Name", PropConsts.Name);

        return _function;
      }
    }

    private static LispObject Call (
        LispObject self,
        List<LispObject> pargs,
        Dictionary<SymbolType, LispObject> kwargs) {
      if (self is FunctionType) {
        return ((FunctionType)self).Call(pargs, kwargs);
      }
      throw ExceptionType.ThrowTypeError("first argument must be a Function");
    }

    private static LispObject Get (
        LispObject self,
        LispObject instance,
        LispObject type) {
      if (!(self is FunctionType)) {
        throw ExceptionType.ThrowTypeError("self must be a Function");
      } else if (instance == NilType.Nil && type != NilType.NilClass) {
        return self;
      } else {
        return MethodType.Create(((FunctionType)self).Name, instance, self);
      }
    }
    #endregion Static Type Setup

    #region Static Helper Methods
    public static FunctionType Create(
        SymbolType name, CallFunc impl) {
      return new FunctionType(name, impl) {
        __class__ = Function,
      };
    }
    #endregion Static Helper Methods

    public SymbolType Name { get; }
    private readonly CallFunc impl;

    protected FunctionType(
        SymbolType name, CallFunc impl) {
      Name = name;
      this.impl = impl;
    }

    private LispObject Call(
        List<LispObject> pargs,
        Dictionary<SymbolType, LispObject> kwargs) {
      return impl(this, pargs, kwargs);
    }
  }
}
