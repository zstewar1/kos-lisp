using System;
using System.Collections.Generic;
using System.Linq;

using static ZStewart.KOSLisp.Types.ExceptionType;

using ZStewart.KOSLisp.Compile.AST;
using ZStewart.KOSLisp.Compile.Contexts;
using ZStewart.KOSLisp.Types;
using ZStewart.KOSLisp.Types.Helpers;

namespace ZStewart.KOSLisp.Compile.SpecialForms {
  /// <summary>
  /// A special form which represents a try-catch.
  /// </summary>
  public class TrySpecialForm : SpecialForm {
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
    public AstOp ToAst(
        LispObject expression, Context context, SemanticAnalyzer compiler) {
      int len;
      try {
        len = ListOperations.Count(expression);
      } catch (ExceptionWrapper ex) {
        throw ThrowSyntaxError(ex, "try-catch expression must be a proper list");
      }
      if (len < 1) {
        throw ThrowSyntaxError("try-catch must have a guarded expression");
      }

      var guarded = compiler.ToAst(ListOperations.GetCar(expression), context);

      expression = ListOperations.GetCdr(expression);

      var catches = new List<Tuple<AstOp, AstBinding, AstOp>>();
      AstOp @finally = null;

      while (!ReferenceEquals(expression, NilType.Nil)) {
        var marker = ListOperations.GetCar(expression);
        expression = ListOperations.GetCdr(expression);
        if (marker == SymbolType.Create("catch")) {
          if (ReferenceEquals(expression, NilType.Nil)) {
            throw ThrowSyntaxError(
              "incomplete catch expression: missing exception type and fallback");
          }
          var exception_expr = ListOperations.GetCar(expression);
          expression = ListOperations.GetCdr(expression);
          if (ReferenceEquals(expression, NilType.Nil)) {
            throw ThrowSyntaxError(
              "incomplete catch expression: missing fallback");
          }

          SymbolType binding_expr = null;
          var next = ListOperations.GetCar(expression);
          if (next == KeywordSymbolType.Create(":as")) {
            expression = ListOperations.GetCdr(expression);
            if (ReferenceEquals(expression, NilType.Nil)) {
              throw ThrowSyntaxError(
                "incomplete catch-as expression: missing binding and fallback");
            }
            var binding_obj = ListOperations.GetCar(expression);
            if (!(binding_obj is SymbolType)) {
              throw ThrowSyntaxError("exception binding must be a symbol");
            }
            binding_expr = (SymbolType)binding_obj;
            if (binding_expr.IsSelfEvaluating) {
              throw ThrowSyntaxError(
                "exception binding cannot be a self-evaluating symbol");
            }
            expression = ListOperations.GetCdr(expression);
            if (ReferenceEquals(expression, NilType.Nil)) {
              throw ThrowSyntaxError("incomplete catch-as expression: missing fallback");
            }
            next = ListOperations.GetCar(expression);
          }
          var fallback_expr = next;

          expression = ListOperations.GetCdr(expression);

          var exception = compiler.ToAst(exception_expr, context);
          AstBinding binding;
          AstOp fallback;
          if (binding_expr == null) {
            binding = null;
            fallback = compiler.ToAst(fallback_expr, context);
          } else {
            var innerContext = new ScopedContext(context);
            binding = innerContext.AddBinding(binding_expr);
            fallback = compiler.ToAst(fallback_expr, innerContext);
          }
          catches.Add(Tuple.Create(exception, binding, fallback));
        } else if (marker == SymbolType.Create("finally")) {
          if (ReferenceEquals(expression, NilType.Nil)) {
            throw ThrowSyntaxError("incomplete finally expression");
          }
          var expr = ListOperations.GetCar(expression);
          expression = ListOperations.GetCdr(expression);
          if (!ReferenceEquals(expression, NilType.Nil)) {
            throw ThrowSyntaxError("unexpected expression after finally");
          }
          @finally = compiler.ToAst(expr, context);
        } else {
          throw ThrowSyntaxError(
            "expressions after guarded expression must either be catch or finally (did " +
            "you mean to use progn?)");
        }
      }

      if (catches.Count == 0 && @finally == null) {
        throw ThrowSyntaxError(
          "try must have either at least one catch or a finally expression");
      }

      return Ast.Try(guarded, catches, @finally);
    }
  }
}
