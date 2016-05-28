using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using static ZStewart.KOSLisp.Types.ExceptionType;

using ZStewart.KOSLisp.Compile.AST;
using ZStewart.KOSLisp.Types;
using ZStewart.KOSLisp.Types.Helpers;

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
      // Expand the AST fragment to get an expression that evaluates to the maybe-macro
      // object.
      var expression = expressionGenerator.Create(macro).Emit();
      // Compile the expression to get a function which we can call.
      var macroExpression = Expression.Lambda<Func<LispObject>>(expression).Compile();

      LispObject maybeMacro;
      try {
        // call the expression, catching name and attribute errors. and abortin macro
        // expansion for these.
        maybeMacro = macroExpression();
      } catch (ExceptionWrapper ex)
        when (CheckException(ex, NameError) || CheckException(ex, AttributeError)) {
        // if it is a name/attribute error, the result should just be null for "don't
        // expand". All other errors are propagated because they are unexpected.
        return null;
      }

      // Do a special lookup to find and call the macroexpand function, with the not-found
      // result as null to send a "don't replace" signal.
      return LookupHelpers.Lookup(
        maybeMacro,
        Arguments.Marshal<List<LispObject>>(args),
        new Dictionary<SymbolType, LispObject>(),
        type => null,
        PropConsts.MacroExpand,
        () => null);
    }
  }
}
