using System;
using System.Collections.Generic;
using System.Text;

using ZStewart.KOSLisp.Types;

namespace ZStewart.KOSLisp.Compile.AST {
  /// <summary>
  /// A class with factory methods for the various AST types.
  ///
  /// This allows the AST op implementations to remain effectively sealed even when they
  /// allow inheritance
  /// </summary>
  public static class Ast {
    /// <summary>
    /// Creates an AstOp that evaluates to a constant value.
    /// </summary>
    public static AstConst Const(LispObject value) { return new AstConst(value); }

    /// <summary>
    /// Creates an AstOp that sets the given variable to the given value.
    /// </summary>
    public static AstSetVar Set(AstBinding variable, AstOp value) {
      return new AstSetVar(variable, value);
    }

    /// <summary>
    /// Creates a new AstOp that calls the given function expression with the given
    /// arguments.
    /// </summary>
    public static AstFuncCall Call(AstOp function, IEnumerable<AstOp> args) {
      return new AstFuncCall(function, args);
    }

    /// <summary>
    /// Creates a new AstOp that calls the given function expression with the given
    /// arguments.
    /// </summary>
    public static AstFuncCall Call(AstOp function, params AstOp[] args) {
      return Call(function, (IEnumerable<AstOp>)args);
    }

    /// <summary>
    /// Creates a new AstOp that evaluates a condition then selects which subtree to
    /// evaluate based on the condition.
    /// </summary>
    public static AstIf If(AstOp condition, AstOp valueIfTrue, AstOp valueIfFalse) {
      return new AstIf(condition, valueIfTrue, valueIfFalse);
    }

    /// <summary>
    /// Creates a new AstOp that imports a module and saves it in the given binding.
    /// </summary>
    public static AstImport Import(
        IEnumerable<string> moduleIdentifier, AstBinding name) {
      return new AstImport(moduleIdentifier, name);
    }

    /// <summary>
    /// Creates a new AstOp that evaluates a series of forms in sequence, then returns the
    /// result of the last one.
    /// </summary>
    public static AstProgn Progn(IEnumerable<AstOp> forms) {
      return new AstProgn(forms);
    }

    /// <summary>
    /// Creates a new AstOp that evaluates a series of forms in sequence, then returns the
    /// result of the last one.
    /// </summary>
    public static AstProgn Progn(params AstOp[] forms) {
      return Progn((IEnumerable<AstOp>)forms);
    }

    /// <summary>
    /// Creates a new AstOp that binds a series of new variables, then evaluates a seiries
    /// of forms in the context of those variables and returns the result of the lat form
    /// evaluated.
    /// </summary>
    public static AstLet Let(
        IEnumerable<Tuple<AstBinding, AstOp>> bindings,
        IEnumerable<AstOp> forms) {
      return new AstLet(bindings, forms);
    }

    /// <summary>
    /// Creates a new AstOp that binds a series of new variables, then evaluates a seiries
    /// of forms in the context of those variables and returns the result of the lat form
    /// evaluated.
    /// </summary>
    public static AstLet Let(
        IEnumerable<Tuple<AstBinding, AstOp>> bindings,
        params AstOp[] forms) {
      return Let(bindings, (IEnumerable<AstOp>)forms);
    }

    /// <summary>
    /// Creates a new AstOp that represents a function with the given arguments that
    /// evaluates the specified forms.
    /// </summary>
    public static AstLambda Lambda(
        IEnumerable<AstBinding> args,
        IEnumerable<AstOp> forms) {
      return new AstLambda(args, forms);
    }

    /// <summary>
    /// Creates a new AstOp that represents a function with the given arguments that
    /// evaluates the specified forms.
    /// </summary>
    public static AstLambda Lambda(
        IEnumerable<AstBinding> args,
        params AstOp[] forms) {
      return Lambda(args, (IEnumerable<AstOp>)forms);
    }

    /// <summary>
    /// Creates a new AstOp that represents defining and binding a named function with the
    /// given arguments that evaluates the specified forms.
    /// </summary>
    public static AstDefun Defun(
        AstBinding name,
        IEnumerable<AstBinding> args,
        IEnumerable<AstOp> forms) {
      return new AstDefun(name, args, forms);
    }

    /// <summary>
    /// Creates a new AstOp that represents defining and binding a named function with the
    /// given arguments that evaluates the specified forms.
    /// </summary>
    public static AstDefun Defun(
        AstBinding name,
        IEnumerable<AstBinding> args,
        params AstOp[] forms) {
      return Defun(name, args, (IEnumerable<AstOp>)forms);
    }

    /// <summary>
    /// Creates a new AstOp that represents defining and binding a macro with the given
    /// arguments that evaluates the specified forms.
    /// </summary>
    public static AstDefmacro Defmacro(
        AstBinding name,
        IEnumerable<AstBinding> args,
        IEnumerable<AstOp> forms) {
      return new AstDefmacro(name, args, forms);
    }

    /// <summary>
    /// Creates a new AstOp that represents defining and binding a macro with the given
    /// arguments that evaluates the specified forms.
    /// </summary>
    public static AstDefmacro Defmacro(
        AstBinding name,
        IEnumerable<AstBinding> args,
        params AstOp[] forms) {
      return Defmacro(name, args, (IEnumerable<AstOp>)forms);
    }

    /// <summary>
    /// Create a global which is bound to the given symbol from the given module.
    /// </summary>
    public static AstGlobalBinding BindGlobal(ModuleType module, SymbolType symbol) {
      return new AstGlobalBinding(module, symbol);
    }

    /// <summary>
    /// Create a local variable for the given symbol. The binding defaults to non-closure.
    /// </summary>
    public static AstLocalBinding BindLocal(SymbolType symbol) {
      return BindLocal(symbol);
    }

    /// <summary>
    /// Convenience function to allow appending an AST op as if it is a method on the
    /// string builder.
    /// </summary>
    internal static StringBuilder AppendIndented(
        this StringBuilder sb, AstOp op, int baseIndent) {
      return op.AppendAstStringIndented(sb, baseIndent);
    }
  }
}
