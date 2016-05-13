using System.Collections.Generic;

using ZStewart.KOSLisp.Compile.AST;
using ZStewart.KOSLisp.Types;

namespace ZStewart.KOSLisp.Compile.Contexts {
  /// <summary>
  /// A context which can refer to variables defined by itself and by a parent context.
  ///
  /// This context type can have unbound locals, but assumes that its parent context will
  /// be able to provide bindings for any variables which are not bound locally.
  /// </summary>
  public class ScopedContext : Context {
    /// <summary>
    /// The parent context of this scope. This is used as a fallback for retrieving
    /// variables which are unbound in the current scope.
    /// </summary>
    public Context ParentScope { get; }

    protected readonly Dictionary<SymbolType, AstBinding> bindings =
      new Dictionary<SymbolType, AstBinding>();

    public ScopedContext(Context parentScope, IEnumerable<SymbolType> newBindings) {
      ParentScope = parentScope;
      foreach (var symbol in newBindings) {
        if (symbol.IsSelfEvaluating) {
          throw ExceptionType.ThrowSyntaxError(
            "cannot bind self evaluating symbol {0}", symbol);
        }
        // Make sure to only add one copy of each symbol.
        if (!bindings.ContainsKey(symbol)) {
          bindings.Add(symbol, Ast.BindLocal(symbol));
        }
      }
    }

    public ScopedContext(Context parentScope, params SymbolType[] newBindings)
        : this(parentScope, (IEnumerable<SymbolType>)newBindings) {}

    /// <summary>
    /// Get the binding for the given symbol, either in the current context or one of its
    /// parents.
    /// </summary>
    public virtual AstBinding GetBinding(SymbolType symbol) {
      var local = GetLocalBinding(symbol);
      if (local == null) return GetParentBinding(symbol);
      return local;
    }

    /// <summary>
    /// Return the binding for the given symbol if it is bound in the current context,
    /// otherwise add a binding in the current context and return it.
    ///
    /// Does not check for bindings in parent contexts.
    /// </summary>
    public virtual AstBinding AddBinding(SymbolType symbol) {
      if (symbol.IsSelfEvaluating) {
        throw ExceptionType.ThrowSyntaxError(
          "cannot bind self evaluating symbol {0}", symbol);
      }
      // Check the existing binding.
      var oldbind = GetLocalBinding(symbol);
      if (oldbind != null) return oldbind;

      // Add a new binding.
      var newbind = Ast.BindLocal(symbol);
      bindings.Add(symbol, newbind);
      return newbind;
    }

    /// <summary>
    /// Check if the symbol is bound in the current context and return it if it is. Can be
    /// overridded in derived context types that wish to change how locals are looked up
    /// without changing the lookup of globals.
    /// </summary>
    /// <returns>
    /// The binding if the symbol is bound in the current context, or null if it is not.
    /// </returns>
    protected virtual AstBinding GetLocalBinding(SymbolType symbol) {
      AstBinding binding;
      // TryGetValue sets the out variable to default(T) if the lookup fails.Since we want
      // to return null for unbound symbols, there's no need to check the return value.
      bindings.TryGetValue(symbol, out binding);
      return binding;
    }

    /// <summary>
    /// Get the binding for the given symbol in the parent scope. This is used by
    /// GetBinding to retrieve the fallback binding when the variable is unbound locally.
    ///
    /// Can be overridden in derived contet types to add other behavior, e.g. making the
    /// binding into a closure before returning.
    /// </summary>
    protected virtual AstBinding GetParentBinding(SymbolType symbol) {
      return ParentScope.GetBinding(symbol);
    }
  }
}
