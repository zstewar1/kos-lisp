using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

using ZStewart.KOSLisp.Types;

namespace ZStewart.KOSLisp.Compile.AST {
  /// <summary>
  /// Represents an expresion which loads a module from a file.
  /// </summary>
  public class AstImport : AstOpBase {
    /// <summary>
    /// The list of strings which uniquely identify the module to be loaded.
    /// </summary>
    public ImmutableList<string> ModuleIdentifier { get; }

    /// <summary>
    /// The name of the variable to bind the module to once it is imported.
    /// </summary>
    public AstBinding Name { get; }

    /// <summary>
    /// Creates a constant with the specified value.
    /// </summary>
    internal AstImport(IEnumerable<string> moduleIdentifier, AstBinding name) {
      ModuleIdentifier = ImmutableList.CreateRange(moduleIdentifier);
      Name = name;
    }

    public override StringBuilder AppendAstStringIndented(
        StringBuilder sb, int baseIndent) {
      return sb.AppendLine("[AST-Import:")
        .Append(' ', baseIndent + 2)
        .Append("Identifier: ")
        .Append(string.Join(".", ModuleIdentifier))
        .AppendLine()
        .Append("Bind Name: ")
        .AppendIndented(Name, baseIndent + 4)
        .Append(' ', baseIndent)
        .Append("]");
    }
  }
}
