using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace ZStewart.KOSLisp.Compiler.AST {
  /// <summary>
  /// Represents a function call in the AST.
  /// </summary>
  public sealed class AstFuncCall : AstOpBase {
    /// <summary>
    /// The expression which evaluates to the function being called.
    /// </summary>
    public AstOp Function { get; }

    /// <summary>
    /// The list of expressions that evaluate to the arguments being passed to the
    /// function.
    /// </summary>
    public ImmutableList<AstOp> Arguments { get; }

    /// <summary>
    /// Creates a function call that calls the function with the provided arguments.
    /// </summary>
    internal AstFuncCall(AstOp function, IEnumerable<AstOp> arguments) {
      Function = function;
      Arguments = ImmutableList.CreateRange(arguments);
    }

    public override StringBuilder AppendAstStringIndented(
        StringBuilder sb, int baseIndent) {
      sb.AppendLine("[AST-Function-Call:");
      sb.Append(' ', baseIndent + 2);
      sb.Append("Function: ");
      Function.AppendAstStringIndented(sb, baseIndent + 2);
      sb.AppendLine();
      for (int i = 0; i < Arguments.Count; i++) {
        sb.Append(' ', baseIndent + 2);
        sb.AppendFormat("Arg {0}: ", i);
        Arguments[i].AppendAstStringIndented(sb, baseIndent + 2);
        sb.AppendLine();
      }
      sb.Append(' ', baseIndent);
      sb.Append("]");
      return sb;
    }
  }
}
