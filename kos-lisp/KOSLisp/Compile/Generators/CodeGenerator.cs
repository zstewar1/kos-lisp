using ZStewart.KOSLisp.Compile.AST;

namespace ZStewart.KOSLisp.Compile.Generators {
  /// <summary>
  /// Non-generic code generator interface. Probably shouldn't be used directly. Since
  /// it's empty.
  /// </summary>
  public interface CodeGenerator {}

  /// <summary>
  /// A code generator that emits code compiled from an AST in the format T.
  /// </summary>
  public interface CodeGenerator<out T> : CodeGenerator {
    /// <summary>
    /// Output the code represented by this generator.
    /// </summary>
    T Emit();
  }
}
