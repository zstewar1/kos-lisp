using ZStewart.KOSLisp.Compile.AST;
using ZStewart.KOSLisp.Types;

namespace ZStewart.KOSLisp.Compile.Contexts {
  /// <summary>
  /// A context which causes variable references to implicitly reference attributes on a
  /// module.
  ///
  /// Variable references created by this context type do not require bindingfirst, and
  /// *should not* appear in the args of a defun or lambda, or the bindings of a let, etc.
  ///
  /// Because it is unknown at compile time what values will e available on a module when
  /// it is evaluated, global contexts considder all non-self-evaluating symbols to be
  /// bound and always return new AstGlobalBindings for every Get and Add Binding.
  /// </summary>
  public sealed class GlobalContext : Context {
    /// <summary>
    /// The module that this context binds symbols for.
    /// </summary>
    private ModuleType module;

    /// <summary>
    /// Creates a global context that binds symbols in the specified module.
    /// </summary>
    public GlobalContext(ModuleType module) {
      this.module = module;
    }

    /// <summary>
    /// Return an AstGlobalBinding that binds the given symbol in this binding's module.
    /// GlobalContext does not do any caching and always returns a new AstGlobalBinding
    /// for ever Get attempt.
    /// </summary>
    public AstBinding GetBinding(SymbolType symbol) {
      return AddBinding(symbol);
    }

    /// <summary>
    /// Return a new AstGlobablBinding that binds the given symbol in this binding's
    /// module.
    /// </summary>
    public AstBinding AddBinding(SymbolType symbol) {
      if (symbol.IsSelfEvaluating) {
        throw ExceptionType.ThrowSyntaxError(
          "cannot bind self evaluating symbol {0}", symbol);
      }
      return Ast.BindGlobal(module, symbol);
    }
  }
}
