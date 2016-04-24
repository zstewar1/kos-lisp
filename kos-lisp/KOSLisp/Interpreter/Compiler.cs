using System;
using System.Text;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using ZStewart.KOSLisp.Types;
using ZStewart.KOSLisp.Types.Helpers;

namespace ZStewart.KOSLisp.Interpreter {

  public class CompilerError : Exception {}
  public class LispException : CompilerError {
    ExceptionType Exception { get; set; }
    public LispException () {
      Exception = LispInterpreter.SaveException();
    }
  }

  public interface Binding : AstOp {
    SymbolType BoundSymbol { get; }
    bool IsClosure { get; }
    void TakeClosure();
  }

  public interface Context {
    /// <summary>
    /// Gets a binding from this context, returning null if the given symbol is not bound
    /// in this context.
    /// </summary>
    Binding GetBinding(SymbolType symbol);

    /// <summary>
    /// Add a binding after the Context has been created. This will affect lookups that
    /// occur in expression evaluated after the symbol has been added, but not before.
    /// </summary>
    void AddBinding(SymbolType symbol);
  }

  /// <summary>
  /// The base, or global context. No symbols are ever bound in this context.
  /// </summary>
  public class GlobalContext : Context {
    private SymbolType module;
    public GlobalContext(SymbolType module) {
      this.module = module;
    }
    public virtual Binding GetBinding(SymbolType symbol) {
      return new AstGlobalBinding(symbol, module);
    }
    // Global lookups always succeed, so we don't need to rebind.
    public void AddBinding(SymbolType symbol) {}

    private class AstGlobalBinding : AstOpBase, Binding {
      public SymbolType BoundSymbol { get; }
      public SymbolType Module { get; }
      public bool IsClosure { get { return true; } }
      public void TakeClosure() {}
      public AstGlobalBinding(SymbolType boundSymbol, SymbolType module) {
        BoundSymbol = boundSymbol;
        Module = module;
      }
      public override void AppendAstStringIndented(StringBuilder sb, int baseIndent) {
        sb.AppendLine("[AST-Global-Binding:");
        sb.Append(' ', baseIndent + 2);
        sb.AppendFormat(
          "Bound Symbol: {0}", BoundSymbol);
        sb.AppendLine();
        sb.Append(' ', baseIndent + 2);
        sb.AppendFormat("Module: {0}", Module);
        sb.AppendLine();
        sb.Append(' ', baseIndent);
        sb.Append("]");
      }
    }
  }

  public class ScopedContext : Context {
    private readonly Context parentScope;
    protected readonly Dictionary<SymbolType, Binding> bindings =
      new Dictionary<SymbolType, Binding>();

    public ScopedContext(Context parentScope, IEnumerable<SymbolType> newBindings) {
      this.parentScope = parentScope;

      foreach (var symbol in newBindings) {
        if (bindings.ContainsKey(symbol)) {
          // TODO(zstewar1): Better error messages
          throw new CompilerError();
        }
        bindings.Add(symbol, new AstLocalBinding(symbol));
      }
    }

    public ScopedContext(Context parentScope, params SymbolType[] newBindings)
        : this(parentScope, (IEnumerable<SymbolType>)newBindings) {}

    public virtual Binding GetBinding(SymbolType symbol) {
      var local = GetLocalBinding(symbol);
      if (local == null) return GetParentBinding(symbol);
      return local;
    }

    public virtual void AddBinding(SymbolType symbol) {
      bindings.Add(symbol, new AstLocalBinding(symbol));
    }

    protected virtual Binding GetLocalBinding(SymbolType symbol) {
      Binding binding;
      bindings.TryGetValue(symbol, out binding);
      // Return a binding or null if unbound. (default(binding) is null).
      return binding;
    }

    protected virtual Binding GetParentBinding(SymbolType symbol) {
      return parentScope.GetBinding(symbol);
    }

    private class AstLocalBinding : AstOpBase, Binding {
      public SymbolType BoundSymbol { get; }
      private bool isClosure;
      public bool IsClosure { get { return isClosure; } }
      public AstLocalBinding(SymbolType boundSymbol) : this(boundSymbol, false) {}
      public AstLocalBinding(SymbolType boundSymbol, bool isClosure) {
        BoundSymbol = boundSymbol;
        this.isClosure = isClosure;
      }
      public void TakeClosure() {
        isClosure = true;
      }

      public override void AppendAstStringIndented(StringBuilder sb, int baseIndent) {
        sb.AppendLine("[AST-Local-Binding:");
        sb.Append(' ', baseIndent + 2);
        sb.AppendFormat(
          "Bound Symbol: {0}", BoundSymbol);
        sb.AppendLine();
        sb.Append(' ', baseIndent + 2);
        sb.AppendFormat("Is Closure: {0}", IsClosure);
        sb.AppendLine();
        sb.Append(' ', baseIndent);
        sb.Append("]");
      }
    }
  }

  public class ClosuredScopedContext : ScopedContext {
    public ClosuredScopedContext(Context parentScope, IEnumerable<SymbolType> newBindings)
        : base(parentScope, newBindings) {}
    public ClosuredScopedContext(Context parentScope, params SymbolType[] newBindings)
        : this(parentScope, (IEnumerable<SymbolType>)newBindings) {}

