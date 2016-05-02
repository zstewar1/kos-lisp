using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;

using ZStewart.KOSLisp.Compiler.AST;
using ZStewart.KOSLisp.Types;

namespace ZStewart.KOSLisp.Compiler.Generators.CSharp {
  /// <summary>
  /// A generator which creates an expression that represents doing a sequence of
  /// operations.
  /// </summary>
  public class CSharpLetGenerator : CSharpPrognGenerator {
    /// <summary>
    /// The bindings and initial value expressions for this let.
    /// </summary>
    ImmutableList<Tuple<CSharpBindingGenerator, CSharpGenerator>> Bindings { get; }

    /// <summary>
    /// Creates a let which creates the given bindings and evaluates the given
    /// expressions.
    /// </summary>
    /// <param name="op">
    /// The AstLet that this generator will create code for.
    /// </param>
    /// <param name="factory">
    /// A generator factory that can be used to get generators for the AST subtrees of the
    /// let.
    /// </param>
    internal CSharpLetGenerator(AstLet op, CSharpGeneratorFactory factory)
        : base(op, factory) {
      Bindings = ImmutableList.CreateRange(
        op.Bindings.Select(
          t => Tuple.Create(
            factory.Create(t.Item1),
            factory.Create(t.Item2))));
    }

    /// <summary>
    /// Produce an expression representing this let expression.
    /// </summary>
    public override Expression Emit() {
      var expressions = new List<Expression>(Bindings.Count + 1);
      foreach (var b in Bindings) {
        expressions.Add(b.Item1.EmitSet(b.Item2.Emit()));
      }
      expressions.Add(EmitForms());

      return Expression.Block(
        typeof(LispObject),
        Bindings.Select(b => (ParameterExpression)b.Item1.Emit()),
        expressions);
    }
  }
}
