using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace ZStewart.KOSLisp.Compile.AST {
  /// <summary>
  /// Represents an expression which creates a new variable context and new variables,
  /// then evaluates a set of expressions in sequence then returns the result of the last
  /// one.
  /// </summary>
  public class AstLet : AstProgn {
    /// <summary>
    /// The bindings and initial values for this generator.
    /// </summary>
    public ImmutableList<Tuple<AstBinding, AstOp>> Bindings { get; }

    internal AstLet(
        IEnumerable<Tuple<AstBinding, AstOp>> bindings, IEnumerable<AstOp> forms)
        : base(forms) {
      Bindings = ImmutableList.CreateRange(bindings);
    }

    public override StringBuilder AppendAstStringIndented(
        StringBuilder sb, int baseIndent) {
      if (Forms.Count == 0 && Bindings.Count == 0) {
        sb.Append("[AST-Let]");
      } else {
        sb.AppendLine("[AST-Let:");
        AppendBindings(sb, baseIndent);
        AppendForms(sb, baseIndent);
        sb.Append(' ', baseIndent);
        sb.Append("]");
      }
      return sb;
    }

    /// <summary>
    /// Writes just the Bindings element to the string builder. Unlike
    /// AppendAstStringIndented, the first line is indented, and a newline is appended at
    /// the end.
    /// </summary>
    protected virtual StringBuilder AppendBindings(StringBuilder sb, int baseIndent) {
      for (int i = 0; i < Bindings.Count; i++) {
        sb.Append(' ', baseIndent + 2);
        sb.AppendFormat("Let Binding {0}:", i);
        sb.AppendLine();
        sb.Append(' ', baseIndent + 4);
        sb.Append("Bound Symbol: ");
        Bindings[i].Item1.AppendAstStringIndented(sb, baseIndent + 4);
        sb.AppendLine();
        sb.Append(' ', baseIndent + 4);
        sb.Append("To Value: ");
        Bindings[i].Item2.AppendAstStringIndented(sb, baseIndent + 4);
        sb.AppendLine();
      }
      return sb;
    }
  }
}
