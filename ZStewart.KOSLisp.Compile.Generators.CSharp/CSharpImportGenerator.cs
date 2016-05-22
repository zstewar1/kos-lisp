using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq.Expressions;

using ZStewart.KOSLisp.Compile.AST;
using ZStewart.KOSLisp.Modules;
using ZStewart.KOSLisp.Types;

namespace ZStewart.KOSLisp.Compile.Generators.CSharp {
  /// <summary>
  /// A generator which creates an expression that represents a conditional expression.
  /// </summary>
  public sealed class CSharpImportGenerator : CSharpGenerator {
    /// <summary>
    /// A string collection identifying the module to import.
    /// </summary>
    public ImmutableList<string> ModuleIdentifier { get; }

    /// <summary>
    /// A binding generator for the symbol to bind the module to.
    /// </summary>
    public CSharpBindingGenerator Binding { get; }

    /// <summary>
    /// The module importer to be used to retrieve the module.
    /// </summary>
    private ModuleImporter Importer { get; }

    /// <summary>
    /// Creates an import op generator.
    /// </summary>
    /// <param name="op">
    /// The AstImport that this generator will create code for.
    /// </param>
    /// <param name="factory">
    /// A generator factory that can be used to get generators for the AST subtrees of the
    /// if expression.
    /// </param>
    /// <param name="importer">
    /// A ModuleImporter which can be used to import the given module.
    /// </param>
    internal CSharpImportGenerator(
        AstImport op, CSharpGeneratorFactory factory, ModuleImporter importer) {
      ModuleIdentifier = op.ModuleIdentifier;
      Binding = factory.Create(op.Name);
      Importer = importer;
    }

    /// <summary>
    /// Produce an expression representing a module import.
    /// </summary>
    public Expression Emit() {
      return Binding.EmitSet(
        Expression.Call(
          Expression.Constant(Importer),
          Importer.GetType().GetMethod("Import", new Type[]{typeof(IEnumerable<string>)}),
          Expression.Constant(ModuleIdentifier)));
    }
  }
}
