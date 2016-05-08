using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace ZStewart.KOSLisp.Compile.AST {
  /// <summary>
  /// Representes an expression that generates a lambda function.
  /// </summary>
  public class AstLambda : AstProgn {
    /// <summary>
    /// The list of positional argument bindings for this function.
    /// </summary>
    public ImmutableList<AstBinding> Args { get; }

    /// <summary>
    /// Creates an expression that evaluates to a lambda function.
    /// </summary>
    internal AstLambda(IEnumerable<AstBinding> args, IEnumerable<AstOp> forms)
        : base(forms) {
      Args = ImmutableList.CreateRange(args);
    }

    public override StringBuilder AppendAstStringIndented(
        StringBuilder sb, int baseIndent) {
      if (Args.Count == 0 && Forms.Count == 0) {
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
      for (int i = 0; i < Args.Count; i++) {
        sb.Append(' ', baseIndent + 2);
        sb.AppendFormat("Arg {0}: ", i);
        Args[i].AppendAstStringIndented(sb, baseIndent + 4);
        sb.AppendLine();
      }
      return sb;
    }
  }
}