    protected override Binding GetParentBinding(SymbolType symbol) {
      var b = base.GetParentBinding(symbol);
      if (b != null)
        b.TakeClosure();
      return b;
    }
  }

  public interface SpecialForm {
    // TODO(zstewar1): an operation that handles a special form.
    AstOp ExpressionToIntermediate(LispObject expression, Context context);
  }

  public class DelegateSpecialForm : SpecialForm {
    private Func<LispObject, Context, AstOp> intermediateConverter;
    public DelegateSpecialForm(Func<LispObject, Context, AstOp> intermediateConverter) {
      this.intermediateConverter = intermediateConverter;
    }

    public AstOp ExpressionToIntermediate(LispObject expression, Context context) {
      return intermediateConverter(expression, context);
    }
  }

  public class LambdaSpecialForm : SpecialForm {
    public virtual AstOp ExpressionToIntermediate(
        LispObject expression, Context context) {
      List<SymbolType> args;
      List<AstOp> forms;
      ParseArgsAndForms(expression, context, out args, out forms);
      return new AstLambda(args, forms);
    }

    protected void ParseArgsAndForms(
        LispObject expression, Context context,
        out List<SymbolType> args, out List<AstOp> forms) {
      var len = ListOperations.Count(expression);
      if (!len.HasValue) throw new LispException();
      if (len.Value < 1) throw new CompilerError();
      var arglist = ListOperations.GetCar(expression);
      if (arglist == null) throw new LispException();
      var rest = ListOperations.GetCdr(expression);
      if (rest == null) throw new LispException();

      args = ParseArgumentList(arglist);
      forms = ParseForms(rest, new ClosuredScopedContext(context, args));
    }

    protected List<SymbolType> ParseArgumentList(LispObject arglist) {
      // TODO(zstewar1): We'll require a more advanced notation (for both definition and
      // calling) once we start supporting keyword arguments.
      var ok = ListOperations.Proper(arglist);
      if (!ok.HasValue) throw new LispException();
      if (!ok.Value) throw new CompilerError();
      return ListOperations.IterList<SymbolType>(arglist)
        .Select(arg => {
          if (arg == null) throw new LispException();
          return arg;
        }).ToList();
    }

    protected List<AstOp> ParseForms(LispObject forms, Context context) {
      var ok = ListOperations.Proper(forms);
      if (!ok.HasValue) throw new LispException();
      if (!ok.Value) throw new CompilerError();
      return ListOperations.IterList(forms)
        .Select(form => {
          if (form == null) throw new LispException();
          return Compiler.ExpressionToIntermediate(form, context);
        }).ToList();
    }
  }

  public class DefunSpecialForm : LambdaSpecialForm {
    public override AstOp ExpressionToIntermediate(
        LispObject expression, Context context) {
      var len = ListOperations.Count(expression);
      if (!len.HasValue) throw new LispException();
      if (len.Value < 2) throw new CompilerError();

      var nameObj = ListOperations.GetCar(expression);
      if (nameObj == null) throw new LispException();
      if (!(nameObj is SymbolType)) throw new CompilerError();
      var name = (SymbolType)nameObj;

      var rest = ListOperations.GetCdr(expression);
      if (rest == null) throw new LispException();

      context.AddBinding(name);

      List<SymbolType> args;
      List<AstOp> forms;
      ParseArgsAndForms(rest, context, out args, out forms);
      return new AstDefun(name, args, forms);
    }
  }

  public interface AstOp {
    /// <summary>
    /// Append the string representation of this AST element with any trailing lines
    /// indented by an appropriate offset from baseIndent. If the caller wants the first
    /// line indented, they must indent it themeslves. There shall be no newline appended
    /// after the appended value.
    /// </summary>
    void AppendAstStringIndented(StringBuilder sb, int baseIndent);
  }

  public abstract class AstOpBase : AstOp {
    public override string ToString() {
      var sb = new StringBuilder();
      AppendAstStringIndented(sb, 0);
      return sb.ToString();
    }

    public abstract void AppendAstStringIndented(StringBuilder sb, int baseIndent);
  }

  public class AstConst : AstOpBase {
    public LispObject Value { get; }
    public AstConst(LispObject value) {
      Value = value;
    }

    public override void AppendAstStringIndented(StringBuilder sb, int baseIndent) {
      sb.AppendFormat("[AST-Constant: {0}]", Value);
    }
  }

  public class AstFuncCall : AstOpBase {
    public AstOp Function { get; }
    public IList<AstOp> Arguments { get; }
    public AstFuncCall(AstOp function, IList<AstOp> arguments) {
      Function = function;
      Arguments = arguments;
    }

