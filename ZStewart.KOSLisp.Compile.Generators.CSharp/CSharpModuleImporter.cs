using System.IO;
using System.Text;

using static ZStewart.KOSLisp.Types.ExceptionType;

using ZStewart.KOSLisp.Modules;
using ZStewart.KOSLisp.Types;

namespace ZStewart.KOSLisp.Compile.Generators.CSharp {
  /// <summary>
  /// Implements a module importer which can load modules as either lisp code or C#
  /// assemblies.
  /// </summary>
  public class CSharpModuleImporter : ModuleImporter {
    /// <summary>
    /// Comparer used when checking for modules.
    /// </summary>
    private static StringComparer comparer => StringComparer.InvariantCultureIgnoreCase;

    /// <summary>
    /// dictionary of imported modules by name, used to avoid a file-system lookup when
    /// calling import, and to prevent making duplicate copies of modules.
    /// </summary>
    protected Dictionary<string, LispObject> importedModules =
      new Dictionary<string, LispObject>(StringComparer.InvariantCultureIgnoreCase);

    /// <summary>
    /// List of directory to search for modules.
    ///
    /// Note: while module names are searched case-insensitive within the library search
    /// path, the libraryPath itself is case sensitive.
    /// </summary>
    protected List<string> libraryPath;

    public CSharpModuleImporter(IEnumerable<string> libraryPath) {
      this.libraryPath = libraryPath.ToList();
    }

    public CSharpModuleImporter(params string[] libraryPath)
        : this((IEnumerable<string>)libraryPath) {}

    public virtual LispObject Import(string moduleIdentifier) {
      LispObject module;
      if (importedModules.TryGetValue(moduleIdentifier, out module)) {
        return module;
      }

      var modulePath = SplitAndValidate(moduleIdentifier);

      throw ThrowNotImplementedException("");
    }

    /// <summary>
    /// Split the module identifier into path components and ensure that it is a valid
    /// module specification (no consecutive dots, no slashes).
    /// </summary>
    protected virtual string[] SplitAndValidate(string moduleIdentifier) {
      var components = moduleIdentifier.Split(".");
      if (components.Any(s => string.IsNullOrEmpty(s) || s.Contains("/"))) {
        throw ThrowImportError("invalid module name: {0}", moduleIdentifier);
      }
      return components;
    }

    /// <summary>
    /// Search the path for the given module name with the given file extension. Return
    /// null if the module cannot be found. The extension should include a dot.
    ///
    /// On windows, this is unnecessary because windows paths are insensitive anyway.We do
    /// this not to mimic windows behavior, but because lisp symbols are case insensitive
    /// and get recorded in uppercase, so we need to check directories without case
    /// sensitivity to make the lisp work correctly. libraryPath *is* case sensitive.
    /// </summary>
    protected virtual string CaseInsensitivePathSearch(
        string[] modulePath, string extension) {
      foreach (var basePath in libraryPath) {
        for (int i = 0; i < modulePath.Length && basePath != null; i++) {
          if (i < modulePath.Length - 1) {
            basePath = CaseInsensitiveDirectoryCheck(basePath, modulePath[i]);
          } else {
            basePath = CaseInsensitiveFileCheck(basePath, modulePath[i] + extension);
          }
        }
        // Found.
        if (basePath != null) {
          return basePath;
        }
      }
      // Couldn't find on any module path.
      return null;
    }

    /// <summary>
    /// Do a case-insensitive check for the given directory name in the given base
    /// directory. basePath is case sensitive on unix-like systems, nextComponent will
    /// always be case insensitive.
    /// </summary>
    protected virtual string CaseInsensitiveDirectoryCheck(
        string basePath, string nextComponent) {
      try {
        foreach (var dirname in Dicrectory.EnumerateDirectories(basePath)) {
          if (comparer.Compare(Path.GetFileName(dirname), nextComponent) == 0) {
            return dirname;
          }
        }
        // catch and ignore certain types of path errors as null return values to mean
        // that we didn't find the path here.
      } catch (DirectoryNotFoundException) {
      } catch (IOException) {
      } catch (PathTooLongException) {
      }
      return null;
    }

    /// <summary>
    /// Do a case-insensitive check for the given file name in the given base directory.
    /// basePath is case sensitive on unix-like systems, nextComponent will always be case
    /// insensitive.
    /// </summary>
    protected virtual string CaseInsentiveFileCheck(
        string basePath, string nextComponent) {
      try {
        foreach (var filename in Directory.EnumerateFiles(basePath)) {
          if (comparer.Compare(Path.GetFileName(filename), nextComponent) == 0) {
            return filename;
          }
        }
      } catch (DirectoryNotFoundException) {
      } catch (IOException) {
      } catch (PathTooLongException) {
      }
      return null;
    }
  }
}
