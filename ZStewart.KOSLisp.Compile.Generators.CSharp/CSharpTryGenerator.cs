using System;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;

using ZStewart.KOSLisp.Compile.AST;
using ZStewart.KOSLisp.Types;

namespace ZStewart.KOSLisp.Compile.Generators.CSharp {
  /// <summary>
  /// A generator which creates an expression that represents a try-catch block.
  /// </summary>
  public sealed class CSharpTryGenerator : CSharpGenerator {
    /// <summary>
    /// A generator which generates the expression guarded by the try.
    /// </summary>
    public CSharpGenerator Guarded { get; }

    /// <summary>
    /// A collection of catch blocks for the try-catch.
    /// </summary>
    public ImmutableList<Tuple<CSharpGenerator, CSharpBindingGenerator, CSharpGenerator>>
      Catches { get; }

    /// <summary>
    /// An optional generator for the finally block.
    /// </summary>
    public CSharpGenerator Finally { get; }

    /// <summary>
    /// Creates a try-catch op generator.
    /// </summary>
    /// <param name="op">
    /// The AstTry that this generator will create code for.
    /// </param>
    /// <param name="factory">
    /// A generator factory that can be used to get generators for the AST subtrees of the
    /// try-catch expression.
    /// </param>
    internal CSharpTryGenerator(AstTry op, CSharpGeneratorFactory factory) {
      Guarded = factory.Create(op.Guarded);

      Catches = ImmutableList.CreateRange(
        op.Catches.Select(
          @catch => Tuple.Create(
            factory.Create(@catch.Item1),
            @catch.Item2 != null ? factory.Create(@catch.Item2) : null,
            factory.Create(@catch.Item3))));

      Finally = op.Finally != null ? factory.Create(op.Finally) : null;
    }

    /// <summary>
    /// Produce an expression representing a conditional result.
    /// </summary>
    public Expression Emit() {
      var guarded = Guarded.Emit();

      var catches = Catches.Select(c => CreateCatch(c.Item1, c.Item2, c.Item3))
        .ToArray();

      var @finally = Finally != null ? Finally.Emit() : null;

      if (catches.Length > 0 && @finally != null) {
        return Expression.TryCatchFinally(guarded, @finally, catches);
      } else if (catches.Length > 0) {
        return Expression.TryCatch(guarded, catches);
      } else if (@finally != null) {
        return Expression.TryFinally(guarded, @finally);
      } else {
        throw ExceptionType.ThrowSyntaxError(
          "try must have either at least one catch or a finally");
      }
    }

    private CatchBlock CreateCatch(
        CSharpGenerator exceptionTypeGenerator,
        CSharpBindingGenerator optionalExceptionBindingGenerator,
        CSharpGenerator fallbackExpressionGenerator) {
      var body = fallbackExpressionGenerator.Emit();
      var exceptionTypeExpr = exceptionTypeGenerator.Emit();

      var exceptionType = Expression.Variable(typeof(LispObject), "exception type");

      var exception = Expression.Variable(typeof(ExceptionWrapper), "raw-exception");

      var filter = Expression.Block(
        typeof(bool),
        ImmutableList.Create(exceptionType),
        Expression.Assign(exceptionType, exceptionTypeExpr),
        Expression.IfThen(
          Expression.Not(
            Expression.Call(
              typeof(LispType), "IsSubtype", null,
              exceptionType,
              Expression.Constant(ExceptionType.Exception, typeof(LispObject)))),
          Expression.Throw(
            Expression.Call(
              typeof(ExceptionType), "ThrowTypeError", null,
              Expression.Constant(
                "exception to catch a subclass of Exception, got {0}"),
              Expression.NewArrayInit(typeof(object), exceptionType)))),
        Expression.Call(
          typeof(ExceptionType), "CheckException", null,
          Expression.Convert(exception, typeof(ExceptionType)),
          Expression.Convert(exceptionType, typeof(LispType))));

      if (optionalExceptionBindingGenerator != null) {
        body = Expression.Block(
          typeof(LispObject),
          ImmutableList.Create(
            (ParameterExpression)optionalExceptionBindingGenerator.Emit()),
          optionalExceptionBindingGenerator.EmitSet(
            Expression.Convert(exception, typeof(ExceptionType))),
          body);
      }

      return Expression.Catch(exception, body, filter);
    }
  }
}
