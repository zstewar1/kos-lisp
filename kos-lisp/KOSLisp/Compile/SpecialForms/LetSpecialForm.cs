using System;
using System.Collections.Generic;

using ZStewart.KOSLisp.Compile.AST;
using ZStewart.KOSLisp.Compile.Contexts;
using ZStewart.KOSLisp.Types;
using ZStewart.KOSLisp.Types.Helpers;

namespace ZStewart.KOSLisp.Compile.SpecialForms {
  /// <summary>
  /// A special form that evaluates a set of expressions in the context of a new set of
  /// variable bindings.
  /// </summary>
  public class LetSpecialForm : PrognSpecialForm {
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
    public override AstOp ToAst(LispObject expression, Context context, Compiler compiler) {
      int len;
      try {
        len = ListOperations.Count(expression);
      } catch (ExceptionWrapper ex) {
        throw ExceptionType.ThrowSyntaxError(ex, "let expression must be a proper list");
      }

      // Empty let expression evaluates to nil.
      if (len == 0) {
        return Ast.Const(NilType.Nil);
      }

      var bindingList = ListOperations.GetCar(expression);
      var rest = ListOperations.GetCdr(expression);

      Context innerContext;
      var bindings = ParseBindingList(bindingList, context, compiler, out innerContext);
      var forms = ParseForms(rest, innerContext, compiler);

      return Ast.Let(bindings, forms);
    }

    /// <summary>
    /// Parse the bindings from an expression representing a list of bindings for a let
    /// expression.
    ///
    /// Each binding in the list can either be a symbol represnting a new variable to
    /// bind, in which case that symbol is bound to nil, or a two-tuple of a symbol and an
    /// expresison to evaluate as the value to bind the variable to.
    /// </summary>
    private IEnumerable<Tuple<AstBinding, AstOp>> ParseBindingList(
        LispObject bindingList, Context context, Compiler compiler,
        out Context innerContext) {
      // Create a list to hold the new bindings and a context to bind them in.
      var bindings = new List<Tuple<AstBinding, AstOp>>();
      // The new context has the old context as its parent to preserve static scoping, and
      // is not a closured context because a let expression cannot be extracted from its
      // parent scope and evaluated elsewhere.
      innerContext = new ScopedContext(context);

      foreach (var newbind in ListOperations.IterList(bindingList)) {
        // Check whether the new binding is a symbol or a symbol-value tuple.
        if (newbind is SymbolType) {
          if (SymbolType.IsSelfEvaluating((SymbolType)newbind)) {
            throw ExceptionType.ThrowSyntaxError(
              "cannot bind self-evaluating symbol {0}", newbind);
          }

          var binding = innerContext.AddBinding((SymbolType)newbind);
          bindings.Add(Tuple.Create<AstBinding, AstOp>(binding, Ast.Const(NilType.Nil)));
        } else {
          int len;
          try {
            len = ListOperations.Count(newbind);
          } catch (ExceptionWrapper ex) {
            throw ExceptionType.ThrowSyntaxError(ex, "binding must be a proper list");
          }
          // Should never get len < 1 because only Nil has len < 1, and Nil is covered
          // under SymbolType.
          if (len < 1 || len > 2) {
            throw ExceptionType.ThrowSyntaxError(
              "binding expression must have length 1 or 2, was {0}", len);
          }

          var symb = ListOperations.GetCar(newbind);
          if (!(symb is SymbolType)) {
            throw ExceptionType.ThrowSyntaxError(
              "variable to bind must be a symbol, was {0}", symb.__class__);
          } else if(SymbolType.IsSelfEvaluating((SymbolType)symb)) {
            throw ExceptionType.ThrowSyntaxError(
              "cannot bind self-evaluating symbol {0}", symb);
          }

          LispObject value = NilType.Nil;
          if (len == 2) {
            value = ListOperations.GetCar(ListOperations.GetCdr(newbind));
          }

          // Add the binding after parsing the bound value. This gives semantics where the
          // expression inside of a binding cannot reference itself (since it would not
          // yet have an assigned value), but later bindings can access earlier bindings.
          var boundValue = compiler.ToAst(value, innerContext);
          var binding = innerContext.AddBinding((SymbolType)symb);
          bindings.Add(Tuple.Create(binding, boundValue));
        }
      }
      return bindings;
    }
  }
}
