using ZStewart.KOSLisp.Compile.AST;
using ZStewart.KOSLisp.Compile.Contexts;
using ZStewart.KOSLisp.Types;

namespace ZStewart.KOSLisp.Compile {
  /// <summary>
  /// A Compiler can convert the usual LispObject (typically cons-list) representation of
  /// a code structure to an Abstract Syntax Tree.
  /// </summary>
  public interface Compiler {
    /// <summary>
    /// Convert the given expression to an AST in the provided context.
    /// </summary>
    /// <param name="expression">
    /// The lisp object representing the expression to convert. Typically this will be a
    /// cons list for a complicated expression, though other objects are allowed (e.g. a
    /// single symbol for a variable, a numeric constant, a string).
    /// </param>
    /// <param name="context">
    /// The variable context to do the conversion in. This is used to lookup any variables
    /// used in the expression. (Though some expressions, e.g. let, may result in other
    /// sub-contexts being generated with new variable bindings, so not all bindings are
    /// looked up on the provided context directly.
    /// </param>
    AstOp ToAst(LispObject expression, Context context);
  }
}
