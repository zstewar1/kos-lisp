using System;

using ZStewart.KOSLisp.Types;
using ZStewart.KOSLisp.Types.Helpers;

namespace ZStewart.KOSLisp.Modules.Builtins {
  public static class BuiltinsModule {
    private static void AddBuiltin(Type fromType, string name, string symbol) {
      var sym = SymbolType.Create(symbol);
      MappingOperations.SetItem(
        _builtins.__dict__, sym, BuiltinFunctionType.Create(fromType, name, sym));
    }

    private static void AddBuiltin<T>(string name, string symbol) {
      AddBuiltin(typeof(T), name, symbol);
    }

    private static ModuleType _builtins;
    public static ModuleType Builtins {
      get {
        if (_builtins != null) return _builtins;

        _builtins = ModuleType.Create(SymbolType.Create("builtins"));

        AddBuiltin(typeof(ListOperations), "GetCar", "car");
        AddBuiltin(typeof(ListOperations), "GetCdr", "cdr");
        AddBuiltin(typeof(ListOperations), "SetCar", "%setcar");
        AddBuiltin(typeof(ListOperations), "SetCdr", "%setcdr");

        return _builtins;
      }
    }
  }
}

