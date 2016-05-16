using System.Collections.Immutable;

using ZStewart.KOSLisp.Compile.AST;
using ZStewart.KOSLisp.Compile.Contexts;
using ZStewart.KOSLisp.Compile.SpecialForms;
using ZStewart.KOSLisp.Types;
using ZStewart.KOSLisp.Types.Helpers;

namespace ZStewart.KOSLisp.Compile {
  /// <summary>
  /// A basic semantic analyzer which uses a set of special forms to convert the input and
  /// does not really do anything else special.
  /// </summary>
  public abstract class BaseSemanticAnalyzer : SemanticAnalyzer {
    /// <summary>
    /// Dictionary of symbols to special forms. This is what the compiler looks through
    /// while parsing an expression if the first element is a symbol. If the symbol is in
    /// this dictionary, the the rest of the expression (i.e. the exprssion *after* the
    /// name of the form) is passed to the special form.
    /// </summary>
    protected readonly ImmutableDictionary<SymbolType, SpecialForm> namedSpecialForms;

    /// <summary>
    /// This is a special form which will be passed the entirety of the expression if the
    /// first item is not a symbol or it is a symbol which does not match any special
    /// form. The functionOrMacroForm is expected know what to do with the result.
    /// Typically this means checking if the first item is a macro and expanding it, or
    /// converting it to an AstFuncCall.
    /// </summary>
    protected readonly SpecialForm functionOrMacroForm;

    /// <summary>
    /// This is the special form which will be passed the entirety of the expression if
    /// the expression is not a Cons.
    /// </summary>
    protected readonly SpecialForm primitiveForm;

    /// <param name="namedSpecialForms">
    /// Dictionary of symbols to special forms. This is what the compiler looks through
    /// while parsing an expression if the first element is a symbol. If the symbol is in
    /// this dictionary, the the rest of the expression (i.e. the exprssion *after* the
    /// name of the form) is passed to the special form.
    /// </param>
    /// <param name="functionOrMacroForm">
    /// This is a special form which will be passed the entirety of the expression if the
    /// first item is not a symbol or it is a symbol which does not match any special
    /// form. The functionOrMacroForm is expected know what to do with the result.
    /// Typically this means checking if the first item is a macro and expanding it, or
    /// converting it to an AstFuncCall.
    /// </param>
    /// <param name="primitiveForm">
    /// This is the special form which will be passed the entirety of the expression if
    /// the expression is not a Cons.
    /// </param>
    protected BaseSemanticAnalyzer(
        ImmutableDictionary<SymbolType, SpecialForm> namedSpecialForms,
        SpecialForm functionOrMacroForm,
        SpecialForm primitiveForm) {
      this.namedSpecialForms = namedSpecialForms;
      this.functionOrMacroForm = functionOrMacroForm;
      this.primitiveForm = primitiveForm;
    }

    /// <summary>
    /// Convert from lisp expression representing a syntax tree to the AST.
    /// </summary>
    public virtual AstOp ToAst(LispObject form, Context context) {
      // A cons expression is either a special form or a function call now.
      if (form is ConsType) {
        // Get the car: if it's a symbol and the symbol matches a named special form, then
        // return that form. Otherwise, just treat it as a function call.
        var op = ListOperations.GetCar(form);
        if (op is SymbolType) {
          SpecialForm sf;
          if (namedSpecialForms.TryGetValue((SymbolType)op, out sf)) {
            var rest = ListOperations.GetCdr(form);
            return sf.ToAst(rest, context, this);
          }
        }
        // Make functionOrMacroForm handle anything that isn't a constant or special form.
        return functionOrMacroForm.ToAst(form, context, this);
      } else {
        // All non-cons expressions are to be treated as primitives, which may be
        // variable lookups or constants.
        return primitiveForm.ToAst(form, context, this);
      }
    }
  }
}
