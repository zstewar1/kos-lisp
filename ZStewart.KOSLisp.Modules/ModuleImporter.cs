using ZStewart.KOSLisp.Types;

namespace ZStewart.KOSLisp.Modules {
  /// <summary>
  /// An interface for loading new modules.
  /// </summary>
  public interface ModuleImporter {
    /// <summary>
    /// Load the given module and return it. Raise an import error if the module is not
    /// found.
    /// </summary>
    LispObject Import(SymbolType moduleIdentifier);
  }
}
