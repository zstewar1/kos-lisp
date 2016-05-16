using System.Collections.Immutable;

using ZStewart.KOSLisp.Compile.AST;
using ZStewart.KOSLisp.Compile.Contexts;
using ZStewart.KOSLisp.Compile.SpecialForms;
using ZStewart.KOSLisp.Types;

namespace ZStewart.KOSLisp.Compile {
  /// <summary>
  /// The default compiler type. This registers the various builtin special form types
  /// under default names.
  /// </summary>
  public class DefaultSemanticAnalyzer : BaseSemanticAnalyzer {
    /// <summary>
    /// Create the dictionary of builtin special forms used to parse macroexpanded
    /// expressions.
    ///
    /// This is a static function because we need to have the dictionary to pass in the
    /// base constructor.
    /// </summary>
    private static ImmutableDictionary<SymbolType, SpecialForm> CreateSpecialFormsDict() {
      var db =
        ImmutableDictionary.CreateBuilder<SymbolType, SpecialForm>();
      db.Add(SymbolType.Create("quote"), new QuoteSpecialForm());
      db.Add(SymbolType.Create("lambda"), new LambdaSpecialForm());
      db.Add(SymbolType.Create("defun"), new DefunSpecialForm());
      db.Add(SymbolType.Create("defmacro"), new DefmacroSpecialForm());
      db.Add(SymbolType.Create("let"), new LetSpecialForm());
      db.Add(SymbolType.Create("progn"), new PrognSpecialForm());
      db.Add(SymbolType.Create("if"), new IfSpecialForm());
      db.Add(SymbolType.Create("%setvar"), new SetVarSpecialForm());
      return db.ToImmutable();
    }

    public DefaultSemanticAnalyzer() : base(
        CreateSpecialFormsDict(),
        new FuncCallSpecialForm(),
        new PrimitiveSpecialForm()) {}

    /// <summary>
    /// Implementation of ToAst for forms.
    /// </summary>
    public override AstOp ToAst(LispObject expression, Context context) {
      // TODO(zstewar1): support macro-expansion of the expression before evaluating.
      return ConvertForm(expression, context);
    }
  }
}
