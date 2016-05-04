using ZStewart.KOSLisp.Compile.AST;
using ZStewart.KOSLisp.Compile.Contexts;
using ZStewart.KOSLisp.Types;
using ZStewart.KOSLisp.Types.Helpers;

namespace ZStewart.KOSLisp.Compile.SpecialForms {
  /// <summary>
  /// A special form at converts any expression passed to it to a constant expression.
  /// </summary>
  public class QuoteSpecialForm : SpecialForm {
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
    public AstOp ToAst(LispObject expression, Context context, Compiler compiler) {
      int len;
      try {
        len = ListOperations.Count(expression);
      } catch (ExceptionWrapper ex) {
        throw ExceptionType.ThrowSyntaxError(
          ex,
          "illegal quote expression: not a proper list. note: " +
          "'(a . b) => (quote (a . b)); (quote a . b) is illegal");
      }
      if (len != 1) {
        throw ExceptionType.ThrowSyntaxError(
          "quote expected 1 argument, got {0}. note: " +
          "'(a b c) => (quote (a b c)); (quote a b c) is illegal",
          len);
      }

      return Ast.Const(ListOperations.GetCar(expression));
    }
  }
}
