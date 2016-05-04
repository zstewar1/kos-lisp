using System;
using System.Collections.Immutable;
using System.Linq.Expressions;

using ZStewart.KOSLisp.Compile.AST;
using ZStewart.KOSLisp.Types;

namespace ZStewart.KOSLisp.Compile.Generators.CSharp {
  /// <summary>
  /// A generator which creates an expression that represents a conditional expression.
  /// </summary>
  public sealed class CSharpGlobalBindingGenerator : CSharpBindingGenerator {
    /// <summary>
    /// The module-level symbol to look up or set with this binding.
    /// </summary>
    public SymbolType Symbol { get; }

    /// <summary>
    /// The module that this symbol is bound in.
    /// </summary>
    public ModuleType Module { get; }

    /// <summary>
    /// Creates a global binding generator.
    /// </summary>
    /// <param name="op">
    /// The AstGlobalBinding that this generator will create code for.
    /// </param>
    /// <param name="factory">
    /// A generator factory that can be used to get generators for the AST subtrees of the
    /// expression.
    /// </param>
    internal CSharpGlobalBindingGenerator(
        AstGlobalBinding binding, CSharpGeneratorFactory factory) {
      Symbol = binding.Symbol;
      Module = binding.Module;
    }

    /// <summary>
    /// Produce an expression representing getting a global.
    /// </summary>
    public Expression Emit() {
      return Expression.Call(
        typeof(ModuleType), "GetGlobal", null,
        Expression.Constant(Module), Expression.Constant(Symbol));
    }

    /// <summary>
    /// Produce an expression representing setting a global.
    /// </summary>
    public Expression EmitSet(Expression value) {
      return Expression.Call(
        typeof(ModuleType), "SetGlobal", null,
        Expression.Constant(Module), Expression.Constant(Symbol), value);
    }
  }
}
