using System.Collections.Generic;
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
    public AstOp ToAst(
        LispObject expression, Context context, SemanticAnalyzer compiler) {
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

      return CreateFuncCall(fnOrMacro, rest, context, compiler);
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

    /// <summary>
    /// Create an AstOp representing the call to the given function.
    /// </summary>
    protected virtual AstOp CreateFuncCall(
        AstOp fn, LispObject args, Context context, SemanticAnalyzer compiler) {
      var positionalArgs = ReadPositionalArgs(ref args, context, compiler);
      var keywordArgs = ReadKeywordArgs(ref args, context, compiler);
      return Ast.Call(fn, positionalArgs, keywordArgs);
    }

    /// <summary>
    /// Read the argument list until a keyword is found, adding the value of each argument
    /// to the positional argument list.
    /// </summary>
    protected virtual List<AstOp> ReadPositionalArgs(
        ref LispObject args, Context context, SemanticAnalyzer compiler) {
      List<AstOp> result = new List<AstOp>();
      for (; !ReferenceEquals(args, NilType.Nil); args = ListOperations.GetCdr(args)) {
        var current = ListOperations.GetCar(args);
        // Stop parsing keyword arguments once we find a keyword.
        if (current is KeywordSymbolType) {
          break;
        }
        result.Add(compiler.ToAst(current, context));
      }
      return result;
    }

    /// <summary>
    /// Read keyword-argument pairs from the argument list, raising errors for invalid
    /// states of keyword arguments.
    /// </summary>
    protected virtual Dictionary<SymbolType, AstOp> ReadKeywordArgs(
        ref LispObject args, Context context, SemanticAnalyzer compiler) {
      var result = new Dictionary<SymbolType, AstOp>();
      for (; !ReferenceEquals(args, NilType.Nil); args = ListOperations.GetCdr(args)) {
        var current = ListOperations.GetCar(args);
        if (!(current is KeywordSymbolType)) {
          throw ThrowSyntaxError("positional argument follows keyword argument");
        }
        var sym = ((KeywordSymbolType)current).Unprefix();
        if (result.ContainsKey(sym)) {
          throw ThrowSyntaxError("duplicate keyword {0}", sym);
        }
        args = ListOperations.GetCdr(args);
        if (ReferenceEquals(args, NilType.Nil)) {
          throw ThrowSyntaxError("end of argument list while reading keyword {0}", sym);
        }
        current = ListOperations.GetCar(args);
        if (current is KeywordSymbolType) {
          throw ThrowSyntaxError(
            "unmatched keyword {0} (did you mean to quote {1}?)", sym, current);
        }
        result.Add(sym, compiler.ToAst(current, context));
      }
      return result;
    }
  }
}
