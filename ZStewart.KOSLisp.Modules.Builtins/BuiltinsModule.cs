using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using static ZStewart.KOSLisp.Types.ExceptionType;

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
    public static ModuleType ImportModule() {
      var builtins = ModuleType.Create(SymbolType.Create("_builtins"));

      // List Operations.
      AddBuiltin(builtins, typeof(ListOperations), "GetCar", "car");
      AddBuiltin(builtins, typeof(ListOperations), "GetCdr", "cdr");
      AddBuiltin(builtins, typeof(ListOperations), "SetCar", "%setcar");
      AddBuiltin(builtins, typeof(ListOperations), "SetCdr", "%setcdr");
      AddBuiltin(builtins, typeof(ListOperations), "Count");
      AddBuiltin(builtins, typeof(ListOperations), "Proper");
      AddBuiltin(builtins, typeof(BuiltinsModule), "Append");
      AddBuiltin(builtins, typeof(BuiltinsModule), "IAppend");

      AddBuiltin(builtins, typeof(BuiltinsModule), "Map");

      // The convienience list/tuple constructors.
      var toList = typeof(ConsType).GetMethod(
          "ToLispList", new Type[] {typeof(IReadOnlyList<LispObject>)});
      var toTuple = typeof(IConsType).GetMethod(
          "ToLispTuple", new Type[] {typeof(IReadOnlyList<LispObject>)});
      AddBuiltin(builtins, toList, "list");
      AddBuiltin(builtins, toTuple, "tuple");
      AddBuiltin(builtins, typeof(ConsType), "Copy");
      AddBuiltin(builtins, typeof(IConsType), "Copy", "icopy");

      // Mapping Operations.
      AddBuiltin(builtins, typeof(MappingOperations), "GetItem", "item");
      AddBuiltin(builtins, typeof(MappingOperations), "SetItem", "%setitem");

      // Attribute retrieval
      AddBuiltin(builtins, typeof(LispObject), "GetAttribute", "getattr");
      AddBuiltin(builtins, typeof(LispObject), "HasAttribute", "hasattr");
      AddBuiltin(builtins, typeof(LispObject), "SetAttribute", "%setattr");

      // Base (binary) comparison operations.
      AddBuiltin(builtins, typeof(ComparisonOperations), "Eq");
      AddBuiltin(builtins, typeof(ComparisonOperations), "Le");
      AddBuiltin(builtins, typeof(ComparisonOperations), "Lt");
      AddBuiltin(builtins, typeof(ComparisonOperations), "Gt");
      AddBuiltin(builtins, typeof(ComparisonOperations), "Ge");
      AddBuiltin(builtins, typeof(ComparisonOperations), "Hash");
      AddBuiltin(builtins, typeof(ComparisonOperations), "Is");

      // Number Operations
      // (binary-only versions)
      AddBuiltin(builtins, typeof(NumberOperations), "Add");
      AddBuiltin(builtins, typeof(NumberOperations), "Sub");
      AddBuiltin(builtins, typeof(NumberOperations), "Mul");
      AddBuiltin(builtins, typeof(NumberOperations), "Div");
      AddBuiltin(builtins, typeof(NumberOperations), "FloorDiv");
      AddBuiltin(builtins, typeof(NumberOperations), "Rem");
      // unary operations
      AddBuiltin(builtins, typeof(NumberOperations), "Neg");
      AddBuiltin(builtins, typeof(NumberOperations), "Pos");
      AddBuiltin(builtins, typeof(NumberOperations), "Abs", "||");

      // Common boolean operations
      AddBuiltin(builtins, typeof(BoolType), "Any");
      AddBuiltin(builtins, typeof(BoolType), "All");

      // String Ops
      AddBuiltin(builtins, typeof(StringType), "GetRepr", "repr");

      // Type Checking
      AddBuiltin(builtins, typeof(LispType), "IsInstance");
      AddBuiltin(builtins, typeof(LispType), "IsSubtype");
      AddBuiltin(builtins, typeof(ListOperations), "IConsIsh", "tuple?");
      AddBuiltin(builtins, typeof(ListOperations), "ConsIsh", "list?");

      // Extras defined in this module.
      AddBuiltin(builtins, typeof(BuiltinsModule), "Print");

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

      // Exception types.
      AddType(builtins, ExceptionType.Exception);
      AddType(builtins, ExceptionType.TypeError);
      AddType(builtins, ExceptionType.ValueError);
      AddType(builtins, ExceptionType.RuntimeError);
      AddType(builtins, ExceptionType.AttributeError);
      AddType(builtins, ExceptionType.NameError);
      AddType(builtins, ExceptionType.KeyError);
      AddType(builtins, ExceptionType.NotImplementedException);
      AddType(builtins, ExceptionType.SyntaxError);
      AddType(builtins, ExceptionType.ImportError);

      AddBuiltin(builtins, typeof(BuiltinsModule), "Throw", "raise");

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
    /// Applies a function to sequential elements of all of the given lists until the
    /// shortest list runs out, returning a list of the results.
    /// </summary>
    public static LispObject Map(
        [Required] LispObject func,
        [RestCapture] List<LispObject> lists) {
      if (lists.Count < 1) {
        throw ThrowTypeError("at least 1 list is required, got 0");
      }
      return ConsType.ToLispList(
        MapZipHelper(lists)
          .Select(args => CallableOperations.Call(func, args))
          .ToList());
    }

    /// <summary>
    /// Creates an enumerator that returns lists enumerating over all of the elements of
    /// the given lists in parallel, until any list runs out of elements.
    /// </summary>
    private static IEnumerable<List<LispObject>> MapZipHelper(List<LispObject> lists) {
      var enumerators = lists.Select(l => ListOperations.IterList(l).GetEnumerator())
        .ToList();
      while (enumerators.All(e => e.MoveNext())) {
        yield return enumerators.Select(e => e.Current).ToList();
      }
    }

    /// <summary>
    /// Appends the given lists together nondestructively.
    /// </summary>
    public static LispObject Append([RestCapture] List<LispObject> lists) {
      return AppendInner(lists, ConsType.Create);
    }

    /// <summary>
    /// Appends the given lists together nondestructively, converting them all to tuples
    /// in the process.
    /// </summary>
    public static LispObject IAppend([RestCapture] List<LispObject> lists) {
      if (lists.Count > 0) {
        lists[lists.Count - 1] = IConsType.Copy(lists[lists.Count - 1]);
      }
      return AppendInner(lists, IConsType.Create);
    }

    /// <summary>
    /// Helper for both append methods above. Nondestructively appends together the given
    /// lists using the provided method for creating new cons cells.
    /// </summary>
    private static LispObject AppendInner(
        List<LispObject> lists, Func<LispObject, LispObject, LispObject> cons) {
      if (lists.Count == 0) {
        return NilType.Nil;
      }
      var result = lists[lists.Count - 1];
      for (int i = lists.Count - 2; i >= 0; i--) {
        foreach(var val in ListOperations.IterList(lists[i]).Reverse()) {
          result = cons(val, result);
        }
      }
      return result;
    }

    /// <summary>
    /// Throws the given exception.
    /// </summary>
    private static void Throw([Required] ExceptionType exception) {
      throw new ExceptionWrapper(exception);
    }
    #endregion Simple Builtin Functions

    #region Builtin Macros
    #endregion Builtin Macros
  }
}

