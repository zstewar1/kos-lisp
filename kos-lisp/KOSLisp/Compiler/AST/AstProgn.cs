using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace ZStewart.KOSLisp.Compiler.AST {
  /// <summary>
  /// Represents an expression which evaluates a set of expressions in sequence then
  /// returns the result of the last one.
  /// </summary>
  public class AstProgn : AstOpBase {
    /// <summary>
    /// The list of forms to evaluate in this progn.
    /// </summary>
    public ImmutableList<AstOp> Forms { get; }

    /// <summary>
    /// Creates an expression which evaluates a series of expressions in sequence then
    /// returns the result of the last one.
    /// </summary>
    internal AstProgn(IEnumerable<AstOp> forms) {
      Forms = ImmutableList.CreateRange(forms);
    }

    public override StringBuilder AppendAstStringIndented(
        StringBuilder sb, int baseIndent) {
      if (Forms.Count == 0) {
        sb.Append("[AST-Progn]");
      } else {
        sb.AppendLine("[AST-Progn:");
        AppendForms(sb, baseIndent);
        sb.Append(' ', baseIndent);
        sb.Append("]");
      }
      return sb;
    }

    /// <summary>
    /// Writes just the Forms element to the string builder. Unlike
    /// AppendAstStringIndented, the first line is indented, and a newline is appended at
    /// the end.
    /// </summary>
    protected virtual StringBuilder AppendForms(StringBuilder sb, int baseIndent) {
      for (int i = 0; i < Forms.Count; i++) {
        sb.Append(' ', baseIndent + 2);
        sb.AppendFormat("Form {0}: ", i);
        Forms[i].AppendAstStringIndented(sb, baseIndent + 2);
        sb.AppendLine();
      }
      return sb;
    }
  }
}
