using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

using ZStewart.KOSLisp.Types;

namespace ZStewart.KOSLisp.Compile.AST {
  /// <summary>
  /// Representes an expression that generates a macro.
  /// </summary>
  public class AstDefmacro : AstLambda {
    /// <summary>
    /// The binding that this defmacro will assign the resulting macro to. Also serves as
    /// the name of the macro.
    /// </summary>
    public AstBinding Name { get; }

    /// <summary>
    /// Creates an expression that evaluates to a macro.
    ///
    /// Macro definitions cannot take positional arguments, but assurance of this is left
    /// to the special form that creates the AST.
    /// </summary>
    internal AstDefmacro(
        AstBinding name,
        IEnumerable<Tuple<ArgumentProperties, AstBinding, AstOp>> args,
        IEnumerable<AstOp> forms)
        : base(args, forms) {
      Name = name;
    }

    public override StringBuilder AppendAstStringIndented(
        StringBuilder sb, int baseIndent) {
      sb.AppendLine("[AST-Defmacro:");
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
