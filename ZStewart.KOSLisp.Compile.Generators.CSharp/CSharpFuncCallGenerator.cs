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
  /// A generator which creates an expression that represents calling a function.
  /// </summary>
  public sealed class CSharpFuncCallGenerator : CSharpGenerator {
    /// <summary>
    /// A generator which generates the object that is the function being called.
    /// </summary>
    public CSharpGenerator Function { get; }
    /// <summary>
    /// A list of generators which generate the arguments to the function being called.
    /// </summary>
    public ImmutableList<CSharpGenerator> Arguments { get; }

    /// <summary>
    /// Creates a function call generator for the given AST op.
    /// </summary>
    /// <param name="op">
    /// The AstFuncCall that this generator will create code for.
    /// </param>
    /// <param name="factory">
    /// A generator factory that can be used to get generators for the AST subtrees of the
    /// function call.
    /// </param>
    internal CSharpFuncCallGenerator(AstFuncCall op, CSharpGeneratorFactory factory) {
      Function = factory.Create(op.Function);
      Arguments = ImmutableList.CreateRange(
        op.Arguments.Select(arg => factory.Create(arg)));
    }

    /// <summary>
    /// Produce an expression representing calling the specified function.
    /// </summary>
    public Expression Emit() {
      var listAdd = typeof(List<LispObject>).GetMethod("Add");
      var callMethod = typeof(CallableOperations).GetMethod(
        "Call", new Type[] {typeof(LispObject), typeof(List<LispObject>)});

      var expressions = new List<Expression>(2);
      expressions.Add(Function.Emit());
      if (Arguments.Count > 0) {
        expressions.Add(Expression.ListInit(
          Expression.New(typeof(List<LispObject>)),
          Arguments.Select(arg => Expression.ElementInit(listAdd, arg.Emit()))));
      } else {
        expressions.Add(Expression.New(typeof(List<LispObject>)));
      }

      return Expression.Call(callMethod, expressions);
    }
  }
}
