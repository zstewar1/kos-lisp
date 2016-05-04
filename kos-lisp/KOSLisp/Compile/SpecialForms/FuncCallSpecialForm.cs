using System.Linq;

using ZStewart.KOSLisp.Compile.AST;
using ZStewart.KOSLisp.Compile.Contexts;
using ZStewart.KOSLisp.Types;
using ZStewart.KOSLisp.Types.Helpers;

namespace ZStewart.KOSLisp.Compile.SpecialForms {
  /// <summary>
  /// A nameless special form that represents the action of calling a function.
  /// </summary>
  public class FuncCallSpecialForm : SpecialForm {
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
      if (!ListOperations.Proper(expression)) {
        throw ExceptionType.ThrowSyntaxError(
          "function call expression must be a proper list");
      }
      var fn = compiler.ToAst(ListOperations.GetCar(expression), context);
      var args = ListOperations.IterList(ListOperations.GetCdr(expression))
        .Select(arg => compiler.ToAst(arg, context));

      return Ast.Call(fn, args);
    }
  }
}
