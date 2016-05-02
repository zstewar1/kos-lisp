using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace ZStewart.KOSLisp.Compiler.AST {
  /// <summary>
  /// Represents a function call in the AST.
  /// </summary>
  public sealed class AstIf : AstOpBase {
    /// <summary>
    /// An AST subtree which evaluates to the boolean condition for this if.
    /// </summary>
    public AstOp Condition { get; }
    /// <summary>
    /// An AST subtree which will be evaluated an used as the result if the condition is
    /// true.
    /// </summary>
    public AstOp ValueIfTrue { get; }
    /// <summary>
    /// An AST subtree which will be evaluated an used as the result if the condition is
    /// false.
    /// </summary>
    public AstOp ValueIfFalse { get; }

    /// <summary>
    /// Creates a conditional AST op, which provides one of two results based on a
    /// condition.
    /// </summary>
    internal AstIf(AstOp condition, AstOp valueIfTrue, AstOp valueIfFalse) {
      Condition = condition;
      ValueIfTrue = valueIfTrue;
      ValueIfFalse = valueIfFalse;
    }

    public override StringBuilder AppendAstStringIndented(
        StringBuilder sb, int baseIndent) {
      sb.AppendLine("[AST-If:");
      sb.Append(' ', baseIndent + 2);
      sb.Append("Condition: ");
      Condition.AppendAstStringIndented(sb, baseIndent + 2);
      sb.AppendLine();
      sb.Append(' ', baseIndent + 2);
      sb.Append("Value If True: ");
      ValueIfTrue.AppendAstStringIndented(sb, baseIndent + 2);
      sb.AppendLine();
      sb.Append(' ', baseIndent + 2);
      sb.Append("Value If False: ");
      ValueIfFalse.AppendAstStringIndented(sb, baseIndent + 2);
      sb.AppendLine();
      sb.Append(' ', baseIndent);
      return sb.Append("]");
    }
  }
}
