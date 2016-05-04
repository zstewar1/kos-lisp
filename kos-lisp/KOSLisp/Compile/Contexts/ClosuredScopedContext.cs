using System.Collections.Generic;

using ZStewart.KOSLisp.Compile.AST;
using ZStewart.KOSLisp.Types;

namespace ZStewart.KOSLisp.Compile.Contexts {
  /// <summary>
  /// A type of scoped context which is a closure over its parent context, e.g. a function
  /// body or lambda which can reference variables from the parent context even after they
  /// have gone out of scope.
  ///
  /// This is accomplished by ensuring that any binding retrieved from the parent contxt
  /// is marked as a closure before returning it.
  /// </summary>
  public class ClosuredScopedContext : ScopedContext {

    public ClosuredScopedContext(Context parentScope, IEnumerable<SymbolType> newBindings)
        : base(parentScope, newBindings) {}

    public ClosuredScopedContext(Context parentScope, params SymbolType[] newBindings)
        : this(parentScope, (IEnumerable<SymbolType>)newBindings) {}

    /// <summary>
    /// Get the binding for the given symbol in the parent scope, and mark it as a
    /// closure.
    /// </summary>
    protected override AstBinding GetParentBinding(SymbolType symbol) {
      var binding = base.GetParentBinding(symbol);
      binding.HasClosure = true;
      return binding;
    }
  }
}
