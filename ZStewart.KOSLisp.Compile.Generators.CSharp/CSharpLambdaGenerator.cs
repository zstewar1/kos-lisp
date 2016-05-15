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
  public class CSharpLambdaGenerator : CSharpPrognGenerator {
    /// <summary>
    /// A list of generators which generate the argument bindings for this function.
    /// </summary>
    public ImmutableList<CSharpBindingGenerator> Args { get; }

    /// <summary>
    /// Creates a lambda which evaluates the given expressions.
    /// </summary>
    /// <param name="op">
    /// The AstLambda that this generator will create code for.
    /// </param>
    /// <param name="factory">
    /// A generator factory that can be used to get generators for the AST subtrees of the
    /// lambda.
    /// </param>
    internal CSharpLambdaGenerator(AstLambda op, CSharpGeneratorFactory factory)
        : base(op, factory) {
      Args = ImmutableList.CreateRange(
        op.Args.Select(arg => factory.Create(arg)));
    }

    /// <summary>
    /// Gets a name representing this function. Can be overridden in subtypes for getting
    /// names from other sources, e.g. defun functions provide their name from the symbol
    /// they are bound to.
    /// </summary>
    protected virtual SymbolType GetFunctionName() {
      return SymbolType.Create("<lambda>");
    }

    /// <summary>
    /// Produce an expression representing executing the specified expressions.
    /// </summary>
    public override Expression Emit() {
      var pargsParameter = Expression.Parameter(typeof(List<LispObject>), "--in-pargs--");
      var kwargsParameter = Expression.Parameter(
        typeof(Dictionary<SymbolType, LispObject>), "--in-kwargs--");
      return Expression.Call(
        typeof(FunctionType), "Create", null,
        Expression.Constant(GetFunctionName()),
        Expression.Lambda(
          typeof(FunctionType.Impl),
          EmitLambdaBody(pargsParameter, kwargsParameter),
          GetFunctionName().Identifier,
          ImmutableList.Create(pargsParameter, kwargsParameter)));
    }

    /// <summary>
    /// Return an expression block which has bindings for the real, individual arguments
    /// and contains two sub-blocks, one is the block that evaluates and assigns the
    /// arguments, the other is the block that evaluates the lambda's code.
    /// </summary>
    private Expression EmitLambdaBody(
        ParameterExpression pargsParameter,
        ParameterExpression kwargsParameter) {
      return Expression.Block(
        typeof(LispObject),
        Args.Select(b => (ParameterExpression)b.Emit()),
        EmitBindArgs(pargsParameter, kwargsParameter),
        EmitForms());
    }

    /// <summary>
    /// Return an expression block which binds the arguments of the function the the args
    /// which were passed to it.
    ///
    /// This function just executes Arguments.GetPositionalArguments, and checks that the
    /// correct number of arguments were passed, then uses a function to bind those
    /// arguments.
    /// </summary>
    private Expression EmitBindArgs(
        ParameterExpression pargsParameter,
        ParameterExpression kwargsParameter) {
      var argCount = Expression.Variable(typeof(int), "Arg Count");

      // TODO(zstewar1): Check kwargs, pargs wrapping into kwargs, rest, and kwrest
      return Expression.Block(
        typeof(LispObject),
        ImmutableList.Create(argCount),
        Expression.Assign(
          argCount,
          Expression.Property(pargsParameter, "Count")),
        // Check if the list has the correct number of arguments.
        Expression.IfThen(
          Expression.NotEqual(argCount, Expression.Constant(Args.Count)),
          Expression.Throw(
            Expression.Call(
              typeof(ExceptionType), "ThrowTypeError", null,
              Expression.Constant(
                "function " + GetFunctionName().Identifier + " expected " +
                Args.Count + " arguments, got {0}"),
              Expression.NewArrayInit(
                typeof(object),
                Expression.Convert(argCount, typeof(object)))))),
        EmitInstantiateArgs(pargsParameter, kwargsParameter));
    }

    /// <summary>
    /// Return an expression which evaluates to assigning a value to each argument binding
    /// from a list of arguments. The list of arguments and the number of argument
    /// bindings are assumend to be equal.
    /// </summary>
    private Expression EmitInstantiateArgs(
        ParameterExpression pargsParameter,
        ParameterExpression kwargsParameter) {
      if (Args.Count == 0) return Expression.Constant(NilType.Nil);

      var bindExpressions = new List<Expression>(Args.Count);

      int item = 0;
      foreach (var arg in Args) {
        bindExpressions.Add(
          arg.EmitSet(
            Expression.Property(pargsParameter, "Item", Expression.Constant(item++))));
      }

      return Expression.Block(
        typeof(LispObject),
        bindExpressions);
    }
  }
}
