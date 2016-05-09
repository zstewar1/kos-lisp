using ZStewart.KOSLisp.Compile.AST;
using ZStewart.KOSLisp.Compile.Contexts;
using ZStewart.KOSLisp.Types;
using ZStewart.KOSLisp.Types.Helpers;

namespace ZStewart.KOSLisp.Compile.SpecialForms {
  /// <summary>
  /// A special form that assigns a variable to a value.
  /// </summary>
  public class SetVarSpecialForm : SpecialForm {
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
    public virtual AstOp ToAst(
        LispObject expression, Context context, SemanticAnalyzer compiler) {
      int len;
      try {
        len = ListOperations.Count(expression);
      } catch (ExceptionWrapper ex) {
        throw ExceptionType.ThrowSyntaxError(
          ex, "set-var expression must be a propper list");
      }
      if (len < 1) {
        throw ExceptionType.ThrowSyntaxError(
          "set-var expression requires at least a variable to set");
      } else if (len > 2) {
        throw ExceptionType.ThrowSyntaxError(
          "set-var expression expected at most 2 arguments (a var and value), got {1}",
          len);
      }

      AstOp shouldBeBinding = compiler.ToAst(ListOperations.GetCar(expression), context);
      if (!(shouldBeBinding is AstBinding)) {
        throw ExceptionType.ThrowSyntaxError("set-var expression can only set variables");
      }
      AstBinding binding = (AstBinding)shouldBeBinding;

      AstOp value;
      if (len < 2) {
        value = Ast.Const(NilType.Nil);
      } else {
        value = compiler.ToAst(
          ListOperations.GetCar(ListOperations.GetCdr(expression)), context);
      }
      return Ast.Set(binding, value);
    }
  }
}
