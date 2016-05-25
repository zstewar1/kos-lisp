using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

using ZStewart.KOSLisp.Types;

namespace ZStewart.KOSLisp.Compile.AST {
  /// <summary>
  /// Representes an expression that generates a lambda function.
  /// </summary>
  public class AstLambda : AstProgn {
    /// <summary>
    /// List containing argument properties, the bindings to attach the associated
    /// argument to for the arguments to this function, and an optional ast that evaluates
    /// to the default value for that argument.
    ///
    /// The binding must be null iff the argument is a rest-ignore or rest block argument.
    ///
    /// The default value must be null iff the argument is positional or keyword argument
    /// and optional is true.
    ///
    /// Required positional arguments must not follow optional positional arguments. Rest
    /// arguments of each type (positional or keyword) may not appear more than once. Rest
    /// positional can only appear after any positional arguments, rest keyword can only
    /// appear after any keyword arguments. keyword-only arguments can only appear after
    /// any positional or rest positional arguments.
    ///
    /// Assurance of these preconditions is left up to the SpecialForm that parsed this
    /// argument list.
    /// </summary>
    public ImmutableList<Tuple<ArgumentProperties, AstBinding, AstOp>> Args { get; }

    /// <summary>
    /// Creates an expression that evaluates to a lambda function.
    /// </summary>
    internal AstLambda(
        IEnumerable<Tuple<ArgumentProperties, AstBinding, AstOp>> args,
        IEnumerable<AstOp> forms)
        : base(forms) {
      Args = ImmutableList.CreateRange(args);
    }

    public override StringBuilder AppendAstStringIndented(
        StringBuilder sb, int baseIndent) {
      if (Args.Count == 0 && Forms.Count == 0) {
        sb.Append("[AST-Lambda]");
      } else {
        sb.AppendLine("[AST-Lambda:");
        AppendArgs(sb, baseIndent);
        AppendForms(sb, baseIndent);
        sb.Append(' ', baseIndent);
        sb.Append("]");
      }
      return sb;
    }

    /// <summary>
    /// Writes just the Args element to the string builder. Unlike
    /// AppendAstStringIndented, the first line is indented, and a newline is appended at
    /// the end.
    /// </summary>
    protected virtual StringBuilder AppendArgs(StringBuilder sb, int baseIndent) {
      for (int i = 0; i < Args.Count; i++) {
        var prop = Args[i].Item1;
        var bind = Args[i].Item2;
        var @default = Args[i].Item3;
        sb.Append(' ', baseIndent + 2);
        sb.AppendFormat("Argument {0}: ", i, prop.Type).AppendLine();
        if (prop.Type == ArgumentType.PositionalOrKeyword
            || prop.Type == ArgumentType.Keyword) {
          sb.Append(' ', baseIndent + 4);
          sb.AppendFormat("Name: {0}", prop.Name).AppendLine();
          sb.Append(' ', baseIndent + 4);
          sb.AppendFormat("Is Optional: {0}", prop.IsOptional).AppendLine();
        }
        if (bind != null) {
          sb.Append(' ', baseIndent + 4);
          sb.Append("Bound To: ").AppendIndented(bind, baseIndent + 4).AppendLine();
        }
        if (@default != null) {
          sb.Append(' ', baseIndent + 4);
          sb.Append("Default Value: ").AppendIndented(@default, baseIndent + 4);
          sb.AppendLine();
        }
      }
      return sb;
    }
  }
}
