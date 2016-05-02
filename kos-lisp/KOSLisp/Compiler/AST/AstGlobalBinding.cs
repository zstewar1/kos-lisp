using System.Text;

using ZStewart.KOSLisp.Types;

namespace ZStewart.KOSLisp.Compiler.AST {
  /// <summary>
  /// Represents a binding to a module-level variable.
  /// </summary>
  public class AstGlobalBinding : AstOpBase, AstBinding {
    /// <summary>
    /// The module-level symbol to look up or set with this binding.
    /// </summary>
    public SymbolType Symbol { get; }

    /// <summary>
    /// The module that this symbol is bound in.
    /// </summary>
    public ModuleType Module { get; }

    /// <summary>
    /// Global bindings always have closure over the module that they reference.
    /// </summary>
    public bool HasClosure {
      get { return true; }
      set {}
    }

    /// <summary>
    /// Create a global binding for the given symbol and module.
    /// </summary>
    internal AstGlobalBinding(ModuleType module, SymbolType symbol) {
      Symbol = symbol;
      Module = module;
    }

    public override StringBuilder AppendAstStringIndented(
        StringBuilder sb, int baseIndent) {
      sb.AppendLine("[AST-Global-Binding:");
      sb.Append(' ', baseIndent + 2);
      sb.AppendFormat(
        "Bound Symbol: {0}", Symbol);
      sb.AppendLine();
      sb.Append(' ', baseIndent + 2);
      sb.AppendFormat("Module: {0}", Module.Name);
      sb.AppendLine();
      sb.Append(' ', baseIndent);
      return sb.Append("]");
    }
  }
}
