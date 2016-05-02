using System;
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
  public class CSharpPrognGenerator : CSharpGenerator {
    /// <summary>
    /// A list of generators which generate the expressions to be evaluated.
    /// </summary>
    public ImmutableList<CSharpGenerator> Forms { get; }

    /// <summary>
    /// Creates a progn which evaluates the given expressions.
    /// </summary>
    /// <param name="op">
    /// The AstProgn that this generator will create code for.
    /// </param>
    /// <param name="factory">
    /// A generator factory that can be used to get generators for the AST subtrees of the
    /// progn.
    /// </param>
    internal CSharpPrognGenerator(AstProgn op, CSharpGeneratorFactory factory) {
      Forms = ImmutableList.CreateRange(
        op.Forms.Select(form => factory.Create(form)));
    }

    /// <summary>
    /// Produce an expression representing executing the specified expressions.
    /// </summary>
    public virtual Expression Emit() {
      return EmitForms();
    }

    /// <summary>
    /// Emit an expression representing the forms of this progn. For Progn this is the
    /// entire expression, but this may be useful for subclasses which add extra content
    /// around the forms.
    /// </summary>
    protected Expression EmitForms() {
      if (Forms.Count == 0) return Expression.Constant(NilType.Nil, typeof(LispObject));

      return Expression.Block(
        typeof(LispObject),
        Forms.Select(f => f.Emit()));
    }
  }
}
