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
      }

      var modname = GetModuleName(ListOperations.GetCar(expression));

      if (len == 1) {
        return Ast.Import(
          modname, context.AddBinding(SymbolType.Create(modname[modname.Count-1])));
      }

      expression = ListOperations.GetCdr(expression);
      var op = ListOperations.GetCar(expression);
      if (op == KeywordSymbolType.Create(":")) {
        return CreateFromImport(modname, ListOperations.GetCdr(expression), context);
      } else if (op == KeywordSymbolType.Create(":all")) {
        return CreateAllImport(modname, ListOperations.GetCdr(expression), context);
      } else if (op == KeywordSymbolType.Create(":as")) {
        return CreateAsImport(modname, ListOperations.GetCdr(expression), context);
      } else {
        throw ThrowSyntaxError(
          "unknown import operation, must be one of ': ':all or ':as");
      }
    }

    /// <summary>
    /// Reads the specified lisp object as a module name. Raises a syntax error if it is
    /// not a recursive sequence of getattr conses where the inner-most element is a
    /// symbol and the keys are all symbols.
    /// </summary>
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

    /// <summary>
    /// Create an import expression which imports the specified module and binds a set of
    /// keys from the module to (optionally specified) bindings in the current context.
    /// </summary>
    private AstOp CreateFromImport(
        List<string> modname, LispObject args, Context context) {

    }

    /// <summary>
    /// Create an import expression which loads all symbols from the imported module into
    /// the current module.
    /// </summary>
    private AstOp CreateAllImport(
        List<string> modname, LispObject args, Context context) {
      if (!(context is GlobalContext)) {
        throw ThrowSyntaxError("import :all only allowed at the module level");
      }
      if (!ReferenceEquals(args, NilType.Nil)) {
        throw ThrowSyntaxError("import :all takes no additional arguments");
      }
      return Ast.Import(modname, ((GlobalContext)context).Module);
    }

    /// <summary>
    /// Create an import expression which imports the given module and binds it to a
    /// symbol specified in args.
    /// </summary>
    private AstOp CreateAsImport(
        List<string> modname, LispObject args, Context context) {
      int len;
      try {
        len = ListOperations.Count(args);
      } catch (ExceptionWrapper ex) {
        throw ThrowSyntaxError(ex, "arguments to import :as must be a proper list");
      }
      if (len != 1) {
        throw ThrowSyntaxError("import :as expected 1 additional argument, got {0}", len);
      }
      var arg = ListOperations.GetCar(args);
      if (!(arg is SymbolType)) {
        throw ThrowSyntaxError(
          "symbol to bind imported module as must be a symbol, got {0}",
          arg.__class__);
      }
      var sym = (SymbolType)arg;
      if (sym.IsSelfEvaluating) {
        throw ThrowSyntaxError(
          "cannot bind imported module to self-evaluating symbol '{0}", sym);
      }
      return Ast.Import(modname, context.AddBinding(sym));
    }
  }
}
