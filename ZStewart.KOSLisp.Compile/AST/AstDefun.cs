using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace ZStewart.KOSLisp.Compile.AST {
  /// <summary>
  /// Representes an expression that generates a lambda function.
  /// </summary>
  public class AstDefun : AstLambda {
    /// <summary>
    /// The binding that this defun will assign the resulting function to. Also serves as
    /// the name of the function.
    /// </summary>
    public AstBinding Name { get; }

    /// <summary>
    /// Creates an expression that evaluates to a lambda function.
    /// </summary>
    internal AstDefun(
        AstBinding name, IEnumerable<AstBinding> args, IEnumerable<AstOp> forms)
        : base(args, forms) {
      Name = name;
    }

    public override StringBuilder AppendAstStringIndented(
        StringBuilder sb, int baseIndent) {
      sb.AppendLine("[AST-Defun:");
      sb.Append(' ', baseIndent + 2);
      sb.Append("Name: ");
      Name.AppendAstStringIndented(sb, baseIndent + 4);
      sb.AppendLine();
      AppendArgs(sb, baseIndent);
      AppendForms(sb, baseIndent);
      sb.Append(' ', baseIndent);
      return sb.Append("]");
    }
  }
}
