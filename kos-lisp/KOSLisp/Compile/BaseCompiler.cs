using System.Collections.Immutable;

using ZStewart.KOSLisp.Compile.AST;
using ZStewart.KOSLisp.Compile.Contexts;
using ZStewart.KOSLisp.Compile.SpecialForms;
using ZStewart.KOSLisp.Types;
using ZStewart.KOSLisp.Types.Helpers;

namespace ZStewart.KOSLisp.Compile {
  /// <summary>
  /// An ABC for compiler implementations. Provides a method to covert an expression to
  /// AST assuming it has been *sufficiently* macroexpanded. This means that the
  /// expression has been macroexpanded until either it is a constant value or the first
  /// value in the cons list expression is either a symbol designating a special form or
  /// an expression that evaluates to a function to call.
  /// </summary>
  public abstract class BaseCompiler : Compiler {
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
    /// form. The functionForm is expected know what to do with the result.
    /// </summary>
    protected readonly SpecialForm functionForm;

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
    /// <param name="functionForm">
    /// This is a special form which will be passed the entirety of the expression if the
    /// first item is not a symbol or it is a symbol which does not match any special
    /// form. The functionForm is expected know what to do with the result.
    /// </param>
    /// <param name="primitiveForm">
    /// This is the special form which will be passed the entirety of the expression if
    /// the expression is not a Cons.
    /// </param>
    protected BaseCompiler(
        ImmutableDictionary<SymbolType, SpecialForm> namedSpecialForms,
        SpecialForm functionForm,
        SpecialForm primitiveForm) {
      this.namedSpecialForms = namedSpecialForms;
      this.functionForm = functionForm;
      this.primitiveForm = primitiveForm;
    }

    /// <summary>
    /// Derived classes need to implement this method, as the base class only knows how to
    /// do special-form expansion. (This may change later).
    /// </summary>
    public abstract AstOp ToAst(LispObject expression, Context context);

    /// <summary>
    /// Convert a from to an AST. The form must be "sufficiently" macroexpanded, i.e. if
    /// it is a cons list, and the first element represents a macro that should be
    /// expanded, then that macro should have been expanded already.
    /// </summary>
    protected AstOp ConvertForm(LispObject form, Context context) {
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
        return functionForm.ToAst(form, context, this);
      } else {
        // All non-cons expressions are to be treated as primitives, which may be
        // variable lookups or constants.
        return primitiveForm.ToAst(form, context, this);
      }
    }
  }
}
