using ZStewart.KOSLisp.Compile.AST;
using ZStewart.KOSLisp.Types;

namespace ZStewart.KOSLisp.Compile {
  /// <summary>
  /// An interface for a type that can convert an expression in the form of an Ast which
  /// references a macro and a LispObject which represents the macro's arguments to a new
  /// LispObject representing the expanded macro.
  /// </summary>
  public interface MacroExpander {
    /// <summary>
    /// Try to pass the given args to the given macro, to produce a new expression which
    /// replaces the macro expression in the AST.
    /// </summary>
    /// <param name="macro">
    /// An AST op which can be independently evaluated to retrieve the macro which is
    /// being expanded.
    /// </param>
    /// <param name="args">
    /// The literal exression being macroexpanded
    /// </param>
    /// <returns>
    /// The result of macro expansion if the value designated by macro is actually a
    /// macro.
    ///
    /// Return null if macro does not designate a macro, e.g. because the expression
    /// raises a Name or Attribute Error, or because the value of the expression is not an
    /// object which has a --macroexpand-- function.
    /// </returns>
    LispObject Expand(AstOp macro, LispObject args);
  }
}
