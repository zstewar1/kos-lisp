using ZStewart.KOSLisp.Compile.AST;
using ZStewart.KOSLisp.Compile.Contexts;
using ZStewart.KOSLisp.Types;
using ZStewart.KOSLisp.Types.Helpers;

namespace ZStewart.KOSLisp.Compile.SpecialForms {
  /// <summary>
  /// A nameless special form that serves as a fallback fro expressions that don't
  /// explicitly match any other special form and can't e read as function calls.
  ///
  /// This form evaluates symbols as variables and all other expressions as constants.
  /// </summary>
  public class PrimitiveSpecialForm : SpecialForm {
    /// <summary>
    /// Convert the given expression to an AST.
    /// </summary>
    /// <param name="expression">
    /// The lisp object representing the expression to convert.
    /// </param>
    /// <param name="context">
    /// Variable context of the outer expression. Can be used to look up variables which
    /// this expression does not itself bind.
    /// </param>
    /// <param name="compiler">
    /// The Lisp compiler, which this special form can use to parse sub-expressions.
    /// </param>
    public AstOp ToAst(LispObject expression, Context context, SemanticAnalyzer compiler) {
      if (!(expression is SymbolType)
          || ((SymbolType)expression).IsSelfEvaluating) {
        return Ast.Const(expression);
      }
      return context.GetBinding((SymbolType)expression);
    }
  }
}
