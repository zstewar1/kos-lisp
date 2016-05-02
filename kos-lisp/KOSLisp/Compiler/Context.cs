using ZStewart.KOSLisp.Compiler.AST;
using ZStewart.KOSLisp.Types;

namespace ZStewart.KOSLisp.Compiler {
  /// <summary>
  /// A context represents the state of the available variables while converting
  /// s-expressions to AST. Contexts are used to figure out where variables are bound, and
  /// add new variables.
  ///
  /// Static scoping is handled by having some context types attempt to get variables from
  /// a parent context if the variable is not declared locally.
  ///
  /// Contexts are not ultimately passed to code generators, however, so any code so
  /// adding a new variable in a context does not suffice to define a new variable. New
  /// variables must appear in the AST in an expression that allows binding variables
  /// (e.g. a defun, let, or lambda).
  ///
  /// Some variable types are "safe" to use without first including them in a
  /// variable-definition statement, e.g. globals, meaning that they will "only" cause
  /// runtime errors if not given a value first, whereas a local appearing without first
  /// putting it in a variable-declaration AST element will cause an uncaught compiler
  /// error in the CSharpGenerator and crash the interpreter.
  /// </summary>
  public interface Context {
    /// <summary>
    /// Retrieves the AstBinding which is bound to the given symbol in the current
    /// context. In derived classes this may be a variable directly on the current context
    /// or a variable from a parent context.
    /// </summary>
    /// <returns>
    /// The AstBinding if the symbol is bound, or null if the symbol is not bound.
    /// </returns>
    AstBinding GetBinding(SymbolType symbol);

    /// <summary>
    /// Get or Add a binding for the given symbol in the current context.
    ///
    /// If the symbol is already bound in the current context, return the existing
    /// binding. Otherwise add a new binding for that symbol to this context and return
    /// it.
    ///
    /// Not that simply AddBinding a symbol is not sufficient for it to be bound correctly
    /// for some AstBinding types. See the documentation for the Context interface.
    AstBinding AddBinding(SymbolType symbol);
  }
}
