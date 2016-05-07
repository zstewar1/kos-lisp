using System.Collections.Generic;
using System.Linq;

using ZStewart.KOSLisp.Compile.AST;
using ZStewart.KOSLisp.Compile.Contexts;
using ZStewart.KOSLisp.Types;
using ZStewart.KOSLisp.Types.Helpers;

namespace ZStewart.KOSLisp.Compile.SpecialForms {
  /// <summary>
  /// A special form that allows you to do multiple things in sequence.
  /// </summary>
  public class PrognSpecialForm : SpecialForm {
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
      return Ast.Progn(ParseForms(expression, context, compiler));
    }

    /// <summary>
    /// Pares a set of forms from the given lisp object. The object must be a proper list
    /// of forms. This method allows child classes which overide the ToAst method to
    /// explicitly reference the form-parsing mechanism.
    /// </summary>
    protected IEnumerable<AstOp> ParseForms(
        LispObject forms, Context context, SemanticAnalyzer compiler) {
      if (!ListOperations.Proper(forms)) {
        throw ExceptionType.ThrowSyntaxError(
          "progn forms list must be a proper list");
      }

      return ListOperations.IterList(forms).Select(form => compiler.ToAst(form, context));
    }
  }
}
