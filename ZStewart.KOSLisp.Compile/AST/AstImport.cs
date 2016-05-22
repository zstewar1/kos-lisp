using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

using ZStewart.KOSLisp.Types;

namespace ZStewart.KOSLisp.Compile.AST {
  /// <summary>
  /// Represents an expresion which loads a module from a file.
  ///
  /// This ast element can represent both normal import(-as), from-import(-as), and
  /// from-import-all. Though the lisp impolementation only supplies one of these at a
  /// time, implementations should support doing all three at once, or none (just import).
  /// </summary>
  public class AstImport : AstOpBase {
    /// <summary>
    /// The list of strings which uniquely identify the module to be loaded.
    /// </summary>
    public ImmutableList<string> ModuleIdentifier { get; }

    /// <summary>
    /// The name of the variable to bind the module to once it is imported.
    ///
    /// this is optional and may be null.
    /// </summary>
    public AstBinding Name { get; }

    /// <summary>
    /// A collection representing things to import from the module. The symbol is the
    /// identifier to retrieve from the module, the binding is the symbol to bind it to.
    ///
    /// this is optional and may be null.
    /// </summary>
    public ImmutableDictionary<SymbolType, AstBinding> FromImport { get; }

    /// <summary>
    /// This import is an import all and all of its contents should be imported into the
    /// global space of the specified module.
    ///
    /// this is optional and may be null.
    /// </summary>
    public ModuleType AllTo { get; }

    /// <summary>
    /// Creates a constant with the specified value.
    /// </summary>
    internal AstImport(
        IEnumerable<string> moduleIdentifier,
        AstBinding name = null,
        IEnumerable<KeyValuePair<SymbolType, AstBinding>> fromImport = null,
        ModuleType allTo = null) {
      ModuleIdentifier = ImmutableList.CreateRange(moduleIdentifier);
      Name = name;
      FromImport = fromImport != null ? ImmutableDictionary.CreateRange(fromImport)
        : null;
      AllTo = allTo;
    }

    public override StringBuilder AppendAstStringIndented(
        StringBuilder sb, int baseIndent) {
      return sb.AppendLine("[AST-Import:")
        .Append(' ', baseIndent + 2)
        .Append("Identifier: ")
        .Append(string.Join(".", ModuleIdentifier))
        .AppendLine()
        .Append(' ', baseIndent + 2)
        .Append("Bind Name: ")
        .AppendIndented(Name, baseIndent + 2)
        .AppendLine()
        .Append(' ', baseIndent)
        .Append("]");
    }
  }
}
