using System;

using static ZStewart.KOSLisp.Types.ExceptionType;

using ZStewart.KOSLisp.Compile.AST;
using ZStewart.KOSLisp.Compile.Contexts;
using ZStewart.KOSLisp.Compile.Generators.CSharp;
using ZStewart.KOSLisp.Modules;
using ZStewart.KOSLisp.Types;

namespace ZStewart.KOSLisp.Compile.Generators.Json {
  /// <summary>
  /// A compiler which uses an evaluator to convert a proram into a json syntax tree.
  /// </summary>
  public class JCompiler {
    /// <summary>
    /// CSharpEvaluator used to convert AST.
    /// </summary>
    private readonly CSharpEvaluator evaluator;
  }
}
