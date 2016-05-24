using System.Linq;

using static ZStewart.KOSLisp.Types.ExceptionType;

using ZStewart.KOSLisp.Compile.AST;
using ZStewart.KOSLisp.Compile.Contexts;
using ZStewart.KOSLisp.Types;
using ZStewart.KOSLisp.Types.Helpers;

namespace ZStewart.KOSLisp.Compile.SpecialForms {
  /// <summary>
  /// A nameless special form that represents the action of calling a function.
  /// </summary>
  public class FuncCallOrMacroSpecialForm : SpecialForm {
    /// <summary>
    /// An object which supports applying a macro specified as an Ast.
    /// </summary>
    protected readonly MacroExpander macroExpander;

    /// <summary>
    /// Create a FuncCallOrMacro form specifiying the macro expander to be used.
    /// </summary>
    public FuncCallOrMacroSpecialForm(MacroExpander macroExpander) {
      this.macroExpander = macroExpander;
    }

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
      if (!ListOperations.Proper(expression)) {
        throw ExceptionType.ThrowSyntaxError(
          "function call or macro expression must be a proper list");
      }
      var fnOrMacro = compiler.ToAst(ListOperations.GetCar(expression), context);
      var rest = ListOperations.GetCdr(expression);

      LispObject replacement;
      // try to expand the name as a macro, and if it succeeds, replace this expressin
      // with the expanded result by calling back to the compiler.
      if (TryExpand(fnOrMacro, rest, out replacement)) {
        return compiler.ToAst(replacement, context);
      }

      // Otherwise just expand as a plain function call.
      // TODO(zstewar1): expand with keyword arguments.
      var args = ListOperations.IterList(rest)
        .Select(arg => compiler.ToAst(arg, context));

      return Ast.Call(fnOrMacro, args);
    }

    /// <summary>
    /// Checks if the given macro expression can be expanded and tries to expand it.
    /// Returns true with a valid expression if the expansion succeeds, in which case, the
    /// current expression should be replaced with the result expression.
    /// </summary>
    protected virtual bool TryExpand(
        AstOp macro, LispObject args, out LispObject result) {
      if (!CheckValidMacroNameExpression(macro)) {
        result = null;
        return false;
      }
      result = macroExpander.Expand(macro, args);
      return result != null;
    }

    /// <summary>
    /// Checks if the given AST represents an aceptable macro-name expression.a valid
    /// macro name expression is either a global binding or a linearly nested series of
    /// (getattr) calls where the innermost object is a global binding and the attribute
    /// to get is always a constant, non-self-evaluating symbol.
    /// </summary>
    protected virtual bool CheckValidMacroNameExpression(AstOp macro) {
      if (macro is AstGlobalBinding) {
        return true;
      }
      if (!(macro is AstFuncCall)) {
        return false;
      }
      var m = (AstFuncCall)macro;
      if (!(m.Function is AstGlobalBinding) ||
          m.PositionalArguments.Count != 2 ||
          m.KeywordArguments.Count != 0 ||
          !(m.PositionalArguments[1] is AstConst) ||
          !CheckValidMacroNameExpression(m.PositionalArguments[0])) {
        return false;
      }
      var f = (AstGlobalBinding)m.Function;
      var c = (AstConst)m.PositionalArguments[1];
      if (f.Symbol != SymbolType.Create("getattr") ||
          !(c.Value is SymbolType) ||
          ((SymbolType)c.Value).IsSelfEvaluating) {
        return false;
      }
      // We should now have excluded everything besides the recursive pattern:
      // (getattr (getattr global 'b) 'c)
      // i.e. the following pseudo bnf:
      // macro_name ::= global_name | macro_getattr
      // global_name ::= <AST-Global-Binding: *>
      // macro_getattr ::= <AST-Func-Call
      //   <AST-Global-Binding: 'GETATTR>
      //   macro_name
      //   <AST-Constant: SymbolType && !SymbolType::IsSelfEvaluating>>
      return true;
    }
  }
}
