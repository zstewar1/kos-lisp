using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace ZStewart.KOSLisp.Compile.AST {
  /// <summary>
  /// Represents a try-catch in the AST.
  /// </summary>
  public sealed class AstTry : AstOpBase {
    /// <summary>
    /// The form which is guarded by this try-catch. The result of the try-catch is this
    /// if no exception occurs.
    /// </summary>
    public AstOp Guarded { get; }

    /// <summary>
    /// A collection of catch expressions, containing the exception to catch, the binding
    /// to bind it to while executing the catch expression, and the expression to handle
    /// the exception.
    ///
    /// The result of the try-catch is the result of the matched catch expression if an
    /// exception did occur.
    ///
    /// The binding may be null if the exception is not bound to a variable.
    /// </summary>
    public ImmutableList<Tuple<AstOp, AstBinding, AstOp>> Catches { get; }

    /// <summary>
    /// An expression which is executed after try and any catch block as relevant. Its
    /// result value is discarded.
    ///
    /// It is optional and may be null.
    /// </summary>
    public AstOp Finally { get; }

    internal AstTry(
        AstOp guarded,
        IEnumerable<Tuple<AstOp, AstBinding, AstOp>> catches,
        AstOp @finally) {
      Guarded = guarded;
      Catches = ImmutableList.CreateRange(catches);
      Finally = @finally;
    }

    public override StringBuilder AppendAstStringIndented(
        StringBuilder sb, int baseIndent) {
      sb.AppendLine("[AST-Try:");
      sb.Append(' ', baseIndent + 2);
      sb.Append("Guarded: ");
      sb.AppendIndented(Guarded, baseIndent + 2);
      sb.AppendLine();
      foreach (var ct in Catches) {
        sb.Append(' ', baseIndent + 2);
        sb.AppendLine("Catch:");
        sb.Append(' ', baseIndent + 4);
        sb.Append("Exception: ");
        sb.AppendIndented(ct.Item1, baseIndent + 4);
        sb.AppendLine();
        if (ct.Item2 != null) {
          sb.Append(' ', baseIndent + 4);
          sb.Append("Bound To: ");
          sb.AppendIndented(ct.Item3, baseIndent + 4);
          sb.AppendLine();
        }
        sb.Append(' ', baseIndent + 4);
        sb.Append("In Expression: ");
        sb.AppendIndented(ct.Item3, baseIndent + 4);
        sb.AppendLine();
      }
      if (Finally != null) {
        sb.Append(' ', baseIndent + 2);
        sb.Append("Finally: ");
        sb.AppendIndented(Finally, baseIndent + 2);
        sb.AppendLine();
      }
      sb.Append(' ', baseIndent);
      sb.Append("]");
      return sb;
    }
  }
}
