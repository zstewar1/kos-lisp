using System;
using System.Linq.Expressions;

using ZStewart.KOSLisp.Compile.AST;

namespace ZStewart.KOSLisp.Compile.Generators.CSharp {
  /// <summary>
  /// A Code Generator that emits a C# function which performs the operation represented
  /// by this code.
  /// </summary>
  public interface CSharpGenerator : CodeGenerator<Expression> {}
}
