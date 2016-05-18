using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

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

    public virtual LispObject Import(params string[] moduleIdentifier) {
      if (moduleIdentifier.Length == 0) {
        throw ThrowImportError("empty module name");
      }

      var modident = string.Join(".", moduleIdentifier).ToUpperInvariant();
      var modname = moduleIdentifier[moduleIdentifier.Length - 1];

      if (moduleIdentifier.Any(
            s => string.IsNullOrEmpty(s)
              || s.Contains(".")
              || Path.GetInvalidFileNameChars().Any(c => s.Contains(c)))) {
        throw ThrowImportError("invalid module name: {0}", modident);
      }

      LispObject module;
      if (importedModules.TryGetValue(modident, out module)) {
        return module;
      }

      var dllPath = CaseInsensitivePathSearch(moduleIdentifier, ".dll");
      if (dllPath != null) {
        return ImportFromDotNetAssembly(dllPath, modident, modname);
      }

      var lispPath = CaseInsensitivePathSearch(moduleIdentifier, ".kl") ??
        CaseInsensitivePathSearch(moduleIdentifier, ".lisp");
      if (lispPath != null) {
        return ImportFromLispFile(lispPath, modident, modname);
      }

      throw ThrowImportError("module {0} not found", modident);
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
        var currentPath = basePath;
        for (int i = 0; i < modulePath.Length && currentPath != null; i++) {
          if (i < modulePath.Length - 1) {
            currentPath = CaseInsensitiveDirectoryCheck(currentPath, modulePath[i]);
          } else {
            currentPath = CaseInsensitiveFileCheck(
              currentPath, modulePath[i] + extension);
          }
        }
        // Found.
        if (currentPath != null) {
          return currentPath;
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
        foreach (var dirname in Directory.EnumerateDirectories(basePath)) {
          if (comparer.Compare(Path.GetFileName(dirname), nextComponent) == 0) {
            return dirname;
          }
        }
      } catch (IOException) {
      }
      return null;
    }

    /// <summary>
    /// Do a case-insensitive check for the given file name in the given base directory.
    /// basePath is case sensitive on unix-like systems, nextComponent will always be case
    /// insensitive.
    /// </summary>
    protected virtual string CaseInsensitiveFileCheck(
        string basePath, string nextComponent) {
      try {
        foreach (var filename in Directory.EnumerateFiles(basePath)) {
          if (comparer.Compare(Path.GetFileName(filename), nextComponent) == 0) {
            return filename;
          }
        }
      } catch (IOException) {
      }
      return null;
    }

    /// <summary>
    /// Given a known extant .net assembly file name, load it and look for a class with
    /// the appropriately annotated function.
    /// </summary>
    protected virtual LispObject ImportFromDotNetAssembly(
        string assemblyPath, string moduleIdentifier, string moduleName) {
      var assembly = Assembly.LoadFrom("file://" + assemblyPath);

      foreach (var type in assembly.GetTypes()) {
        if (type.ContainsGenericParameters) {
          continue;
        }

        var method = type.GetMethod(
          "ImportModule", BindingFlags.NonPublic | BindingFlags.Static);

        if (method.ContainsGenericParameters) {
          throw ThrowImportError(
            "importer functions for .net modules cannot be generic");
        }

        if (!typeof(LispObject).IsAssignableFrom(method.ReturnType)) {
          throw ThrowImportError(
            "importer functions for .net modules must return LispObjects, got {0}",
            method.ReturnType);
        }

        var parameters = method.GetParameters();
        if ((parameters.Length == 1
              && !parameters[0].ParameterType.IsAssignableFrom(typeof(ModuleImporter))) ||
            parameters.Length > 1) {
          throw ThrowImportError(
            "importer functions for .net modules must take either zero parameters or a " +
            "single parameter for a module importer.");
        }

        return CallImportFunction(method);
      }

      throw ThrowImportError("no import function found");
    }

    protected virtual LispObject ImportFromLispFile(
        string filePath, string moduleIdentifier, string moduleName) {

      return null;
    }
  }
}
