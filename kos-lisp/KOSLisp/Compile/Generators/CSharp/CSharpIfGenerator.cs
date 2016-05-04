using System;
using System.Collections.Immutable;
using System.Linq.Expressions;

using ZStewart.KOSLisp.Compile.AST;
using ZStewart.KOSLisp.Types;

namespace ZStewart.KOSLisp.Compile.Generators.CSharp {
  /// <summary>
  /// A generator which creates an expression that represents a conditional expression.
  /// </summary>
  public sealed class CSharpIfGenerator : CSharpGenerator {
    /// <summary>
    /// A generator which generates the expression to be used as the condition for the if.
    /// </summary>
    public CSharpGenerator Condition { get; }
    /// <summary>
    /// A generator which generates the expression to be used if the condition is true.
    /// </summary>
    public CSharpGenerator ValueIfTrue { get; }
    /// <summary>
    /// A generator which generates the expression to be used if the condition is false.
    /// </summary>
    public CSharpGenerator ValueIfFalse { get; }

    /// <summary>
    /// Creates a conditional op generator.
    /// </summary>
    /// <param name="op">
    /// The AstIf that this generator will create code for.
    /// </param>
    /// <param name="factory">
    /// A generator factory that can be used to get generators for the AST subtrees of the
    /// if expression.
    /// </param>
    internal CSharpIfGenerator(AstIf op, CSharpGeneratorFactory factory) {
      Condition = factory.Create(op.Condition);
      ValueIfTrue = factory.Create(op.ValueIfTrue);
      ValueIfFalse = factory.Create(op.ValueIfFalse);
    }

    /// <summary>
    /// Produce an expression representing a conditional result.
    /// </summary>
    public Expression Emit() {
      return Expression.Condition(
        Expression.Equal(
          Expression.Call(typeof(BoolType), "From", null, Condition.Emit()),
          Expression.Constant(BoolType.T)),
        ValueIfTrue.Emit(),
        ValueIfFalse.Emit());
    }
  }
}
