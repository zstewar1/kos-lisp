using System.Collections.Generic;

using static ZStewart.KOSLisp.Types.ExceptionType;

using ZStewart.KOSLisp.Compile.AST;
using ZStewart.KOSLisp.Compile.Contexts;
using ZStewart.KOSLisp.Types;
using ZStewart.KOSLisp.Types.Helpers;

namespace ZStewart.KOSLisp.Compile.SpecialForms {
  /// <summary>
  /// A special form which represents importing a module.
  /// </summary>
  public class ImportSpecialForm : SpecialForm {
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
        throw ThrowSyntaxError(ex, "import expression must be a propper list");
      }
      if (len < 1) {
        throw ThrowSyntaxError(
          "import expression expected at least 1 arguments, got {0}", len);
      } else if (len != 1 && len != 3) {
        throw ThrowSyntaxError(
          "import expression must either be in the form (import module) or " +
          "(import module as name)");
      }

      var modname = GetModuleName(ListOperations.GetCar(expression));
      SymbolType ident;
      if (len == 1) {
        ident = SymbolType.Create(modname[modname.Count - 1]);
      } else {
        expression = ListOperations.GetCdr(expression);
        if (ListOperations.GetCar(expression) != SymbolType.Create("as")) {
          throw ThrowSyntaxError(
            "module name must be followed with 'as for import-as");
        }
        var modas = ListOperations.GetCar(ListOperations.GetCdr(expression));
        if (!(modas is SymbolType)) {
          throw ThrowSyntaxError(
            "module name for import-as must be a symbol, got {0}", modas.__class__);
        }
        ident = (SymbolType)modas;
        if (ident.IsSelfEvaluating) {
          throw ThrowSyntaxError(
            "cannot bind imported module to self-evaluating symbol {0}", ident);
        }
      }

      return Ast.Import(modname, context.AddBinding(ident));
    }

    private List<string> GetModuleName(LispObject modname) {
      if (modname is SymbolType) {
        return new List<string>() {CheckModnameSymbol(modname)};
      } else if (modname is ConsType) {
        int len;
        try {
          len = ListOperations.Count(modname);
        } catch (ExceptionWrapper ex) {
          throw ThrowSyntaxError(ex, "subreference module name must be a proper list");
        }
        if (len != 3) {
          throw ThrowSyntaxError("subreference module name must be of length 3");
        }
        var ga = ListOperations.GetCar(modname);
        if (ga != SymbolType.Create("getattr")) {
          throw ThrowSyntaxError("subreferences must start with getattr");
        }
        modname = ListOperations.GetCdr(modname);
        var rest = GetModuleName(ListOperations.GetCar(modname));
        modname = ListOperations.GetCdr(modname);
        var ident = CheckModnameSymbol(ListOperations.GetCar(modname));
        rest.Insert(0, ident);
        return rest;
      } else {
        throw ThrowSyntaxError(
          "module name must be a symbol or cons, got {0}", modname.__class__);
      }
    }

    /// <summary>
    /// Check that the given component is a symbol and that it is not self evaluating, and
    /// return its identifier.
    /// </summary>
    private string CheckModnameSymbol(LispObject shouldBeSym) {
      if (!(shouldBeSym is SymbolType)) {
        throw ThrowSyntaxError("identifier in module name should be a symbol");
      }
      var sym = (SymbolType)shouldBeSym;
      if (sym.IsSelfEvaluating) {
        throw ThrowSyntaxError("module names cannot be self-evaluating symbols");
      }
      return sym.Identifier;
    }
  }
}
