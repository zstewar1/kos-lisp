using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;

using static ZStewart.KOSLisp.Types.ExceptionType;

using ZStewart.KOSLisp.Types;

namespace ZStewart.KOSLisp.Compile.Generators.Json {
  /// <summary>
  /// Encompasses an entire kerbolisp program encoded in JSON.
  /// </summary>
  public sealed class JProgram {
    /// <summary>
    /// Storage for all constants used in the program. In the tree, these will appear as
    /// JConstantReference containing the integer id of the constant.
    /// </summary>
    [JsonProperty]
    private List<JConstant> ReferencedConstants { get; } = new List<JConstant>();

    /// <summary>
    /// The list of modules which are included in this program.
    /// </summary>
    [JsonProperty]
    private Dictionary<string, JModule> Modules { get; } =
      new Dictionary<string, JModule>();

    /// <summary>
    /// A dictionary used to ensure consistent constant references when building the
    /// program's syntax tree.
    /// </summary>
    private readonly Dictionary<LispObject, int> reverseConstantLookup =
      new Dictionary<LispObject, int>();

    /// <summary>
    /// A dictionary used to ensure consistent module references when building the
    /// program's syntax tree.
    /// </summary>
    private readonly Dictionary<ModuleType, string> reverseModuleLookup =
      new Dictionary<ModuleType, string>();

    /// <summary>
    /// Gets a reference to the converted form of the given lisp object.
    ///
    /// If the object is already in the constant list, returns an existing reference to
    /// it.
    ///
    /// Otherwise, creates a new JConstant, dedupes it against the list of existing
    /// constants, adds it to the constants and returns a reference to it.
    /// </summary>
    public int ReferToConstant(LispObject obj) {
      int reference;
      if (reverseConstantLookup.TryGetValue(obj, out reference)) {
        return reference;
      } else {
        var newConstant = ConvertConstant(obj);
        reference = ReferencedConstants.Count;
        ReferencedConstants.Add(newConstant);
        reverseConstantLookup.Add(obj, reference);
        return newReference;
      }
    }

    /// <summary>
    /// Get the reference string for the given module. Does not add a new module if the
    /// module is not known already.
    /// </summary>
    public string ReferToModule(ModuleType module) {
      string reference;
      if (reverseModuleLookup.TryGetValue(module, out reference)) {
        return reference;
      } else {
        throw new InvalidOperationException("Unknown module");
      }
    }

    /// <summary>
    /// Adds a module so it can be referenced.
    /// </summary>
    public JModule AddModule(ModuleType module, string moduleIdentifier, bool builtin) {
      var created = new JModule(builtin, module.Name.Identifier);
      reverseModuleLookup.Add(module, moduleIdentifier);
      Modules.Add(moduleIdentifier, created);
      return created;
    }

    /// <summary>
    /// Gets the JConstant referred to by the reference.
    /// </summary>
    public JConstant DereferenceConstant(int reference) {
      if (reference.Id < 0 || reference.Id >= ReferencedConstants.Count) {
        throw new ArgumentException("bad reference: id out of range");
      }
      return ReferencedConstants[reference.Id];
    }

    /// <summary>
    /// Gets the JModule referred to by the given identifier.
    /// </summary>
    public JModule DereferenceModule(string moduleIdentifer) {
      return Modules[moduleIdentifier];
    }

    /// <summary>
    /// Converts a lisp object representing a constant value to the equivalent export-AST
    /// value.
    /// </summary>
    private JConstant ConvertConstant(LispObject obj) {
      var type = obj.GetType();
      if (type == typeof(BoolType)) {
        return new JBool(((BoolType)obj).Value);
      } else if (type == typeof(ConsType)) {
        var cons = (ConsType)obj;
        return new JCons(ReferToConstant(cons.Car), ReferToConstant(cons.Cdr));
      } else if (type == typeof(KeywordSymbolType)) {
        return new JKeyword(((KeywordSymbolType)obj).Identifier);
      } else if (type == typeof(NilType)) {
        return new JNil();
      } else if (type == typeof(NumberType)) {
        return new JNumber(((NumberType)obj).Value);
      } else if (type == typeof(StringType)) {
        return new JString(((StringType)obj).Value);
      } else if (type == typeof(SymbolType)) {
        return new JSymbol(((SymbolType)obj).Identifier);
      } else {
        throw ThrowTypeError(
          "emitted AST can only contain primitive values, please check your macros. " +
          "(found non-primitive object {0})", obj);
      }
    }
  }
}
