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
    public ImmutableList<
      Tuple<ArgumentProperties, CSharpBindingGenerator, CSharpGenerator>> Args { get; }

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
        op.Args.Select(
          arg => Tuple.Create(
            arg.Item1,
            arg.Item2 != null ? factory.Create(arg.Item2) : null,
            arg.Item3 != null ? factory.Create(arg.Item3) : null)));
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
      return Expression.Call(
        typeof(FunctionType), "Create", null,
        Expression.Constant(GetFunctionName()),
        EmitLambda());
    }

    /// <summary>
    /// Produce an expression that evaluates to the lambda being handled. This also binds
    /// the default values to variables which will be captured by the lambda.
    /// </summary>
    protected Expression EmitLambda() {
      var pargsParameter = Expression.Parameter(typeof(List<LispObject>), "--in-pargs--");
      var kwargsParameter = Expression.Parameter(
        typeof(Dictionary<SymbolType, LispObject>), "--in-kwargs--");

      var defaultValueBindings =
        ImmutableList.CreateRange(
          Args.Select(
            arg => arg.Item3 != null ? Expression.Variable(typeof(LispObject)) : null));

      var expressions = new List<Expression>();

      // Bind the default values for any optional arguments to the variable allocated
      // for it.
      expressions.AddRange(
        Args.Zip(defaultValueBindings, (a,b) => Tuple.Create(a,b))
          .Where(pair => pair.Item2 != null)
          .Select(pair => Expression.Assign(pair.Item2, pair.Item1.Item3.Emit())));

      expressions.Add(
        Expression.Lambda(
          typeof(FunctionType.Impl),
          EmitLambdaBody(pargsParameter, kwargsParameter, defaultValueBindings),
          GetFunctionName().Identifier,
          ImmutableList.Create(pargsParameter, kwargsParameter)));

      return Expression.Block(
        typeof(FunctionType.Impl),
        defaultValueBindings.Where(binding => binding != null),
        expressions);
    }

    /// <summary>
    /// Return an expression block which has bindings for the real, individual arguments
    /// and contains two sub-blocks, one is the block that evaluates and assigns the
    /// arguments, the other is the block that evaluates the lambda's code.
    /// </summary>
    private Expression EmitLambdaBody(
        ParameterExpression pargsParameter,
        ParameterExpression kwargsParameter,
        ImmutableList<ParameterExpression> defaultValueBindings) {

      return Expression.Block(
        typeof(LispObject),
        Args.Where(arg => arg.Item2 != null)
          .Select(arg => arg.Item2)
          .Select(binding => (ParameterExpression)binding.Emit()),
        EmitBindArgs(pargsParameter, kwargsParameter, defaultValueBindings),
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
        ParameterExpression kwargsParameter,
        ImmutableList<ParameterExpression> defaultValueBindings) {

      var argumentProps = ImmutableList.CreateRange(Args.Select(arg => arg.Item1));

      var collected = Expression.Variable(typeof(object[]), "--collected--");

      var bindExpressions = new List<Expression>();

      bindExpressions.Add(
        Expression.Assign(
          collected,
          Expression.Call(
            typeof(Arguments), "CollectArguments", null,
            pargsParameter, kwargsParameter, Expression.Constant(argumentProps))));

      for(int i = 0; i < Args.Count; i++) {
        var prop = Args[i].Item1;
        var bind = Args[i].Item2;
        var @default = defaultValueBindings[i];

        switch (prop.Type) {
          case ArgumentType.PositionalOrKeyword:
          case ArgumentType.Keyword:
            if (prop.IsOptional) {
              bindExpressions.Add(
                bind.EmitSet(
                  Expression.Condition(
                    Expression.ReferenceEqual(
                      Expression.ArrayAccess(collected, Expression.Constant(i)),
                      Expression.Constant(null, typeof(object))),
                    @default,
                    Expression.Convert(
                      Expression.ArrayAccess(collected, Expression.Constant(i)),
                      typeof(LispObject)))));
            } else {
              bindExpressions.Add(
                bind.EmitSet(
                  Expression.Convert(
                    Expression.ArrayAccess(collected, Expression.Constant(i)),
                    typeof(LispObject))));
            }
            break;
          case ArgumentType.RestIgnore:
          case ArgumentType.RestBlock:
          case ArgumentType.RestKwIgnore:
            break;
          case ArgumentType.RestCapture:
          case ArgumentType.RestKwCapture:
            bindExpressions.Add(
              bind.EmitSet(
                Expression.Call(
                  typeof(Arguments), "Unmarshal", null,
                  Expression.ArrayAccess(collected, Expression.Constant(i)))));
            break;
          default:
            throw new InvalidOperationException("This should be impossible.");
        }
      }

      return Expression.Block(
        typeof(void),
        ImmutableList.Create(collected),
        bindExpressions);
    }
  }
}