    public override void AppendAstStringIndented(StringBuilder sb, int baseIndent) {
      sb.AppendLine("[AST-Function-Call:");
      sb.Append(' ', baseIndent + 2);
      sb.Append("Function: ");
      Function.AppendAstStringIndented(sb, baseIndent + 2);
      sb.AppendLine();
      for (int i = 0; i < Arguments.Count; i++) {
        sb.Append(' ', baseIndent + 2);
        sb.AppendFormat("Arg {0}: ", i);
        Arguments[i].AppendAstStringIndented(sb, baseIndent + 2);
        sb.AppendLine();
      }
      sb.Append(' ', baseIndent);
      sb.Append("]");
    }
  }

  public class AstLambda : AstOpBase {
    public IList<SymbolType> Args { get; }
    public IList<AstOp> Forms { get; }
    public AstLambda(IList<SymbolType> args, IList<AstOp> forms) {
      Args = args;
      Forms = forms;
    }

    public override void AppendAstStringIndented(StringBuilder sb, int baseIndent) {
      if (Args.Count == 0 && Forms.Count == 0) {
        sb.Append("[AST-Lambda");
      } else {
        sb.AppendLine("[AST-Lambda:");
      }
      AppendArgs(sb, baseIndent);
      AppendForms(sb, baseIndent);
      sb.Append(' ', baseIndent);
      sb.Append("]");
    }

    protected virtual void AppendArgs(StringBuilder sb, int baseIndent) {
      for (int i = 0; i < Args.Count; i++) {
        sb.Append(' ', baseIndent + 2);
        sb.AppendFormat("Arg {0}: {1}", i, Args[i]);
        sb.AppendLine();
      }
    }

    protected virtual void AppendForms(StringBuilder sb, int baseIndent) {
      for (int i = 0; i < Forms.Count; i++) {
        sb.Append(' ', baseIndent + 2);
        sb.AppendFormat("Form {0}: ", i);
        Forms[i].AppendAstStringIndented(sb, baseIndent + 2);
        sb.AppendLine();
      }
    }
  }

  public class AstDefun : AstLambda {
    public SymbolType Name { get; }
    public AstDefun(SymbolType name, IList<SymbolType> args, IList<AstOp> forms)
        : base(args, forms) {
      Name = name;
    }

    public override void AppendAstStringIndented(StringBuilder sb, int baseIndent) {
      sb.AppendLine("[AST-Defun:");
      sb.Append(' ', baseIndent + 2);
      sb.AppendFormat("Name: {0}", Name);
      sb.AppendLine();
      AppendArgs(sb, baseIndent);
      AppendForms(sb, baseIndent);
      sb.Append(' ', baseIndent);
      sb.Append("]");
    }
  }

  public static class Compiler {

    private static readonly ImmutableDictionary<SymbolType, SpecialForm> specialForms;

    private static SpecialForm functionForm = new DelegateSpecialForm((exp, ctx) => {
      var proper = ListOperations.Proper(exp);
      if (!proper.HasValue) throw new LispException();
      // TODO(zstewar1): Better error message.
      if (!proper.Value) throw new CompilerError();
      var fnExp = ListOperations.GetCar(exp);
      if (fnExp == null) throw new LispException();
      var fn = ExpressionToIntermediate(fnExp, ctx);
      var args = ListOperations.IterList(ListOperations.GetCdr(exp))
        .Select(arg => {
          if(arg == null) throw new LispException();
          return ExpressionToIntermediate(arg, ctx);
        }).ToList();
      return new AstFuncCall(fn, args);
    });

    private static SpecialForm plainPrimitiveForm =
      new DelegateSpecialForm((exp, ctx) => {
        if (!(exp is SymbolType) || SymbolType.IsSelfEvaluating((SymbolType)exp)) {
          return new AstConst(exp);
        }
        return ctx.GetBinding((SymbolType)exp);
      });

    static Compiler () {
      var db =
        ImmutableDictionary.CreateBuilder<SymbolType, SpecialForm>();
      db.Add(SymbolType.Create("quote"), new DelegateSpecialForm((exp, ctx) => {
        var len = ListOperations.Count(exp);
        if (!len.HasValue) throw new LispException();
        // TODO(zstewar1): More specific error?
        if (len.Value != 1) throw new CompilerError();
        var val = ListOperations.GetCar(exp);
        if (val == null) throw new LispException();
        return new AstConst(val);
      }));
      db.Add(SymbolType.Create("lambda"), new LambdaSpecialForm());
      db.Add(SymbolType.Create("defun"), new DefunSpecialForm());
      specialForms = db.ToImmutable();
    }

    public static AstOp ExpressionToIntermediate(LispObject expression, Context context) {
      // TODO(zstewar1): macroexpand the expression first.
      if (expression is ConsType) {
        var op = ListOperations.GetCar(expression);
        if (op == null) throw new LispException();
        // TODO(zstewar1): More specific error? (can't look up non-symbol)
        if (!(op is SymbolType)) throw new CompilerError();
        SpecialForm sf;
        if (specialForms.TryGetValue((SymbolType)op, out sf)) {
          var args = ListOperations.GetCdr(expression);
          if (args == null) throw new LispException();
          return sf.ExpressionToIntermediate(args, context);
        }
        return functionForm.ExpressionToIntermediate(expression, context);
      } else {
        return plainPrimitiveForm.ExpressionToIntermediate(expression, context);
      }
    }
  }
}
