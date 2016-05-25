using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

using ZStewart.KOSLisp.Types;

namespace ZStewart.KOSLisp.Compile.AST {
  /// <summary>
  /// Representes an expression that generates a lambda function.
  /// </summary>
  public class AstLambda : AstProgn {
    /// <summary>
    /// The list of positional argument bindings for this function.
    /// </summary>
    public ImmutableList<AstBinding> PositionalArguments { get; }

    /// <summary>
    /// The list of keyword arguments and their bindings for this function.
    /// </summary>
    public ImmutableDictionary<SymbolType, AstBinding> KeywordArguments { get; }

    /// <summary>
    /// Creates an expression that evaluates to a lambda function.
    /// </summary>
    internal AstLambda(
        IEnumerable<AstBinding> positionalArguments,
        IEnumerable<KeyValuePair<SymbolType, AstBinding>> keywordArguments,
        IEnumerable<AstOp> forms)
        : base(forms) {
      PositionalArguments = ImmutableList.CreateRange(positionalArguments);
      KeywordArguments = ImmutableDictionary.CreateRange(keywordArguments);
    }

    public override StringBuilder AppendAstStringIndented(
        StringBuilder sb, int baseIndent) {
      if (PositionalArguments.Count == 0
          && KeywordArguments.Count == 0
          && Forms.Count == 0) {
        sb.Append("[AST-Lambda]");
      } else {
        sb.AppendLine("[AST-Lambda:");
        AppendArgs(sb, baseIndent);
        AppendForms(sb, baseIndent);
        sb.Append(' ', baseIndent);
        sb.Append("]");
      }
      return sb;
    }

    /// <summary>
    /// Writes just the Args element to the string builder. Unlike
    /// AppendAstStringIndented, the first line is indented, and a newline is appended at
    /// the end.
    /// </summary>
    protected virtual StringBuilder AppendArgs(StringBuilder sb, int baseIndent) {
      for (int i = 0; i < PositionalArguments.Count; i++) {
        sb.Append(' ', baseIndent + 2);
        sb.AppendFormat("Positional Argument {0}: ", i);
        sb.AppendIndented(PositionalArguments[i], baseIndent + 4);
        sb.AppendLine();
      }
      foreach (var kvp in KeywordArguments) {
        sb.Append(' ', baseIndent + 2);
        sb.AppendFormat("Keyword Argument {0}: ", kvp.Key);
        sb.AppendIndented(kvp.Value, baseIndent + 4);
        sb.AppendLine();
      }
      return sb;
    }
  }
}
