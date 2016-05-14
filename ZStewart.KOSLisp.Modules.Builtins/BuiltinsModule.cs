using System;
using System.Collections.Generic;
using System.Reflection;

using ZStewart.KOSLisp.Types;
using ZStewart.KOSLisp.Types.Attributes;
using ZStewart.KOSLisp.Types.Helpers;

namespace ZStewart.KOSLisp.Modules.Builtins {
  public static class BuiltinsModule {
    /// <summary>
    /// Add a BuiltinFunction for the given static method to the _builtins module under
    /// the given name.
    /// </summary>
    private static void AddBuiltin(MethodInfo method, string symbol) {
      var sym = SymbolType.Create(symbol);
      MappingOperations.SetItem(
        _builtins.__dict__, sym, BuiltinFunctionType.Create(method, sym));
    }

    /// <summary>
    /// Look up the given static method on the given type and ad it to the _builtins
    /// dictionary.
    /// </summary>
    private static void AddBuiltin(Type fromType, string name, string symbol) {
      AddBuiltin(CallMagic.FindMethod(fromType, name), symbol);
    }

    /// <summary>
    /// Look up the given static method on the given type and ad it to the _builtins
    /// dictionary.
    /// </summary>
    private static void AddBuiltin<T>(string name, string symbol) {
      AddBuiltin(typeof(T), name, symbol);
    }

    /// <summary>
    /// Add a reference to the given lisp type to the _builtins module.
    /// </summary>
    private static void AddType(LispType type) {
      MappingOperations.SetItem(
        _builtins.__dict__, SymbolType.Create(type.__name__), type);
    }

    private static ModuleType _builtins;
    /// <summary>
    /// The builtins module, which should be referenced from other modules to provide
    /// basic functionality.
    /// </summary>
    public static ModuleType Builtins {
      get {
        if (_builtins != null) return _builtins;

        _builtins = ModuleType.Create(SymbolType.Create("builtins"));

        // List Operations.
        AddBuiltin(typeof(ListOperations), "GetCar", "car");
        AddBuiltin(typeof(ListOperations), "GetCdr", "cdr");
        AddBuiltin(typeof(ListOperations), "SetCar", "%setcar");
        AddBuiltin(typeof(ListOperations), "SetCdr", "%setcdr");

        // The convienience list/tuple constructors.
        var toList = typeof(ConsType).GetMethod(
            "ToLispList", new Type[] {typeof(IReadOnlyList<LispObject>)});
        var toTuple = typeof(IConsType).GetMethod(
            "ToLispTuple", new Type[] {typeof(IReadOnlyList<LispObject>)});
        AddBuiltin(toList, "list");
        AddBuiltin(toTuple, "tuple");

        // Extras defined in this module.
        AddBuiltin(typeof(BuiltinsModule), "Print", "print");
        AddBuiltin(typeof(BuiltinsModule), "Repr", "repr");

        // Types from the Types library.
        AddType(LispType.Type);
        AddType(LispObject.Object);
        AddType(IConsType.ICons);
        AddType(ConsType.Cons);
        AddType(DictType.Dict);
        AddType(BoolType.Bool);
        AddType(NumberType.Number);
        AddType(StringType.String);
        AddType(SymbolType.Symbol);

        // Type defined in builtins.
        AddType(GenSymType.GenSym);

        return _builtins;
      }
    }

    #region Simple Builtin Functions
    /// <summary>
    /// Simple builtin function to implement print.
    /// </summary>
    public static LispObject Print([RestCapture] List<LispObject> thingsToPrint) {
      foreach(var obj in thingsToPrint) {
        Console.Write(obj.ToString());
        Console.Write(" ");
      }
      Console.WriteLine();
      return NilType.Nil;
    }

    /// <summary>
    /// Simple function to return the repr of an object.
    /// </summary>
    public static LispObject Repr([Required] LispObject value) {
      return StringType.GetRepr(value);
    }
    #endregion Simple Builtin Functions
  }
}

