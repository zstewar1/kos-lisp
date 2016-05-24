using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;

using ZStewart.KOSLisp.Compile.AST;
using ZStewart.KOSLisp.Modules;
using ZStewart.KOSLisp.Types;
using ZStewart.KOSLisp.Types.Helpers;

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
    ///
    /// Optional, may be null.
    /// </summary>
    public CSharpBindingGenerator Binding { get; }

    /// <summary>
    /// A collection of bindings to load from the specified module.
    ///
    /// Optional, may be null.
    /// </summary>
    public ImmutableDictionary<SymbolType, CSharpBindingGenerator> FromImport;
    /// <summary>
    /// A module to do an :all import into.
    ///
    /// Optional, may be null;
    /// </summary>
    public ModuleType AllTo { get; }

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
      if (op.Name != null) {
        Binding = factory.Create(op.Name);
      }
      if (op.FromImport != null) {
        FromImport = ImmutableDictionary.CreateRange(
          op.FromImport.Select(
            kvp => new KeyValuePair<SymbolType, CSharpBindingGenerator>(
              kvp.Key, factory.Create(kvp.Value))));
      }
      AllTo = op.AllTo;
      Importer = importer;
    }

    /// <summary>
    /// Produce an expression representing a module import.
    /// </summary>
    public Expression Emit() {
      var imported = Expression.Variable(typeof(ModuleType), "imported");
      var expressions = new List<Expression>(5);
      expressions.Add(
        Expression.Assign(
          imported,
          Expression.Call(
            Expression.Constant(Importer),
            Importer.GetType()
              .GetMethod("Import", new Type[]{typeof(IEnumerable<string>)}),
            Expression.Constant(ModuleIdentifier))));

      AddBind(imported, expressions);
      AddFrom(imported, expressions);
      AddAll(imported, expressions);

      expressions.Add(imported);

      return Expression.Block(
        typeof(LispObject),
        ImmutableList.Create(imported),
        expressions);
    }

    /// <summary>
    /// if Binding is non-null, appends an expression which binds the imported module to
    /// the expression list, otherwsie leave the expression list unchanged.
    /// </summary>
    private void AddBind(Expression imported, List<Expression> expressions) {
      if (Binding != null) {
        expressions.Add(Binding.EmitSet(imported));
      }
    }

    /// <summary>
    /// if FromImport is non-null, appends an expression which binds all of the symbols in
    /// FromImport to their respective bindings.
    /// </summary>
    private void AddFrom(Expression imported, List<Expression> expressions) {
      if (FromImport != null) {
        var fromBinds = new List<Expression>(FromImport.Count);
        foreach(var kvp in FromImport) {
          fromBinds.Add(
            kvp.Value.EmitSet(
              Expression.Call(
                typeof(ModuleType), "GetGlobal", null,
                imported, Expression.Constant(kvp.Key))));
        }
        expressions.Add(Expression.Block(fromBinds));
      }
    }

    /// <summary>
    /// If the module to import to is non-null, adds an expression which binds all of the
    /// symbols in the imported module to globals in the target module.
    /// </summary>
    private void AddAll(Expression imported, List<Expression> expressions) {
      if (AllTo != null) {
        var @break = Expression.Label("all-import-break");
        var enumerator = Expression.Variable(typeof(LispObject), "loop-enumerator");
        var enumcar = Expression.Variable(typeof(LispObject), "loop-enumerator-current");

        expressions.Add(
          // Check that the new module's dict is present; if not, do nothing.
          Expression.IfThen(
            Expression.NotEqual(
              Expression.Field(imported, "__dict__"),
              Expression.Constant(null, typeof(LispObject))),
            // Dict is present:
            Expression.Block(
              typeof(void),
              ImmutableList.Create(enumerator),
              // Call dict.iter on the module dictionary.
              Expression.Assign(
                enumerator,
                Expression.Call(
                  typeof(LispObject), "Call", null,
                  Expression.Field(imported, "__dict__"),
                  Expression.Constant(SymbolType.Create("iter")))),
              // Loop while enumerator !ref= Nil
              Expression.Loop(
                Expression.IfThenElse(
                  Expression.ReferenceEqual(
                    enumerator,
                    Expression.Constant(NilType.Nil)),
                  // Reference equals nil; break.
                  Expression.Break(@break),
                  // Not nil yet:
                  Expression.Block(
                    typeof(void),
                    ImmutableList.Create(enumcar),
                    // Get the car of the expression. This is the pair (key . value)
                    Expression.Assign(
                      enumcar,
                      Expression.Call(
                        typeof(ListOperations), "GetCar", null, enumerator)),
                    // On the All-To module, set globals for each key in the module's
                    // dictionary to the matched value.
                    Expression.Call(
                      typeof(ModuleType), "SetGlobal", null,
                      Expression.Constant(AllTo),
                      CheckSymbol(
                        Expression.Call(typeof(ListOperations), "GetCar", null, enumcar)),
                      Expression.Call(typeof(ListOperations), "GetCdr", null, enumcar)),
                    Expression.Assign(
                      enumerator,
                      Expression.Call(
                        typeof(ListOperations), "GetCdr", null, enumerator)))),
                @break))));
      }
    }

    /// <summary>
    /// Creates an expression that checks that the given expression is a symbol and throws
    /// if it is not. The returned expression has type SymbolType.
    /// </summary>
    private Expression CheckSymbol(Expression shouldBeSymbol) {
      var castSymbol = Expression.Variable(typeof(SymbolType), "cast symbol");
      return Expression.Condition(
        Expression.TypeIs(shouldBeSymbol, typeof(SymbolType)),
        Expression.Block(
          typeof(SymbolType),
          ImmutableList.Create(castSymbol),
          Expression.Assign(
            castSymbol,
            Expression.Convert(shouldBeSymbol, typeof(SymbolType))),
          Expression.IfThen(
            Expression.Property(castSymbol, "IsSelfEvaluating"),
            Expression.Call(
              typeof(ExceptionType), "ThrowImportError", null,
              Expression.Constant("cannot import self-evaluating symbol '{0}"),
              Expression.NewArrayInit(typeof(object), castSymbol))),
          castSymbol),
        Expression.Block(
          typeof(SymbolType),
          Expression.Call(
            typeof(ExceptionType), "ThrowImportError", null,
            Expression.Constant("cannot import non-symbols"),
            Expression.NewArrayBounds(
              typeof(object),
              Expression.Constant(0))),
          // Nil here should be unreachable, but we have it to return the correct type.
          Expression.Constant(NilType.Nil)));

    }
  }
}
