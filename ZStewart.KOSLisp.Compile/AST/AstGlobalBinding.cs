using System.Text;

using ZStewart.KOSLisp.Types;

namespace ZStewart.KOSLisp.Compile.AST {
  /// <summary>
  /// Represents a binding to a module-level variable.
  ///
  /// Globals do not need to be created in a variable-declaration statement before use.
  /// They reference module-level variables and will "only" cause a NameError if the
  /// symbol they reference doesn't exists *when the global is used*.
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
