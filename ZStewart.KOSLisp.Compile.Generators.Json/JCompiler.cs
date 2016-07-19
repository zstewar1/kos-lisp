using Newtonsoft.Json;
using System;

using static ZStewart.KOSLisp.Types.ExceptionType;

using ZStewart.KOSLisp.Compile.AST;
using ZStewart.KOSLisp.Compile.Contexts;
using ZStewart.KOSLisp.Compile.Generators.CSharp;
using ZStewart.KOSLisp.Modules;
using ZStewart.KOSLisp.Parse;
using ZStewart.KOSLisp.Types;

namespace ZStewart.KOSLisp.Compile.Generators.Json {
  /// <summary>
  /// A compiler which uses an evaluator to convert a proram into a json syntax tree.
  /// </summary>
  public class JCompiler {
    /// <summary>
    /// CSharpEvaluator used to convert AST.
    ///
    /// Should be unique to this compiler instance.
    /// </summary>
    private readonly CSharpEvaluator evaluator;

    /// <summary>
    /// The file to read the initial program from.
    /// </summary>
    private readonly Source source;

    /// <summary>
    /// A program object to hold the result.
    /// </summary>
    private readonly JProgram arena = new JProgram();

    public JCompiler(CSharpEvaluator evaluator, Source source) {
      this.evaluator = evaluator;
      this.source = source;

      // Hook callbacks for actually building the program.
      this.evaluator.OnModuleLoad += OnModuleLoadHandler;
      this.evaluator.OnSemantics += OnSemanticsHandler;
    }

    /// <summary>
    /// Evaluates the program file, including importing additional modules.
    ///
    /// Not idempotent, should not be called more than once.
    /// </summary>
    public string Compile() {
      // Create the main fodule for the program, initially named --main--
      var mainModule = evaluator.GetFreshModule("--main--", "--main--");
      // Add --main-- to the program output.
      arena.AddModule(mainModule, "--main--", false);
      // Change the name of --main-- to --macroexpand-- before evaluating.
      mainModule.Name = SymbolType.Create("--macroexpand--");

      evaluator.Evaluate(mainModule, source);

      return JsonConvert.SerializeObject(arena);
    }

    /// <summary>
    /// Handle the evaluator loading a new module by adding an additional module to the
    /// arena.
    /// </summary>
    private void OnModuleLoadHandler(ModuleType module, string identifier, bool builtin) {
      arena.AddModule(module, identifier, builtin);
    }

    /// <summary>
    /// Handle an expression being parsed by adding it to the appropriate module.
    /// </summary>
    private void OnSemanticsHandler(AstOp ast, Context context) {
      var mod = arena.LookupModule(context.Module);
      var converted = arena.ConvertAst(ast);
      mod.AddOperation(converted);
    }
  }
}
