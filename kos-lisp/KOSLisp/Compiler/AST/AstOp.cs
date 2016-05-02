using System.Text;

namespace ZStewart.KOSLisp.Compiler.AST {
  /// <summary>
  /// Represents an element in the abstract syntax tree. Contains methods for pretty
  /// printing the tree. Implementations will contain the necessary information to
  /// generate code.
  /// </summary>
  public interface AstOp {
    /// <summary>
    /// Append the string representation of this AST element with any trailing lines
    /// indented by an appropriate offset from baseIndent. If the caller wants the first
    /// line indented, they must indent it themeslves. There shall be no newline appended
    /// after the appended value.
    /// </summary>
    StringBuilder AppendAstStringIndented(StringBuilder sb, int baseIndent);
  }
}
