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
    private static void AddBuiltin(
        ModuleType builtins, MethodInfo method, string symbol) {
      var sym = SymbolType.Create(symbol);
      MappingOperations.SetItem(
        builtins.__dict__, sym, BuiltinFunctionType.Create(method, sym));
    }

    /// <summary>
    /// Add the given method, using the method name as the symbol.
    /// </summary>
    private static void AddBuiltin(ModuleType builtins, MethodInfo method) {
      AddBuiltin(builtins, method, method.Name);
    }

    /// <summary>
    /// Look up the given static method on the given type and ad it to the _builtins
    /// dictionary.
    /// </summary>
    private static void AddBuiltin(
        ModuleType builtins, Type fromType, string name, string symbol) {
      AddBuiltin(builtins, CallMagic.FindMethod(fromType, name), symbol);
    }

    /// <summary>
    /// Add the given method, using the name as the symbol.
    /// </summary>
    private static void AddBuiltin(ModuleType builtins, Type fromType, string name) {
      AddBuiltin(builtins, fromType, name, name);
    }

    /// <summary>
    /// Look up the given static method on the given type and ad it to the _builtins
    /// dictionary.
    /// </summary>
    private static void AddBuiltin<T>(ModuleType builtins, string name, string symbol) {
      AddBuiltin(builtins, typeof(T), name, symbol);
    }

    /// <summary>
    /// Add a reference to the given lisp type to the _builtins module.
    /// </summary>
    private static void AddType(ModuleType builtins, LispType type) {
      MappingOperations.SetItem(
        builtins.__dict__, SymbolType.Create(type.__name__), type);
    }

    /// <summary>
    /// The builtins module, which should be referenced from other modules to provide
    /// basic functionality.
    /// </summary>
    public static ModuleType LoadModule(ModuleImporter importer) {
      var builtins = ModuleType.Create(SymbolType.Create("builtins"));

      // List Operations.
      AddBuiltin(builtins, typeof(ListOperations), "GetCar", "car");
      AddBuiltin(builtins, typeof(ListOperations), "GetCdr", "cdr");
      AddBuiltin(builtins, typeof(ListOperations), "SetCar", "%setcar");
      AddBuiltin(builtins, typeof(ListOperations), "SetCdr", "%setcdr");

      // The convienience list/tuple constructors.
      var toList = typeof(ConsType).GetMethod(
          "ToLispList", new Type[] {typeof(IReadOnlyList<LispObject>)});
      var toTuple = typeof(IConsType).GetMethod(
          "ToLispTuple", new Type[] {typeof(IReadOnlyList<LispObject>)});
      AddBuiltin(builtins, toList, "list");
      AddBuiltin(builtins, toTuple, "tuple");

      // Mapping Operations.
      AddBuiltin(builtins, typeof(MappingOperations), "GetItem");
      AddBuiltin(builtins, typeof(MappingOperations), "SetItem");

      // Attribute retrieval
      AddBuiltin(builtins, typeof(LispObject), "GetAttribute", "getattr");
      AddBuiltin(builtins, typeof(LispObject), "SetAttribute", "setattr");

      // Base (binary) comparison operations.
      AddBuiltin(builtins, typeof(ComparisonOperations), "Eq");
      AddBuiltin(builtins, typeof(ComparisonOperations), "Le");
      AddBuiltin(builtins, typeof(ComparisonOperations), "Lt");
      AddBuiltin(builtins, typeof(ComparisonOperations), "Gt");
      AddBuiltin(builtins, typeof(ComparisonOperations), "Ge");
      AddBuiltin(builtins, typeof(ComparisonOperations), "Hash");
      AddBuiltin(builtins, typeof(ComparisonOperations), "Is");

      // Type Checking
      AddBuiltin(builtins, typeof(LispType), "IsInstance");
      AddBuiltin(builtins, typeof(LispType), "IsSubtype");

      // Extras defined in this module.
      AddBuiltin(builtins, typeof(BuiltinsModule), "Print", "print");
      AddBuiltin(builtins, typeof(BuiltinsModule), "Repr", "repr");

      // Types from the Types library.
      AddType(builtins, LispType.Type);
      AddType(builtins, LispObject.Object);
      AddType(builtins, IConsType.ICons);
      AddType(builtins, ConsType.Cons);
      AddType(builtins, DictType.Dict);
      AddType(builtins, BoolType.Bool);
      AddType(builtins, NumberType.Number);
      AddType(builtins, StringType.String);
      AddType(builtins, SymbolType.Symbol);
      AddType(builtins, KeywordSymbolType.KeywordSymbol);
      AddType(builtins, MacroType.Macro);

      // Type defined in builtins.
      AddType(builtins, GenSymType.GenSym);

      return builtins;
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

