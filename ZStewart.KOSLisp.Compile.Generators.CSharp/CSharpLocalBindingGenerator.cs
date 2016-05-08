using System;
using System.Collections.Immutable;
using System.Linq.Expressions;

using ZStewart.KOSLisp.Compile.AST;
using ZStewart.KOSLisp.Types;

namespace ZStewart.KOSLisp.Compile.Generators.CSharp {
  /// <summary>
  /// A generator which creates an expression that represents a conditional expression.
  /// </summary>
  public sealed class CSharpLocalBindingGenerator : CSharpBindingGenerator {
    /// <summary>
    /// The name of the variable that this local binds.
    /// </summary>
    public SymbolType Symbol { get; }

    /// <summary>
    /// The .NET variable bound by this expression.
    /// </summary>
    private readonly ParameterExpression variable;

    /// <summary>
    /// Creates a local binding generator.
    /// </summary>
    /// <param name="op">
    /// The AstLocalBinding that this generator will create code for.
    /// </param>
    /// <param name="factory">
    /// A generator factory that can be used to get generators for the AST subtrees of the
    /// expression.
    /// </param>
    internal CSharpLocalBindingGenerator(
        AstLocalBinding binding, CSharpGeneratorFactory factory) {
      Symbol = binding.Symbol;
      variable = Expression.Variable(typeof(LispObject), Symbol.Identifier);
    }

    /// <summary>
    /// Produce an expression representing getting a global.
    /// </summary>
    public Expression Emit() {
      return variable;
    }

    /// <summary>
    /// Produce an expression representing setting a global.
    /// </summary>
    public Expression EmitSet(Expression value) {
      return Expression.Assign(variable, value);
    }
  }
}
