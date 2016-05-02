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
      var argsParameter = Expression.Parameter(typeof(LispObject), "_in-args");
      return Expression.Call(
        typeof(FunctionType), "Create", null,
        Expression.Constant(GetFunctionName()),
        Expression.Lambda(
          typeof(Func<LispObject, LispObject>),
          EmitLambdaBody(argsParameter),
          GetFunctionName().Identifier,
          ImmutableList.Create(argsParameter)));
    }

    /// <summary>
    /// Return an expression block which has bindings for the real, individual arguments
    /// and contains two sub-blocks, one is the block that evaluates and assigns the
    /// arguments, the other is the block that evaluates the lambda's code.
    /// </summary>
    private Expression EmitLambdaBody(ParameterExpression argsParameter) {
      return Expression.Block(
        typeof(LispObject),
        Args.Select(b => (ParameterExpression)b.Emit()),
        EmitBindArgs(argsParameter),
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
    private Expression EmitBindArgs(ParameterExpression argsParameter) {
      var argList = Expression.Variable(typeof(List<LispObject>), "Arg List");
      var argCount = Expression.Variable(typeof(int), "Arg Count");

      return Expression.Block(
        typeof(LispObject),
        ImmutableList.Create(argList, argCount),
        Expression.Assign(
          argList,
          Expression.Call(
            typeof(Arguments), "GetPositionalArguments", null, argsParameter)),
        Expression.Assign(
          argCount,
          Expression.Property(argList, "Count")),
        // Check if the list has the correct number of arguments.
        Expression.Condition(
          Expression.NotEqual(argCount, Expression.Constant(Args.Count)),
          Expression.Throw(
            Expression.Call(
              typeof(ExceptionType), "ThrowTypeError", null,
              Expression.Constant(
                "function " + GetFunctionName().Identifier + " expected " +
                Args.Count + " arguments, got {0}"),
              Expression.NewArrayInit(
                typeof(object),
                Expression.Convert(argCount, typeof(object))))),
          Expression.Empty()),
        EmitInstantiateArgs(argList));
    }

    /// <summary>
    /// Return an expression which evaluates to assigning a value to each argument binding
    /// from a list of arguments. The list of arguments and the number of argument
    /// bindings are assumend to be equal.
    /// </summary>
    private Expression EmitInstantiateArgs(ParameterExpression argList) {
      if (Args.Count == 0) return Expression.Constant(NilType.Nil);

      var bindExpressions = new List<Expression>(Args.Count);

      int item = 0;
      foreach (var arg in Args) {
        bindExpressions.Add(
          arg.EmitSet(
            Expression.Property(argList, "Item", Expression.Constant(item++))));
      }

      return Expression.Block(
        typeof(LispObject),
        bindExpressions);
    }
  }
}
