using System.Text;

using ZStewart.KOSLisp.Types;

namespace ZStewart.KOSLisp.Compile.AST {
  /// <summary>
  /// Represents a binding to a module-level variable.
  ///
  /// Local bindings are not valid unless they appear in a variable declaration
  /// statement before being used. This means that they must appear as arguments to lambda
  /// or defun, or as bindings in a let, and chan only be used from within the scope of
  /// the defun, lambda, or let where they were defined. Using a LocalBinding without
  /// including it in a variable declartion expression is undefined behavior.
  /// - the CSharpGenerator will cause an undefined local exception during expression
  ///   compilation.
  /// </summary>
  public class AstLocalBinding : AstOpBase, AstBinding {
    /// <summary>
    /// The local symbol of the binding represented by this operation.
    /// </summary>
    public SymbolType Symbol { get; }

    /// <summary>
    /// Create a local binding for the given symbol.
    /// </summary>
    internal AstLocalBinding(SymbolType symbol) {
      Symbol = symbol;
    }

    public override StringBuilder AppendAstStringIndented(
        StringBuilder sb, int baseIndent) {
      return sb.AppendFormat("[AST-Local-Binding: {0}]", Symbol);
    }
  }
}
