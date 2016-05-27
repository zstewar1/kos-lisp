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
  /// Special form that represents declaring a named macro and binding it to a symbol.
  /// </summary>
  public class DefmacroSpecialForm : LambdaSpecialForm {
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
      int len;
      try {
        len = ListOperations.Count(expression);
      } catch (ExceptionWrapper ex) {
        throw ThrowSyntaxError(
          ex, "macro definition expression must be a proper list");
      }
      if (len < 2) {
        throw ThrowSyntaxError(
          "macro definition expression requires at least a macro name and args list");
      }

      var nameObj = ListOperations.GetCar(expression);
      if (!(nameObj is SymbolType)) {
        throw ThrowSyntaxError(
          "macro name must be a symbol, got {0}", nameObj.__class__);
      }
      var name = (SymbolType)nameObj;
      if (name.IsSelfEvaluating) {
        throw ThrowSyntaxError(
          "cannot declare macro with self-evaluating name {0}", name);
      }

      var rest = ListOperations.GetCdr(expression);
      List<Tuple<ArgumentProperties, AstBinding, AstOp>> args;
      List<AstOp> forms;
      ParseArgsAndForms(rest, context, compiler, out args, out forms);

      if (args.Select(arg => arg.Item1.Type)
            .Any(type => type == ArgumentType.Keyword ||
              type == ArgumentType.RestKwCapture ||
              type == ArgumentType.RestKwIgnore)) {
        throw ThrowSyntaxError("macros cannot take keyword arguments");
      }

      // Macro definition adds binding after parsing args and forms. Macros cannot
      // recurse, because the macro definition is needed at macro definition time.
      // However, they can have recursive behavior by returning an expression which
      // contains another invocation of the macro.
      var binding = context.AddBinding(name);

      return Ast.Defmacro(binding, args, forms);
    }
  }
}
