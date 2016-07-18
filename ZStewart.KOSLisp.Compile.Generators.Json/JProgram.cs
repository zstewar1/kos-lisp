using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using static ZStewart.KOSLisp.Types.ExceptionType;

using ZStewart.KOSLisp.Compile.AST;
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
      if (!reverseConstantLookup.TryGetValue(obj, out reference)) {
        var newConstant = ConvertConstant(obj);
        reference = ReferencedConstants.Count;
        ReferencedConstants.Add(newConstant);
        reverseConstantLookup.Add(obj, reference);
      }
      return reference;
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
      if (reference < 0 || reference >= ReferencedConstants.Count) {
        throw new ArgumentException("bad reference: id out of range");
      }
      return ReferencedConstants[reference];
    }

    /// <summary>
    /// Gets the JModule referred to by the given identifier.
    /// </summary>
    public JModule DereferenceModule(string moduleIdentifier) {
      return Modules[moduleIdentifier];
    }

    /// <summary>
    /// Get a module reference to an already existing module.
    /// </summary>
    public JModule LookupModule(ModuleType module) {
      return DereferenceModule(ReferToModule(module));
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

    /// <summary>
    /// Converts an AST into an equivalent JAst, adding any cnstants therein to the
    /// program's constant list.
    /// </summary>
    public JAst ConvertAst(AstOp ast) {
      var type = ast.GetType();
      if (type == typeof(AstConst)) {
        return new JConst(ReferToConstant(((AstConst)ast).Value));
      } else if (type == typeof(AstDefmacro)) {
        var dm = (AstDefmacro)ast;
        return new JDefunOrMacro(
            ConvertAst(dm.Forms),
            dm.Args.Select(arg => new JDefunOrMacro.Argument(
                arg.Item1.Type,
                arg.Item2?.Convert(this),
                arg.Item3?.Convert(this))),
            ConvertAst(dm.Name),
            true);
      } else if (type == typeof(AstDefun)) {
        var df = (AstDefun)ast;
        return new JDefunOrMacro(
            ConvertAst(df.Forms),
            df.Args.Select(arg => new JDefunOrMacro.Argument(
                arg.Item1.Type,
                arg.Item2?.Convert(this),
                arg.Item3?.Convert(this))),
            ConvertAst(df.Name),
            false);
      } else if (type == typeof(AstFuncCall)) {
        var fc = (AstFuncCall)ast;
        return new JCall(
            ConvertAst(fc.Function),
            ConvertAst(fc.PositionalArguments),
            fc.KeywordArguments.Select(arg =>
              new KeyValuePair<int, JAst>(
                ReferToConstant(arg.Key), ConvertAst(arg.Value))));
      } else if (type == typeof(AstIf)) {
        var cond = (AstIf)ast;
        return new JIf(
            ConvertAst(cond.Condition),
            ConvertAst(cond.ValueIfTrue),
            ConvertAst(cond.ValueIfFalse));
      } else if (type == typeof(AstImport)) {
        var im = (AstImport)ast;
        return new JImport(
            string.Join(".", im.ModuleIdentifier).ToUpperInvariant(),
            im.Name?.Convert(this),
            im.FromImport?.Select(kvp => new JImport.FromImportItem(
                ReferToConstant(kvp.Key),
                ConvertAst(kvp.Value))),
            im.AllTo != null ? ReferToModule(im.AllTo) : null);
      } else if (type == typeof(AstLambda)) {
        var la = (AstLambda)ast;
        return new JDefunOrMacro(
            ConvertAst(la.Forms),
            la.Args.Select(arg => new JDefunOrMacro.Argument(
                arg.Item1.Type,
                arg.Item2?.Convert(this),
                arg.Item3?.Convert(this))),
            null, false);
      } else if (type == typeof(AstLet)) {
        var let = (AstLet)ast;
        return new JLet(
            ConvertAst(let.Forms),
            let.Bindings.Select(bind => new JLet.LetBinding(
                ConvertAst(bind.Item1),
                bind.Item2?.Convert(this))));
      } else if (type == typeof(AstProgn)) {
        return new JProgn(ConvertAst(((AstProgn)ast).Forms));
      } else if (type == typeof(AstSetVar)) {
        var sv = (AstSetVar)ast;
        return new JSet(ConvertAst(sv.Variable), ConvertAst(sv.Value));
      } else if (type == typeof(AstTry)) {
        var tr = (AstTry)ast;
        return new JTry(
            ConvertAst(tr.Guarded),
            tr.Catches.Select(ca => new JTry.CatchExpr(
                ca.Item1?.Convert(this),
                ca.Item2?.Convert(this),
                ConvertAst(ca.Item3))),
            tr.Finally?.Convert(this));
      } else if (typeof(AstBinding).IsAssignableFrom(type)) {
        return ConvertAst((AstBinding)ast);
      } else {
        throw new ArgumentException("Unknown ast type: " + type);
      }
    }

    /// <summary>
    /// Converts an AST binding into an equivalent JAst Binding, adding constants as
    /// necessary to the program's constant list.
    /// </summary>
    public JBinding ConvertAst(AstBinding binding) {
      var type = binding.GetType();
      if (type == typeof(AstGlobalBinding)) {
        var global = (AstGlobalBinding)binding;
        return new JGlobal(global.Symbol.Identifier, ReferToConstant(global.Symbol));
      } else if (type == typeof(AstLocalBinding)) {
        return new JLocal(((AstLocalBinding)binding).Symbol.Identifier);
      } else {
        throw new ArgumentException("Unknown binding type: " + type);
      }
    }

    /// <summary>
    /// Converts an enumerable of all non-null AstOps to an enumerable of JAst.
    /// </summary>
    public IEnumerable<JAst> ConvertAst(IEnumerable<AstOp> ops) {
      return ops.Select(op => ConvertAst(op));
    }
  }

  /// <summary>
  /// Provides extension methods for converting AstOps into Json equivalents so that we
  /// can use the null-conditional operator.
  /// </summary>
  public static class AstConvertReverseArgumentOrderExtenstions {
    public static JAst Convert(this AstOp op, JProgram arena) {
      return arena.ConvertAst(op);
    }

    public static JBinding Convert(this AstBinding binding, JProgram arena) {
      return arena.ConvertAst(binding);
    }

    public static IEnumerable<JAst> Convert(this IEnumerable<AstOp> ops, JProgram arena) {
      return arena.ConvertAst(ops);
    }
  }
}
