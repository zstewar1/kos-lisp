using ZStewart.KOSLisp.Types;

namespace ZStewart.KOSLisp.Compiler.AST {
  /// <summary>
  /// Represents a particular instance of a symbol being bound to a value.
  /// </summary>
  public interface AstBinding : AstOp {
    /// <summary>
    /// The symbol being bound.
    /// </summary>
    SymbolType Symbol { get; }

    /// <summary>
    /// Tells whether this binding has closure, i.e. whether it can be referenced from
    /// sub-contextes after the current context has gone out of scope. This is set to true
    /// by certain context types whenever they reference variables from a parent scope.
    ///
    /// Setting this to false after it has been set to true by a locally scoped context
    /// may cause compilation to break (for code generators which make use of this
    /// property).
    /// </summary>
    bool HasClosure { get; set; }
  }
}
