using System;
using System.Linq.Expressions;

using ZStewart.KOSLisp.Compiler.AST;
using ZStewart.KOSLisp.Types;

namespace ZStewart.KOSLisp.Compiler.Generators.CSharp {
  /// <summary>
  /// A generator for a constat expression.
  /// </summary>
  public sealed class CSharpConstGenerator : CSharpGenerator {
    /// <summary>
    /// The constant value that this generator evaluates to.
    /// </summary>
    public LispObject Value { get; }

    internal CSharpConstGenerator(AstConst expr) {
      Value = expr.Value;
    }

    /// <summary>
    /// Produce an expression representing the constant value of this expression.
    /// </summary>
    public Expression Emit() {
      return Expression.Constant(Value);
    }
  }
}
