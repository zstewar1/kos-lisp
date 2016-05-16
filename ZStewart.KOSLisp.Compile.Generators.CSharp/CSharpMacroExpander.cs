using static ZStewart.KOSLisp.Types.ExceptionType;

using ZStewart.KOSLisp.Compile.AST;
using ZStewart.KOSLisp.Types;

namespace ZStewart.KOSLisp.Compile.Generators.CSharp {
  /// <summary>
  /// An implementation of macro expander which expands macros by generating a C# lambda
  /// for the macro expression and calling it to get a macro, then trying to use that
  /// macro to expand.
  /// </summary>
  public class CSharpMacroExpander : MacroExpander {
    /// <summary>
    /// Generator factory used to generate code for the macro-expression AST in order to
    /// evaluate it.
    /// </summary>
    private readonly GeneratorFactory<CodeGenerator<Expression>> expressionGenerator;

    public CSharpMacroExpander(
        GeneratorFactory<CodeGenerator<Expression>> expressionGenerator) {
      this.expressionGenerator = expressionGenerator;
    }

    /// <summary>
    /// Tries to expand the given macro with the given args object.
    /// returns null if macro does not designate a macro.
    /// </summary>
    public LispObject Expand(AstOp macro, LispObject args) {
      // TODO(zstewar1): Actually expand the macro.
      return null;
    }
  }
}
