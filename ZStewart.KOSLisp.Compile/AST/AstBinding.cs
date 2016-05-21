using ZStewart.KOSLisp.Types;

namespace ZStewart.KOSLisp.Compile.AST {
  /// <summary>
  /// Represents a particular instance of a symbol being bound to a value.
  /// </summary>
  public interface AstBinding : AstOp {
    /// <summary>
    /// The symbol being bound.
    /// </summary>
    SymbolType Symbol { get; }
  }
}
