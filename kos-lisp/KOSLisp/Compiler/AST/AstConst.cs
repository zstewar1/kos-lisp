using System.Text;

using ZStewart.KOSLisp.Types;

namespace ZStewart.KOSLisp.Compiler.AST {
  /// <summary>
  /// Represents a constant-valued expression in the AST.
  /// </summary>
  public class AstConst : AstOpBase {
    /// <summary>
    /// The value stored in this constant.
    /// </summary>
    public LispObject Value { get; }

    /// <summary>
    /// Creates a constant with the specified value.
    /// </summary>
    internal AstConst(LispObject value) {
      Value = value;
    }

    public override StringBuilder AppendAstStringIndented(
        StringBuilder sb, int baseIndent) {
      return sb.AppendFormat("[AST-Constant: {0}]", Value);
    }
  }
}
