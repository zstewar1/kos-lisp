using System.Text;

namespace ZStewart.KOSLisp.Compiler.AST {
  /// <summary>
  /// Provides basic implementation helpers for AstOps, such as ToString based on
  /// AppendAstStringIndented.
  /// </summary>
  public abstract class AstOpBase : AstOp {
    public override string ToString() {
      return AppendAstStringIndented(new StringBuilder(), 0).ToString();
    }

    public abstract StringBuilder AppendAstStringIndented(
        StringBuilder sb, int baseIndent);
  }
}
