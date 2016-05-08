using System.Linq.Expressions;

using ZStewart.KOSLisp.Compile.AST;

namespace ZStewart.KOSLisp.Compile.Generators.CSharp {
  /// <summary>
  /// A Code generator which emits expressions which reference bindings, both for set and
  /// get.
  /// </summary>
  public interface CSharpBindingGenerator : CSharpGenerator {
    /// <summary>
    /// Emit an expression which sets the value bound by this generator to the value of
    /// the given expression.
    /// </summary>
    Expression EmitSet(Expression value);
  }
}
