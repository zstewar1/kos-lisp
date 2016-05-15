using System;
using System.Collections.Generic;

using static ZStewart.KOSLisp.Types.ExceptionType;

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
        LispType.AddDataProperty(_macro, "Impl", SymbolType.Create("func"));

        return _macro;
      }
    }

    [BuiltinFunction(Name = "--new--")]
    private static LispObject New(
        [Required] LispType subtype,
        [Required] LispObject func,
        [Optional(null)] SymbolType name) {
      if (name == null) {
        var fallbackName = GetAttribute(
          func, PropConsts.Name, SymbolType.Create("<unnamed>"));
        if (!(fallbackName is SymbolType)) {
          throw ThrowTypeError("name must be a symbol");
        } else {
          name = (SymbolType)fallbackName;
        }
      }
      if (ReferenceEquals(subtype, Macro)) {
        return Create(name, func);
      } else {
        if (!LispType.IsSubtype(subtype, Macro)) {
          throw ThrowTypeError("type must be a  of macro");
        }
        if (!IsCorrectInstanceType(subtype, Macro)) {
          throw ThrowTypeError(
            "macro.--new-- cannot be used to instantiate object of type {0}", subtype);
        }
        if (!CallableOperations.IsCallable(func)) {
          throw ThrowTypeError("func must be callable");
        }
        return new MacroType(name, func) {
          __class__ = subtype,
          __dict__ = DictType.Create(),
        };

      }
    }

    [BuiltinFunction(Name = "--init--")]
    private static void Init([RestIgnore] byte ri, [RestKwIgnore] byte rki) {}

    [BuiltinFunction(Name = "--macroexpand--")]
    private static LispObject MacroExpand (
        [Required] MacroType self,
        [RestCapture] List<LispObject> pargs,
        [RestKwCapture] Dictionary<SymbolType, LispObject> kwargs) {
      return CallableOperations.Call(self.Impl, pargs, kwargs);
    }

    [BuiltinFunction(Name = "--get--")]
    private static LispObject Get (
        [Required] LispObject self,
        [Required] LispObject instance,
        [Optional(null)] LispObject type = null) {
      if (!(self is MacroType)) {
        throw ThrowTypeError("self must be a macro");
      } else if (ReferenceEquals(instance, NilType.Nil)
          && !ReferenceEquals(type, NilType.NilClass)) {
        return self;
      } else {
        var macro = (MacroType)self;
        return Create(macro.Name, MethodType.Create(macro.Name, instance, macro.Impl));
      }
    }

    [BuiltinFunction(Name = "--repr--")]
    private static LispObject ToRepr([Required] MacroType self) {
      return StringType.Format("[macro {0}]", self.Name);
    }
    #endregion Static Type Setup

    #region Static Helper Methods
    /// <summary>
    /// Create a macro with the given name whose implementation is the given callable.
    /// </summary>
    public static MacroType Create(SymbolType name, LispObject impl) {
      if (!CallableOperations.IsCallable(impl)) {
        throw ThrowTypeError("func must be callable");
      }
      return new MacroType(name, impl) {
        __class__ = Macro,
      };
    }
    #endregion Static Helper Methods

    public SymbolType Name { get; }
    public LispObject Impl { get; }

    protected MacroType(
        SymbolType name, LispObject impl) {
      Name = name;
      Impl = impl;
    }
  }
}
