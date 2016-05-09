using System;
using System.Collections.Immutable;
using System.Linq.Expressions;

using ZStewart.KOSLisp.Compile.AST;
using ZStewart.KOSLisp.Types;

namespace ZStewart.KOSLisp.Compile.Generators.CSharp {
  /// <summary>
  /// A generator which creates an expression that represents assigning a variable.
  /// </summary>
  public sealed class CSharpSetVarGenerator : CSharpGenerator {
    /// <summary>
    /// A generator which generates the variable to bind the if to.
    /// </summary>
    public CSharpBindingGenerator Variable { get; }
    /// <summary>
    /// A generator which generates the expression to set the variable to.
    /// </summary>
    public CSharpGenerator Value { get; }

    /// <summary>
    /// Creates a set var op generator.
    /// </summary>
    /// <param name="op">
    /// The AstSetVar that this generator will create code for.
    /// </param>
    /// <param name="factory">
    /// A generator factory that can be used to get generators for the AST subtrees of the
    /// if expression.
    /// </param>
    internal CSharpSetVarGenerator(AstSetVar op, CSharpGeneratorFactory factory) {
      Variable = factory.Create(op.Variable);
      Value = factory.Create(op.Value);
    }

    /// <summary>
    /// Produce an expression representing the assignment.
    /// </summary>
    public Expression Emit() {
      var temp = Expression.Variable(typeof(LispObject));
      return Expression.Block(
        typeof(LispObject),
        ImmutableList.Create(temp),
        Expression.Assign(temp, Value.Emit()),
        Variable.EmitSet(temp),
        temp);
    }
  }
}
