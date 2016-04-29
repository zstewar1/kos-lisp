using System;
using System.Collections.Generic;

using ZStewart.KOSLisp.Interpreter;
using ZStewart.KOSLisp.Types.TypeCategories;

namespace ZStewart.KOSLisp.Types {
  public class ModuleType : LispObject {
    #region Static Type Setup
    private static LispType _module;
    public static LispType Module {
      get {
        if (_module != null) return _module;
        _module = new LispType {
          __name__ = "module",
          _instance_type = typeof(ModuleType),
        };
        _module.__class__ = LispType.Type;
        _module.__bases__ = IConsType.ToLispTuple(LispObject.Object);
        _module.__mro__ = IConsType.ToLispTuple(_module, LispObject.Object);
        _module = LispType.ConfigureType(_module);
        if (_module == null) throw new InvalidOperationException();

        LispType.AddDataProperty(_module, "Name", PropConsts.Name);

        return _module;
      }
    }
    #endregion Static Type Setup

    protected ModuleType(string name) : this(SymbolType.Create(name)) { }

    protected ModuleType(SymbolType name) {
      Name = name;
    }

    private SymbolType Name { get; }
  }
}
