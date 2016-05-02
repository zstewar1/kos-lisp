using System.Text;

using ZStewart.KOSLisp.Types;

namespace ZStewart.KOSLisp.Compiler.AST {
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
    /// Tells whether this local binding is a closure, i.e. referenced from a sub-context
    /// that may exist after the current context goes out of scope. This is tracked,
    /// though unused by both the C# generator and KerboScript generator, as both those
    /// languages have builtin closure samantics.
    ///
    /// Setting this to false after it has been set to true by a locally scoped context
    /// may cause compilation to break (for code generators which make use of this
    /// property).
    /// </summary>
    public bool HasClosure { get; set; }

    /// <summary>
    /// Create a local binding for the given symbol.
    /// </summary>
    internal AstLocalBinding(SymbolType symbol, bool hasClosure) {
      Symbol = symbol;
      HasClosure = hasClosure;
    }

    public override StringBuilder AppendAstStringIndented(
        StringBuilder sb, int baseIndent) {
      sb.AppendLine("[AST-Local-Binding:");
      sb.Append(' ', baseIndent + 2);
      sb.AppendFormat(
        "Bound Symbol: {0}", Symbol);
      sb.AppendLine();
      sb.Append(' ', baseIndent + 2);
      sb.AppendFormat("Has Closure: {0}", HasClosure);
      sb.AppendLine();
      sb.Append(' ', baseIndent);
      return sb.Append("]");
    }
  }
}
