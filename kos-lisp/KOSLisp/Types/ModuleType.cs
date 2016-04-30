using System;
using System.Collections.Generic;

using ZStewart.KOSLisp.Interpreter;
using ZStewart.KOSLisp.Types.TypeCategories;
using ZStewart.KOSLisp.Types.Helpers;

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

    #region Static Helper Methods
    public static ModuleType Create(SymbolType name) {
      return new ModuleType (name) {
        __class__ = Module,
        // Modules always have an instance dict.
        __dict__ = DictType.Create(),
      };
    }

    public static ModuleType Create(SymbolType name, LispObject builtins) {
      var module = Create(name);
      MappingOperations.SetItem(module, PropConsts.Builtins, builtins);
      return module;
    }
    #endregion Static Helper Methods

    protected ModuleType(string name) : this(SymbolType.Create(name)) { }

    protected ModuleType(SymbolType name) {
      Name = name;
    }

    public  SymbolType Name { get; }
  }
}
