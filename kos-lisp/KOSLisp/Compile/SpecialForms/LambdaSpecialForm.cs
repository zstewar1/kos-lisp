using System.Collections.Generic;
using System.Linq;

using ZStewart.KOSLisp.Compile.AST;
using ZStewart.KOSLisp.Compile.Contexts;
using ZStewart.KOSLisp.Types;
using ZStewart.KOSLisp.Types.Helpers;

namespace ZStewart.KOSLisp.Compile.SpecialForms {
  /// <summary>
  /// A form that parses an unnamed function definition.
  /// </summary>
  public class LambdaSpecialForm : PrognSpecialForm {
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
    public override AstOp ToAst(
        LispObject expression, Context context, SemanticAnalyzer compiler) {
      IEnumerable<AstBinding> args;
      IEnumerable<AstOp> forms;
      ParseArgsAndForms(expression, context, compiler, out args, out forms);
      return Ast.Lambda(args, forms);
    }

    /// <summary>
    /// Parses out just the arguments and forms of a lambda expression. This separates out
    /// parsing these parts of the expression from the ToAst logic to make it easier for
    /// derived classes to override the behavior of the lambda.
    /// </summary>
    protected void ParseArgsAndForms(
        LispObject expression, Context context, SemanticAnalyzer compiler,
        out IEnumerable<AstBinding> args, out IEnumerable<AstOp> forms) {
      int len;
      try {
        len = ListOperations.Count(expression);
      } catch (ExceptionWrapper ex) {
        throw ExceptionType.ThrowSyntaxError(
          ex, "lambda expression must be a proper list");
      }

      if (len < 1) {
        throw ExceptionType.ThrowSyntaxError(
          "function definition requires argument list");
      }

      // Get the list of bindings (for now this just means position arguments), however:
      // TODO(zstewar1): Support postitional &rest args (keyword args) &kw kwargs.
      var symargs = ParseArgumentList(ListOperations.GetCar(expression));

      // Create a scoped context for the new variables.
      var innerContext = new ClosuredScopedContext(context);
      // Bind all of the parsed symbols in that context.
      // Note: run ToList() on it to force it to be evaluated immediately, otherwise the
      // bindings will not exist when we parse the forms.
      args = symargs.Select(s => innerContext.AddBinding(s)).ToList();

      // Parse the forms in the context of the new bindings.
      forms = ParseForms(ListOperations.GetCdr(expression), innerContext, compiler);
    }

    /// <summary>
    /// Convert from the arglist to an enumerable of the symbols which we'll want to add
    /// bindings for.
    /// </summary>
    protected IEnumerable<SymbolType> ParseArgumentList(LispObject arglist) {
      // TODO(zstewar1): handle the more complicated argument lists which we actually want
      // to support (this requries AST and Generator changes as well).
      var bindings = new List<SymbolType>();
      foreach (var newbind in ListOperations.IterList(arglist)) {
        if (!(newbind is SymbolType)) {
          throw ExceptionType.ThrowSyntaxError(
            "argument binding must be symbol not {0}", newbind.__class__);
        }
        var symb = (SymbolType)newbind;
        if (SymbolType.IsSelfEvaluating(symb)) {
          throw ExceptionType.ThrowSyntaxError(
            "cannot bind self-evaluating symbol {0}", symb);
        } else if (bindings.Contains(symb)) {
          throw ExceptionType.ThrowSyntaxError(
            "symbol {0} bound twice", symb);
        }
        bindings.Add(symb);
      }
      return bindings;
    }
  }
}
