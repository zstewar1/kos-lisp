using System;
using System.Collections.Generic;

using ZStewart.KOSLisp.Compile.AST;

namespace ZStewart.KOSLisp.Compile.Generators.CSharp {
  /// <summary>
  /// A GeneratorFactory that creates code generators that emit C# delegates representing
  /// expressions.
  /// </summary>
  public class CSharpGeneratorFactory : GeneratorFactory<CSharpGenerator> {
    /// <summary>
    /// Creates a CSharpGenerator for the expression represented by the provided AST.
    /// </summary>
    public virtual CSharpGenerator Create(AstOp op) {
      return CreateFromAstOp(op);
    }

    /// <summary>
    /// Creates a CSharpBindingGenerator for the binding represented by the provided AST
    /// binding.
    /// </summary>
    public virtual CSharpBindingGenerator Create(AstBinding binding) {
      return CreateFromAstBinding(binding);
    }

    /// <summary>
    /// Protected method to create the generator for the given AstOp.
    ///
    /// This separates the creation logic from the public inteface, allowing references
    /// between CreateFromAstOp and CreateFromAstBinding to be overridden without
    /// overriding the public inteface, or allowing the public interface to be overridden
    /// without affecting how the creation itself happens.
    /// </summary>
    protected virtual CSharpGenerator CreateFromAstOp(AstOp op) {
      var type = op.GetType();
      if (type == typeof(AstConst)) {
        return new CSharpConstGenerator((AstConst)op);
      } else if (type == typeof(AstDefun)) {
        return new CSharpDefunGenerator((AstDefun)op, GetFactoryForConstructor());
      } else if (type == typeof(AstFuncCall)) {
        return new CSharpFuncCallGenerator((AstFuncCall)op, GetFactoryForConstructor());
      } else if (type == typeof(AstIf)) {
        return new CSharpIfGenerator((AstIf)op, GetFactoryForConstructor());
      } else if (type == typeof(AstLambda)) {
        return new CSharpLambdaGenerator((AstLambda)op, GetFactoryForConstructor());
      } else if (type == typeof(AstLet)) {
        return new CSharpLetGenerator((AstLet)op, GetFactoryForConstructor());
      } else if (type == typeof(AstProgn)) {
        return new CSharpPrognGenerator((AstProgn)op, GetFactoryForConstructor());
      } else if (type == typeof(AstSetVar)) {
        return new CSharpSetVarGenerator((AstSetVar)op, GetFactoryForConstructor());
      } else if (typeof(AstBinding).IsAssignableFrom(type)) {
        // All binding subtypes can be handled by create binding.
        return CreateFromAstBinding((AstBinding)op);
      }

      throw new InvalidOperationException("Unknown AstOp type: " + type);
    }

    protected virtual CSharpBindingGenerator CreateFromAstBinding(
        AstBinding binding) {
      var type = binding.GetType();

      if (type == typeof(AstGlobalBinding)) {
        return new CSharpGlobalBindingGenerator(
          (AstGlobalBinding)binding, GetFactoryForConstructor());
      } else if (type == typeof(AstLocalBinding)) {
        return new CSharpLocalBindingGenerator(
          (AstLocalBinding)binding, GetFactoryForConstructor());
      }
      // TODO(zstewar1): Binding code here.

      throw new InvalidOperationException("Unknown AstBinding type: " + type);
    }

    /// <summary>
    /// Get a factory to pass to any constructor which requires one to get subexpressions.
    ///
    /// Creates a memoizing factory so that sub-expressions which reference the same
    /// AstBindings always get the same CSharpLocalBindingGenerator.
    /// </summary>
    protected virtual CSharpGeneratorFactory GetFactoryForConstructor() {
      return new CSharpMemoizingGeneratorFactory();
    }
  }

  /// <summary>
  /// A generator factory which saves generators, keyed on the AstOp that they are
  /// for. This is used when a Generator's constructor requires a factory argument, so
  /// that multiple instances of the same subtree always come up with the same generator.
  ///
  /// This is primarily useful because of local bindings. Local bindings have to always
  /// reference the same instance of ParameterExpression, otherwise they represent
  /// different variables. Thus we memoize and always return the same local parameter.
  ///
  /// Since parameter expressions are the only thing this is really useful for, we don't
  /// need or want to memoize at the top level, when only globals can be in effect.
  /// </summary>
  internal class CSharpMemoizingGeneratorFactory : CSharpGeneratorFactory {
    private Dictionary<AstOp, CSharpGenerator> memoized =
      new Dictionary<AstOp, CSharpGenerator>();

    /// <summary>
    /// If the generator for the given AstOp is already created, return it. Otherwise
    /// create a new generator, memoize it, and return it.
    /// </summary>
    public override CSharpGenerator Create(AstOp op) {
      CSharpGenerator generator;
      if (memoized.TryGetValue(op, out generator)) return generator;

      generator = base.Create(op);
      memoized.Add(op, generator);

      return generator;
    }

    /// <summary>
    /// If the generator for the given AstBinding is already created, return it. Otherwise
    /// create a new generator, memoize it, and return it.
    /// </summary>
    public override CSharpBindingGenerator Create(AstBinding binding) {
      CSharpGenerator generator;
      if (memoized.TryGetValue(binding, out generator)) {
        // Check if a legal cast exists from the memoized value to the subtype that we
        // need it to be.
        if (generator is CSharpBindingGenerator) {
            return (CSharpBindingGenerator)generator;
        } else {
          // If we can't cast, then we've memoized a value which is inconsistent with the
          // type that we should have gotten from creating a binding generator, and we
          // should fail. This should never happen unless a derived type overrides us
          // poorly, which should never happen because this class is internal.
          throw new InvalidOperationException(string.Format(
            "Inconsistent memoization. Binding {0} was memoized as {1}, which is not a " +
            "CSharpBindingGenerator",
            binding, generator.GetType()));
        }
      }

      var bindingGenerator = base.Create(binding);
      memoized.Add(binding, bindingGenerator);

      return bindingGenerator;
    }

    /// <summary>
    /// Always just pass the current memoizing factory into any constructor which requires
    /// it to get subexpressions.
    /// </summary>
    protected override CSharpGeneratorFactory GetFactoryForConstructor() {
      return this;
    }
  }
}
