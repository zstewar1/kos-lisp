using ZStewart.KOSLisp.Modules;
using ZStewart.KOSLisp.Types;

namespace ZStewart.KOSLisp.Compile.Generators.CSharp {
  /// <summary>
  /// Implements a module importer which can load modules as either lisp code or C#
  /// assemblies.
  /// </summary>
  public class CSharpModuleImporter : ModuleImporter {
    /// <summary>
    /// dictionary of imported modules by name, used to avoid a file-system lookup when
    /// calling import, and to prevent making duplicate copies of modules.
    /// </summary>
    protected Dictionary<string, LispObject> importedModules =
      new Dictionary<string, LispObject>();

    /// <summary>
    /// List of directory to search for modules.
    /// </summary>
    protected List<string> libraryPath;

    public CSharpModuleImporter(IEnumerable<string> libraryPath) {
      this.libraryPath = libraryPath.ToList();
    }

    public CSharpModuleImporter(params string[] libraryPath)
        : this((IEnumerable<string>)libraryPath) {}
  }
}
