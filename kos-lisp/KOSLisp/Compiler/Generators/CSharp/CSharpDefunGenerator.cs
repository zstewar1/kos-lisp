using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;

using ZStewart.KOSLisp.Compiler.AST;
using ZStewart.KOSLisp.Types;
using ZStewart.KOSLisp.Types.Helpers;

namespace ZStewart.KOSLisp.Compiler.Generators.CSharp {
  /// <summary>
  /// A generator that represents a lambda expression.
  /// </summary>
  public class CSharpDefunGenerator : CSharpLambdaGenerator {
    /// <summary>
    /// The binding generator which is used to bind the resuling function.
    /// </summary>
    public CSharpBindingGenerator Binding { get; }

    /// <summary>
    /// The that this function will have.
    /// </summary>
    public SymbolType Name { get; }

    /// <summary>
    /// Creates a function which evaluates the given expressions.
    /// </summary>
    /// <param name="op">
    /// The AstDefun that this generator will create code for.
    /// </param>
    /// <param name="factory">
    /// A generator factory that can be used to get generators for the AST subtrees of the
    /// lambda.
    /// </param>
    internal CSharpDefunGenerator(AstDefun op, CSharpGeneratorFactory factory)
        : base(op, factory) {
      Binding = factory.Create(op.Name);
      Name = op.Name.Symbol;
    }

    /// <summary>
    /// Gets a name representing this function.
    /// </summary>
    protected override SymbolType GetFunctionName() {
      return Name;
    }

    /// <summary>
    /// Produce an expression representing executing the specified expressions.
    /// </summary>
    public override Expression Emit() {
      var intermediate = Expression.Variable(typeof(LispObject));
      return Expression.Block(
        typeof(LispObject),
        ImmutableList.Create(intermediate),
        Expression.Assign(
          intermediate, base.Emit()),
        Binding.EmitSet(intermediate),
        intermediate);
    }
  }
}
