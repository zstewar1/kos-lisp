using ZStewart.KOSLisp.Compile.AST;

namespace ZStewart.KOSLisp.Compile.Generators {
  /// <summary>
  /// Represents a factory class that can convert AstOps to code generators.
  /// </summary>
  public interface GeneratorFactory<out T> where T : CodeGenerator {
    /// <summary>
    /// Converts a AstOp to a generator which emits code which does the operation
    /// described by that AstOp.
    /// </summary>
    T Create (AstOp op);
  }
}
