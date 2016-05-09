using System.Text;

namespace ZStewart.KOSLisp.Compile.AST {
  /// <summary>
  /// An expression which represents setting a variable (binding) to a value
  /// </summary>
  public class AstSetVar : AstOpBase {
    /// <summary>
    /// The variable to change.
    /// </summary>
    public AstBinding Variable { get; }

    /// <summary>
    /// The value to set the variable to.
    /// </summary>
    public AstOp Value { get; }

    internal AstSetVar(AstBinding variable, AstOp value) {
      Variable = variable;
      Value = value;
    }

    public override StringBuilder AppendAstStringIndented(
        StringBuilder sb, int baseIndent) {
      sb.AppendLine("[AST-Set-Var");
      sb.Append(' ', baseIndent + 2);
      sb.Append("Binding to Set: ");
      Variable.AppendAstStringIndented(sb, baseIndent + 2);
      sb.AppendLine();
      sb.Append(' ', baseIndent + 2);
      sb.Append("Value to Set to: ");
      Value.AppendAstStringIndented(sb, baseIndent + 2);
      sb.AppendLine();
      return sb.Append("]");
    }
  }
}
