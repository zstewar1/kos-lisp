using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;

using ZStewart.KOSLisp.Compile.AST;
using ZStewart.KOSLisp.Types;
using ZStewart.KOSLisp.Types.Helpers;

namespace ZStewart.KOSLisp.Compile.Generators.CSharp {
  /// <summary>
  /// A generator that represents a lambda expression.
  /// </summary>
  public class CSharpDefmacroGenerator : CSharpLambdaGenerator {
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
    /// The AstDefmacro that this generator will create code for.
    /// </param>
    /// <param name="factory">
    /// A generator factory that can be used to get generators for the AST subtrees of the
    /// lambda.
    /// </param>
    internal CSharpDefmacroGenerator(AstDefmacro op, CSharpGeneratorFactory factory)
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
          // Take the base function definition and further block it into a macro.
          intermediate, WrapAsMacro(base.Emit())),
        Binding.EmitSet(intermediate),
        intermediate);
    }

    protected virtual Expression WrapAsMacro(Expression functionDefinition) {
      return Expression.Call(
        typeof(MacroType), "Create", null,
        Expression.Constant(GetFunctionName()),
        functionDefinition);
    }
  }
}
