using ZStewart.KOSLisp.Compile.AST;
using ZStewart.KOSLisp.Compile.Contexts;
using ZStewart.KOSLisp.Types;
using ZStewart.KOSLisp.Types.Helpers;

namespace ZStewart.KOSLisp.Compile.SpecialForms {
  /// <summary>
  /// A special form that represents a conditional operation. It creates an AstIf by
  /// reading the condition, if-true, and if-false from the cons list given. The if
  /// special form accepts either two or three arguments. In the two-argument version, the
  /// value-if-false is implicitly null.
  /// </summary>
  public class IfSpecialForm : SpecialForm {
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
        throw ExceptionType.ThrowSyntaxError(ex, "if expression must be a propper list");
      }
      if (len < 2) {
        throw ExceptionType.ThrowSyntaxError(
          "if expression expected at least 2 arguments, got {0}", len);
      } else if (len > 3) {
        throw ExceptionType.ThrowSyntaxError(
          "if expression expected at most 3 arguments, got {0}", len);
      }

      // Step through the argument list, collecting the condition and outcomes.
      AstOp cond = compiler.ToAst(ListOperations.GetCar(expression), context);

      expression = ListOperations.GetCdr(expression);
      AstOp ifTrue = compiler.ToAst(ListOperations.GetCar(expression), context);

      AstOp ifFalse;
      if (len == 3) {
        expression = ListOperations.GetCdr(expression);
        ifFalse = compiler.ToAst(ListOperations.GetCar(expression), context);
      } else {
        ifFalse = Ast.Const(NilType.Nil);
      }

      // Check if the condition is a constant, and if so, check if it has a known
      // compile-time boolean value. If it does, just return the result of compiling the
      // appropriate result value.
      //
      // Note that we have already compiled both the ifTrue and ifFalse values while
      // retrieving them. Doing that before checking the condition prevents writing
      // semantically-illegal expressions in the unchecked branch of the if.
      if (cond is AstConst) {
        LispObject b = null;
        try {
          b = BoolType.From(((AstConst)cond).Value);
        } catch (ExceptionWrapper) {}
        if (b != null) {
          // If it has returned at all (b != null), BoolType.From is guaranteed to have
          // returned either T or F.
          if (b == BoolType.T) {
            return ifTrue;
          } else {
            return ifFalse;
          }
        }
      }

      return Ast.If(cond, ifTrue, ifFalse);
    }
  }
}
