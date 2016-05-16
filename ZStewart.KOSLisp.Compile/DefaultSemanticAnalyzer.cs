using System.Collections.Generic;
using System.Collections.Immutable;

using ZStewart.KOSLisp.Compile.AST;
using ZStewart.KOSLisp.Compile.Contexts;
using ZStewart.KOSLisp.Compile.SpecialForms;
using ZStewart.KOSLisp.Types;

namespace ZStewart.KOSLisp.Compile {
  /// <summary>
  /// The default compiler type. This registers the various builtin special form types
  /// under default names.
  /// </summary>
  public class DefaultSemanticAnalyzer : BaseSemanticAnalyzer {
    public DefaultSemanticAnalyzer() : base(
        ImmutableDictionary.CreateRange(new Dictionary<SymbolType, SpecialForm> {
          [SymbolType.Create("quote")] = new QuoteSpecialForm(),
          [SymbolType.Create("lambda")] = new LambdaSpecialForm(),
          [SymbolType.Create("defun")] = new DefunSpecialForm(),
          [SymbolType.Create("defmacro")] =  new DefmacroSpecialForm(),
          [SymbolType.Create("let")] = new LetSpecialForm(),
          [SymbolType.Create("progn")] = new PrognSpecialForm(),
          [SymbolType.Create("if")] = new IfSpecialForm(),
          [SymbolType.Create("%setvar")] = new SetVarSpecialForm(),
        }),
        new FuncCallSpecialForm(),
        new PrimitiveSpecialForm()) {}
  }
}
