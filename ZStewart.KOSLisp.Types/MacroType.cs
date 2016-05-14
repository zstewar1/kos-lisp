using System;
using System.Collections.Generic;

using ZStewart.KOSLisp.Types.Attributes;
using ZStewart.KOSLisp.Types.Helpers;

namespace ZStewart.KOSLisp.Types {
  public class MacroType : LispObject {
    #region Static Type Setup
    private static LispType _macro;
    public static LispType Macro {
      get {
        if (_macro != null) return _macro;

        _macro = new LispType {
          __name__ = "Macro",
          __get__ = Get,
          _instance_type = typeof(MacroType),
        };
        _macro.__class__ = LispType.Type;
        _macro.__bases__ = IConsType.ToLispTuple(LispObject.Object);
        _macro.__mro__ = IConsType.ToLispTuple(_macro, LispObject.Object);
        LispType.ConfigureType(_macro);

        LispType.AddDataProperty(_macro, "Name", PropConsts.Name);
        LispType.AddStatic(_macro, "MacroExpand", PropConsts.MacroExpand);

        return _macro;
      }
    }

    private static LispObject MacroExpand (
        LispObject self,
        List<LispObject> pargs,
        Dictionary<SymbolType, LispObject> kwargs) {
      if (self is MacroType) {
        return CallableOperations.Call(((MacroType)self).impl, pargs, kwargs);
      }
      throw ExceptionType.ThrowTypeError("first argument must be a Macro");
    }

    private static LispObject Get (
        LispObject self,
        LispObject instance,
        LispObject type) {
      if (!(self is MacroType)) {
        throw ExceptionType.ThrowTypeError("self must be a Macro");
      } else if (instance == NilType.Nil && type != NilType.NilClass) {
        return self;
      } else {
        var macro = (MacroType)self;
        return Create(macro.Name, MethodType.Create(macro.Name, instance, macro.impl));
      }
    }
    #endregion Static Type Setup

    #region Static Helper Methods
    /// <summary>
    /// Create a macro with the given name whose implementation is the given callable.
    /// </summary>
    public static MacroType Create(SymbolType name, LispObject impl) {
      return new MacroType(name, impl) {
        __class__ = Macro,
      };
    }
    #endregion Static Helper Methods

    public SymbolType Name { get; }
    private readonly LispObject impl;

    protected MacroType(
        SymbolType name, LispObject impl) {
      Name = name;
      this.impl = impl;
    }
  }
}
