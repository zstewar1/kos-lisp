using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

using ZStewart.KOSLisp.Types;

namespace ZStewart.KOSLisp.Compile.AST {
  /// <summary>
  /// Represents a function call in the AST.
  /// </summary>
  public sealed class AstFuncCall : AstOpBase {
    /// <summary>
    /// The expression which evaluates to the function being called.
    /// </summary>
    public AstOp Function { get; }

    /// <summary>
    /// The list of expressions which evaluate to the function's positional arguments.
    /// </summary>
    public ImmutableList<AstOp> PositionalArguments { get; }

    /// <summary>
    /// The list of expressions which evaluate to the function's keyword arguments.
    /// </summary>
    public ImmutableDictionary<SymbolType, AstOp> KeywordArguments { get; }

    /// <summary>
    /// Creates a function call that calls the function with the provided arguments.
    /// </summary>
    internal AstFuncCall(
        AstOp function,
        IEnumerable<AstOp> positionalArguments = null,
        IEnumerable<KeyValuePair<SymbolType, AstOp>> keywordArguments = null) {
      Function = function;
      PositionalArguments = positionalArguments == null ?
        ImmutableList.Create<AstOp>() :
        ImmutableList.CreateRange(positionalArguments);
      KeywordArguments = keywordArguments == null ?
        ImmutableDictionary.Create<SymbolType, AstOp>() :
        ImmutableDictionary.CreateRange(keywordArguments);
    }

    public override StringBuilder AppendAstStringIndented(
        StringBuilder sb, int baseIndent) {
      sb.AppendLine("[AST-Function-Call:");
      sb.Append(' ', baseIndent + 2);
      sb.Append("Function: ");
      sb.AppendIndented(Function, baseIndent + 2);
      sb.AppendLine();
      for (int i = 0; i < PositionalArguments.Count; i++) {
        sb.Append(' ', baseIndent + 2);
        sb.AppendFormat("Arg {0}: ", i);
        sb.AppendIndented(PositionalArguments[i], baseIndent + 2);
        sb.AppendLine();
      }
      foreach (var kvp in KeywordArguments) {
        sb.Append(' ', baseIndent + 2);
        sb.AppendFormat("Kwarg {0}: ", kvp.Key);
        sb.AppendIndented(kvp.Value, baseIndent + 2);
        sb.AppendLine();
      }
      sb.Append(' ', baseIndent);
      sb.Append("]");
      return sb;
    }
  }
}
