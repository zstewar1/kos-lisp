using System.Collections.Generic;

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
    /// <param name="moduleIdentifier">
    /// An array of module name components to map to the components of the full, dotted
    /// module name.
    /// </param>
    LispObject Import(params string[] moduleIdentifier);

    /// <summary>
    /// Load the given module and return it. Raise an import error if the module is not
    /// found.
    /// </summary>
    /// <param name="moduleIdentifier">
    /// An enumerable of module name components to map to the components of the full,
    /// dotted module name.
    /// </param>
    LispObject Import(IEnumerable<string> moduleIdentifier);
  }
}
