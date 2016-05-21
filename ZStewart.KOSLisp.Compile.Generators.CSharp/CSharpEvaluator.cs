using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using static ZStewart.KOSLisp.Types.ExceptionType;

using ZStewart.KOSLisp.Compile.AST;
using ZStewart.KOSLisp.Compile.Contexts;
using ZStewart.KOSLisp.Modules;
using ZStewart.KOSLisp.Parse;
using ZStewart.KOSLisp.Parse.Lisp;
using ZStewart.KOSLisp.Types;

namespace ZStewart.KOSLisp.Compile.Generators.CSharp {
  /// <summary>
  /// Implements a module importer which can load modules as either lisp code or C#
  /// assemblies.
  /// </summary>
  public class CSharpEvaluator : ModuleImporter {

    /// <summary>
    /// Callback that happens after every time the parse reads an expression with the
    /// value of the expression read.
    /// </summary>
    public event Action<LispObject> OnParse;

    /// <summary>
    /// Callback that happens after ever time the parsed expression is converted to an AST
    /// and before it is evaluated.
    /// </summary>
    public event Action<AstOp> OnSemantics;

    /// <summary>
    /// Callback that is called with the result of evaluating each expression.
    /// </summary>
    public event Action<LispObject> OnEvaluate;

    /// <summary>
    /// Comparer used when checking for modules.
    /// </summary>
    protected static StringComparer comparer => StringComparer.InvariantCultureIgnoreCase;

    /// <summary>
    /// dictionary of imported modules by name, used to avoid a file-system lookup when
    /// calling import, and to prevent making duplicate copies of modules.
    /// </summary>
    protected readonly Dictionary<string, LispObject> importedModules =
      new Dictionary<string, LispObject>(StringComparer.InvariantCultureIgnoreCase);

    /// <summary>
    /// List of directory to search for modules.
    ///
    /// Note: while module names are searched case-insensitive within the library search
    /// path, the libraryPath itself is case sensitive (if the filesystem is).
    /// </summary>
    protected readonly List<string> libraryPath;

    protected const string BUILTINS_MODNAME = "builtins";

    protected readonly Parser<LispTokType> parser;
    protected readonly Lexer<LispTokType> lexer;
    protected readonly SemanticAnalyzer semantizer;
    protected readonly GeneratorFactory<CodeGenerator<Expression>> generatorFactory;

    protected CSharpEvaluator(
        IEnumerable<string> libraryPath,
        Parser<LispTokType> parser = null,
        Lexer<LispTokType> lexer = null,
        SemanticAnalyzer semantizer = null,
        GeneratorFactory<CodeGenerator<Expression>> generatorFactory = null,
        MacroExpander macroExpander = null) {
      this.libraryPath = libraryPath.ToList();

      this.parser = parser ?? new LispParser();
      this.lexer = lexer ?? LispLexer.CreateDefaultLexer();
      this.generatorFactory = generatorFactory ?? new CSharpGeneratorFactory();
      this.semantizer = semantizer ?? BasicSemanticAnalyzer.CreateDefaultAnalyzer(
        macroExpander ?? new CSharpMacroExpander(this.generatorFactory));
    }

    public CSharpEvaluator(IEnumerable<string> libraryPath) : this(libraryPath, null) {}

    public CSharpEvaluator(params string[] libraryPath)
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
      assemblyPath = Path.GetFullPath(assemblyPath);
      var assembly = Assembly.LoadFrom("file://" + assemblyPath);

      foreach (var type in assembly.GetTypes()) {
        if (type.ContainsGenericParameters) {
          continue;
        }

        var method = type.GetMethod(
          "ImportModule",
          BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

        if (method == null) {
          continue;
        }

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

    /// <summary>
    /// Given a valid import function (returns a lisp object, has zero or one parameters,
    /// the one parameter takes a module importer) call it and return the result.
    /// </summary>
    protected virtual LispObject CallImportFunction(MethodInfo method) {
      Expression callExpression;
      if (method.GetParameters().Length == 0) {
        callExpression = Expression.Call(method);
      } else {
        callExpression = Expression.Call(method, Expression.Constant(this));
      }
      return Expression.Lambda<Func<LispObject>>(callExpression).Compile()();
    }

    /// <summary>
    /// Given a known extant lisp file name, load it and import it as a lisp module.
    /// </summary>
    protected virtual LispObject ImportFromLispFile(
        string filePath, string moduleIdentifier, string moduleName) {
      var mod = GetFreshModule(moduleIdentifier, moduleName);
      using (var file = File.OpenText(filePath)) {
        var textSource = new TextReaderSource(moduleName, file);
        try {
          Evaluate(mod, textSource);
        } catch {
          // module gets removed if it throws.
          importedModules.Remove(moduleIdentifier);
          throw;
        }
      }
      return mod;
    }

    /// <summary>
    /// Gets and sets up a new empty module.
    /// </summary>
    public virtual ModuleType GetFreshModule(
        string moduleIdentifier, string moduleName) {
      var mod = ModuleType.Create(moduleName);
      if (moduleName != BUILTINS_MODNAME) {
        LispObject.SetAttribute(mod, PropConsts.Builtins, Import(BUILTINS_MODNAME));
      }
      importedModules.Add(moduleIdentifier, mod);
      return mod;
    }

    /// <summary>
    /// Prepare for parsing by getting a parse steam and context to begin evaluating the
    /// module within.
    /// </summary>
    public virtual void StartParse(
        ModuleType module, Source source,
        out IEnumerator<LispObject> parseStream, out Context context) {
      parseStream = parser.Parse(lexer.Lex(source)).GetEnumerator();
      context = new GlobalContext(module);
    }

    /// <summary>
    /// Evaluate every expression in source in the context of the given module.
    /// </summary>
    public virtual void Evaluate(ModuleType module, Source source) {
      IEnumerator<LispObject> parseStream;
      Context context;
      StartParse(module, source, out parseStream, out context);
      while (Evaluate1(parseStream, context));
    }

    /// <summary>
    /// Evaluate a single expression from the parse stream in the given context,
    /// discarding the result.
    /// </summary>
    /// <returns>false if end of parse stream, true otherwise</returns>
    public virtual bool Evaluate1(IEnumerator<LispObject> parseStream, Context context) {
      LispObject unusedResult;
      return Evaluate1(parseStream, context, out unusedResult);
    }

    /// <summary>
    /// Evaluate a single expression from the parse stream in the given context, saving
    /// the result in result.
    /// </summary>
    /// <returns>false if end of parse stream, true otherwise</returns>
    public virtual bool Evaluate1(
        IEnumerator<LispObject> parseStream, Context context, out LispObject result) {
      if (parseStream.MoveNext()) {
        var parsed = parseStream.Current;
        OnParse?.Invoke(parsed);
        var ast = semantizer.ToAst(parsed, context);
        OnSemantics?.Invoke(ast);
        var func = Expression.Lambda<Func<LispObject>>(
          generatorFactory.Create(ast).Emit()).Compile();
        result = func();
        OnEvaluate?.Invoke(result);
        return true;
      } else {
        result = null;
        return false;
      }
    }
  }
}
