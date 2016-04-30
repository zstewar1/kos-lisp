using System;
using System.Text;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;

using ZStewart.KOSLisp.Types;
using ZStewart.KOSLisp.Types.Helpers;

namespace ZStewart.KOSLisp.Interpreter {

  public class CompilerError : Exception {}
  public class LispException : CompilerError {
    ExceptionType Exception { get; set; }
    public LispException () {
      Exception = LispInterpreter.SaveException();
    }

    public override string ToString() {
      return new StringBuilder("Compiler Error due to lisp exception.")
        .AppendLine()
        .Append(Exception)
        .AppendLine()
        .Append(base.ToString())
        .ToString();
    }
  }

  public interface Binding : AstOp {
    SymbolType BoundSymbol { get; }
    bool IsClosure { get; }
    void TakeClosure();
    Expression SetValueCSharp(Expression value);
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
    Binding AddBinding(SymbolType symbol);
  }

  /// <summary>
  /// The base, or global context. No symbols are ever bound in this context.
  /// </summary>
  public class GlobalContext : Context {
    private ModuleType module;
    public GlobalContext(SymbolType moduleName) {
      module = ModuleType.Create(moduleName);
    }
    public virtual Binding GetBinding(SymbolType symbol) {
      return new AstGlobalBinding(symbol, module);
    }
    // Global lookups always succeed, so we don't need to rebind.
    public virtual Binding AddBinding(SymbolType symbol) {
      return GetBinding(symbol);
    }

    private class AstGlobalBinding : AstOpBase, Binding {
      public SymbolType BoundSymbol { get; }
      public ModuleType Module { get; }
      public bool IsClosure { get { return true; } }
      public void TakeClosure() {}
      public AstGlobalBinding(SymbolType boundSymbol, ModuleType module) {
        if (SymbolType.IsSelfEvaluating(boundSymbol)) throw new CompilerError();
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
        sb.AppendFormat("Module: {0}", Module.Name);
        sb.AppendLine();
        sb.Append(' ', baseIndent);
        sb.Append("]");
      }

      private static LispObject GetGlobal(ModuleType module, SymbolType symbol) {
        var val = LispObject.GetAttribute(module, symbol);
        if (val != null) return val;

        if (!LispInterpreter.CheckException(ExceptionType.AttributeError)) return null;
        LispInterpreter.ClearException();

        var builtins = LispObject.GetAttribute(module, PropConsts.Builtins);
        if (builtins == null) {
          if (LispInterpreter.CheckException(ExceptionType.AttributeError)) {
            LispInterpreter.ClearException();
            LispInterpreter.SetException(ExceptionType.CreateNameError(
              "name \"{0}\" is not defined", symbol));
          }
          return null;
        }

        if (builtins is ModuleType) {
          val = LispObject.GetAttribute(builtins, symbol);
          if (val != null) return val;
          if (LispInterpreter.CheckException(ExceptionType.AttributeError)) {
            LispInterpreter.ClearException();
            LispInterpreter.SetException(ExceptionType.CreateNameError(
              "name \"{0}\" is not defined", symbol));
          }
          return null;
        } else {
          val = MappingOperations.GetItem(builtins, symbol);
          if (val != null) return val;
          if (LispInterpreter.CheckException(ExceptionType.KeyError)) {
            LispInterpreter.ClearException();
            LispInterpreter.SetException(ExceptionType.CreateNameError(
              "name \"{0}\" is not defined", symbol));
          }
          return null;
        }
      }

      public override Expression CompileCSharp() {
        return Expression.Call(
          typeof(AstGlobalBinding), "GetGlobal", null,
          Expression.Constant(Module), Expression.Constant(BoundSymbol));
      }

      private static LispObject SetGlobal(
          ModuleType module, SymbolType symbol, LispObject value) {
        if (LispObject.SetAttribute(module, symbol, value) != null) return NilType.Nil;
        if (LispInterpreter.CheckException(ExceptionType.AttributeError)) {
          LispInterpreter.ClearException();
          LispInterpreter.SetException(ExceptionType.CreateNameError(
            "name \"{0}\" is not defined", symbol));
        }
        return null;
      }

      public Expression SetValueCSharp(Expression value) {
        return Expression.Call(
          typeof(AstGlobalBinding), "SetGlobal", null,
          Expression.Constant(Module), Expression.Constant(BoundSymbol), value);
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

    public virtual Binding AddBinding(SymbolType symbol) {
      Binding newbind = GetLocalBinding(symbol);
      if (newbind != null) return newbind;
      newbind = new AstLocalBinding(symbol);
      bindings.Add(symbol, newbind);
      return newbind;
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
      // TODO(zstewar1): With code generation, isClosure may not matter.
      private bool isClosure;
      public bool IsClosure { get { return isClosure; } }

      private ParameterExpression variableBinding;

      public AstLocalBinding(SymbolType boundSymbol) : this(boundSymbol, false) {}
      public AstLocalBinding(SymbolType boundSymbol, bool isClosure) {
        if (SymbolType.IsSelfEvaluating(boundSymbol)) throw new CompilerError();
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

      public override Expression CompileCSharp() {
        if (variableBinding == null)
          variableBinding = Expression.Variable(
            typeof(LispObject), BoundSymbol.Identifier);
        return variableBinding;
      }

      public Expression SetValueCSharp(Expression value) {
        if (variableBinding == null)
          variableBinding = Expression.Variable(
            typeof(LispObject), BoundSymbol.Identifier);
        return Expression.Assign(variableBinding, value);
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

  public class IfSpecialForm : SpecialForm {
    public virtual AstOp ExpressionToIntermediate(
        LispObject expression, Context context) {
      var len = ListOperations.Count(expression);
      if (!len.HasValue) throw new LispException();
      if (len < 2) throw new CompilerError();
      if (len > 3) throw new CompilerError();

      var cond = ListOperations.GetCar(expression);
      if (cond == null) throw new LispException();
      expression = ListOperations.GetCdr(expression);
      if (expression == null) throw new LispException();
      var ifTrue = ListOperations.GetCar(expression);
      if (ifTrue == null) throw new LispException();
      expression = ListOperations.GetCdr(expression);
      if (expression == null) throw new LispException();
      if (len == 3) {
        // Expression will be used for value-if-false. If the list was < 3, this is Nil.
        // If it is 3, we take it from the car of the third cons.
        expression = ListOperations.GetCar(expression);
        if (expression == null) throw new LispException();
      }

      var astcond = Compiler.ExpressionToIntermediate(cond, context);
      if (astcond is AstConst) {
        var b = BoolType.From(((AstConst)astcond).Value);
        if (b == BoolType.T) return Compiler.ExpressionToIntermediate(ifTrue, context);
        if (b == BoolType.F) return Compiler.ExpressionToIntermediate(expression, context);
        if (b == null) {
          LispInterpreter.ClearException();
        }
      }

      return new AstIf(
          astcond,
          Compiler.ExpressionToIntermediate(ifTrue, context),
          Compiler.ExpressionToIntermediate(expression, context));
    }
  }

  public class PrognSpecialForm : SpecialForm {

    public virtual AstOp ExpressionToIntermediate(
        LispObject expression, Context context) {
      return new AstProgn(ParseForms(expression, context));
    }

    protected virtual List<AstOp> ParseForms(LispObject forms, Context context) {
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

  public class LetSpecialForm : PrognSpecialForm {
    public override AstOp ExpressionToIntermediate(
        LispObject expression, Context context) {
      var len = ListOperations.Count(expression);
      if (!len.HasValue) throw new LispException();
      if (len < 1) throw new CompilerError();
      var bindinglist = ListOperations.GetCar(expression);
      if (bindinglist == null) throw new LispException();
      var rest = ListOperations.GetCdr(expression);
      if (rest == null) throw new LispException();

      List<Tuple<Binding, AstOp>> bindings;
      var innerContext = ParseBindingList(bindinglist, context, out bindings);
      var forms = ParseForms(rest, innerContext);
      return new AstLet(bindings, forms);
    }

    protected virtual Context ParseBindingList(
        LispObject bindinglist, Context outerContext,
        out List<Tuple<Binding, AstOp>> bindings) {
      bindings = new List<Tuple<Binding, AstOp>>();
      Context context = new ScopedContext(outerContext);
      foreach (var newbind in ListOperations.IterList(bindinglist)) {
        if (newbind is SymbolType) {
          if (SymbolType.IsSelfEvaluating((SymbolType)newbind))
            throw new CompilerError();
          var binding = context.AddBinding((SymbolType)newbind);
          bindings.Add(Tuple.Create<Binding, AstOp>(binding, new AstConst(NilType.Nil)));
        } else {
          var len = ListOperations.Count(newbind);
          if (!len.HasValue) throw new LispException();
          if (len.Value == 0) throw new CompilerError();
          if (len.Value > 2) throw new CompilerError();
          var symb = ListOperations.GetCar(newbind);
          if (symb == null) throw new LispException();
          if (!(symb is SymbolType) || SymbolType.IsSelfEvaluating((SymbolType)symb))
            throw new CompilerError();
          LispObject value = NilType.Nil;
          if (len.Value == 2) {
            value = ListOperations.GetCdr(newbind);
            if (value == null) throw new LispException();
            value = ListOperations.GetCar(value);
            if (value == null) throw new LispException();
          }
          var binding = context.AddBinding((SymbolType)symb);
          var boundValue = Compiler.ExpressionToIntermediate(value, context);
          bindings.Add(Tuple.Create(binding, boundValue));
        }
      }
      return context;
    }
  }

  public class LambdaSpecialForm : PrognSpecialForm {
    public override AstOp ExpressionToIntermediate(
        LispObject expression, Context context) {
      List<Binding> args;
      List<AstOp> forms;
      ParseArgsAndForms(expression, context, out args, out forms);
      return new AstLambda(args, forms);
    }

    protected void ParseArgsAndForms(
        LispObject expression, Context context,
        out List<Binding> args, out List<AstOp> forms) {
      var len = ListOperations.Count(expression);
      if (!len.HasValue) throw new LispException();
      if (len.Value < 1) throw new CompilerError();
      var arglist = ListOperations.GetCar(expression);
      if (arglist == null) throw new LispException();
      var rest = ListOperations.GetCdr(expression);
      if (rest == null) throw new LispException();

      var symargs = ParseArgumentList(arglist);
      var innerContext = new ClosuredScopedContext(context, symargs);
      args = symargs.Select(s => innerContext.GetBinding(s)).ToList();
      forms = ParseForms(rest, innerContext);
    }

    protected virtual List<SymbolType> ParseArgumentList(LispObject arglist) {
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
  }

  public class DefunSpecialForm : LambdaSpecialForm {
    public override AstOp ExpressionToIntermediate(
        LispObject expression, Context context) {
      var len = ListOperations.Count(expression);
      if (!len.HasValue) throw new LispException();
      if (len.Value < 2) throw new CompilerError();

      var nameObj = ListOperations.GetCar(expression);
      if (nameObj == null) throw new LispException();
      if (!(nameObj is SymbolType) || SymbolType.IsSelfEvaluating((SymbolType)nameObj))
        throw new CompilerError();
      var name = (SymbolType)nameObj;

      var rest = ListOperations.GetCdr(expression);
      if (rest == null) throw new LispException();

      var binding = context.AddBinding(name);

      List<Binding> args;
      List<AstOp> forms;
      ParseArgsAndForms(rest, context, out args, out forms);
      return new AstDefun(binding, args, forms);
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

    Expression CompileCSharp();
  }

  public abstract class AstOpBase : AstOp {
    public override string ToString() {
      var sb = new StringBuilder();
      AppendAstStringIndented(sb, 0);
      return sb.ToString();
    }

    public static Expression ValueOrReturn(
        Type exprType, Expression expr, LabelTarget returnTarget) {
      var intermediate = Expression.Variable(exprType);
      return Expression.Block(
        typeof(LispObject),
        ImmutableList.Create(intermediate),
        Expression.Assign(intermediate, expr),
        Expression.Condition(
          Expression.Equal(intermediate, Expression.Constant(null, exprType)),
          Expression.Block(
            exprType,
            Expression.Return(
              returnTarget, Expression.Constant(null, exprType)),
            Expression.Constant(null, exprType)),
          intermediate));
    }

    public static Expression ValueOrReturn<T>(Expression expr, LabelTarget returnTarget)
        where T: LispObject {
      return ValueOrReturn(typeof(T), expr, returnTarget);
    }

    public static Expression ValueOrReturn(Expression expr, LabelTarget returnTarget) {
      return ValueOrReturn(expr.Type, expr, returnTarget);
    }

    public abstract void AppendAstStringIndented(StringBuilder sb, int baseIndent);

    public abstract Expression CompileCSharp();
  }

  public class AstConst : AstOpBase {
    public LispObject Value { get; }
    public AstConst(LispObject value) {
      Value = value;
    }

    public override void AppendAstStringIndented(StringBuilder sb, int baseIndent) {
      sb.AppendFormat("[AST-Constant: {0}]", Value);
    }

    public override Expression CompileCSharp() {
      return Expression.Constant(Value);
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

    public override Expression CompileCSharp() {
      var returnTarget = Expression.Label(typeof(LispObject));
      return Expression.Block(
        typeof(LispObject),
        Expression.Return(
          returnTarget,
          Expression.Call(
            typeof(CallableOperations), "Call", null,
            ValueOrReturn(Function.CompileCSharp(), returnTarget),
            ValueOrReturn(
              Expression.Call(
                typeof(IConsType), "ToLispTuple", null,
                Expression.Convert(
                  Expression.NewArrayInit(
                    typeof(LispObject),
                    Arguments.Select(
                      arg => ValueOrReturn(arg.CompileCSharp(), returnTarget))),
                  typeof(IReadOnlyList<LispObject>))),
              returnTarget))),
        Expression.Label(returnTarget, Expression.Constant(null, typeof(LispObject))));
    }
  }

  public class AstIf : AstOpBase {
    public AstOp Condition { get; }
    public AstOp ValueIfTrue { get; }
    public AstOp ValueIfFalse { get; }

    public AstIf(AstOp condition, AstOp valueIfTrue, AstOp valueIfFalse) {
      Condition = condition;
      ValueIfTrue = valueIfTrue;
      ValueIfFalse = valueIfFalse;
    }

    public override void AppendAstStringIndented(StringBuilder sb, int baseIndent) {
      sb.AppendLine("[AST-If:");
      sb.Append(' ', baseIndent + 2);
      sb.Append("Condition: ");
      Condition.AppendAstStringIndented(sb, baseIndent + 2);
      sb.AppendLine();
      sb.Append(' ', baseIndent + 2);
      sb.Append("Value If True: ");
      ValueIfTrue.AppendAstStringIndented(sb, baseIndent + 2);
      sb.AppendLine();
      sb.Append(' ', baseIndent + 2);
      sb.Append("Value If False: ");
      ValueIfFalse.AppendAstStringIndented(sb, baseIndent + 2);
      sb.AppendLine();
      sb.Append(' ', baseIndent);
      sb.Append("]");
    }

    public override Expression CompileCSharp() {
      var returnTarget = Expression.Label(typeof(LispObject));
      return Expression.Block(
        typeof(LispObject),
        Expression.Condition(
          Expression.Equal(
            ValueOrReturn(
              Expression.Call(
                typeof(BoolType), "From", null,
                ValueOrReturn(Condition.CompileCSharp(), returnTarget)),
              returnTarget),
            Expression.Constant(BoolType.T)),
          // Rest of contitional goes here.
          Expression.Return(returnTarget, ValueIfTrue.CompileCSharp()),
          Expression.Return(returnTarget, ValueIfFalse.CompileCSharp())),
        Expression.Label(returnTarget, Expression.Constant(null, typeof(LispObject))));
    }
  }

  public class AstProgn : AstOpBase {
    public IList<AstOp> Forms { get; }
    public AstProgn(IList<AstOp> forms) {
      Forms = forms;
    }

    public override void AppendAstStringIndented(StringBuilder sb, int baseIndent) {
      if (Forms.Count == 0) {
        sb.Append("[AST-Progn]");
      } else {
        sb.AppendLine("[AST-Progn:");
        AppendForms(sb, baseIndent);
        sb.Append(' ', baseIndent);
        sb.Append("]");
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

    public override Expression CompileCSharp() {
      return GetFormsCSharp();
    }

    protected Expression GetFormsCSharp() {
      if (Forms.Count == 0) return Expression.Constant(NilType.Nil, typeof(LispObject));

      var returnTarget = Expression.Label(typeof(LispObject));

      var expressions = new List<Expression>(Forms.Count + 1);
      for (int i = 0; i < Forms.Count - 1; i++) {
        expressions.Add(
          ValueOrReturn(Forms[i].CompileCSharp(), returnTarget));
      }
      expressions.Add(
        Expression.Return(returnTarget, Forms[Forms.Count - 1].CompileCSharp()));
      expressions.Add(
        Expression.Label(returnTarget, Expression.Constant(null, typeof(LispObject))));

      return Expression.Block(typeof(LispObject), expressions);
    }
  }

  public class AstLet : AstProgn {
    public IList<Tuple<Binding, AstOp>> Bindings { get; }
    public AstLet(IList<Tuple<Binding, AstOp>> bindings, IList<AstOp> forms)
        : base(forms) {
      Bindings = bindings;
    }

    public override void AppendAstStringIndented(StringBuilder sb, int baseIndent) {
      if (Forms.Count == 0 && Bindings.Count == 0) {
        sb.Append("[AST-Let]");
      } else {
        sb.AppendLine("[AST-Let:");
        AppendBindings(sb, baseIndent);
        AppendForms(sb, baseIndent);
        sb.Append(' ', baseIndent);
        sb.Append("]");
      }
    }

    protected virtual void AppendBindings(StringBuilder sb, int baseIndent) {
      for (int i = 0; i < Bindings.Count; i++) {
        sb.Append(' ', baseIndent + 2);
        sb.AppendFormat("Let Binding {0}:", i);
        sb.AppendLine();
        sb.Append(' ', baseIndent + 4);
        sb.Append("Bound Symbol: ");
        Bindings[i].Item1.AppendAstStringIndented(sb, baseIndent + 4);
        sb.AppendLine();
        sb.Append(' ', baseIndent + 4);
        sb.Append("To Value: ");
        Bindings[i].Item2.AppendAstStringIndented(sb, baseIndent + 4);
        sb.AppendLine();
      }
    }

    public override Expression CompileCSharp() {
      var returnTarget = Expression.Label(typeof(LispObject));

      var expressions = new List<Expression>(Bindings.Count + 2);
      foreach (var b in Bindings) {
        expressions.Add(
          b.Item1.SetValueCSharp(
            ValueOrReturn(b.Item2.CompileCSharp(), returnTarget)));
      }
      expressions.Add(
        Expression.Return(returnTarget, base.CompileCSharp()));
      expressions.Add(
        Expression.Label(returnTarget, Expression.Constant(null, typeof(LispObject))));

      return Expression.Block(
        typeof(LispObject),
        Bindings.Select(b => (ParameterExpression)b.Item1.CompileCSharp()),
        expressions);
    }
  }

  public class AstLambda : AstProgn {
    public IList<Binding> Args { get; }
    public AstLambda(IList<Binding> args, IList<AstOp> forms) : base(forms) {
      Args = args;
    }

    public override void AppendAstStringIndented(StringBuilder sb, int baseIndent) {
      if (Args.Count == 0 && Forms.Count == 0) {
        sb.Append("[AST-Lambda]");
      } else {
        sb.AppendLine("[AST-Lambda:");
        AppendArgs(sb, baseIndent);
        AppendForms(sb, baseIndent);
        sb.Append(' ', baseIndent);
        sb.Append("]");
      }
    }

    protected virtual void AppendArgs(StringBuilder sb, int baseIndent) {
      for (int i = 0; i < Args.Count; i++) {
        sb.Append(' ', baseIndent + 2);
        sb.AppendFormat("Arg {0}: ", i);
        Args[i].AppendAstStringIndented(sb, baseIndent + 4);
        sb.AppendLine();
      }
    }

    protected virtual SymbolType GetFunctionName() {
      return SymbolType.Create("<lambda>");
    }

    public override Expression CompileCSharp() {
      var argsParameter = Expression.Parameter(typeof(LispObject), "_in-args");
      return Expression.Call(
        typeof(FunctionType), "Create", null,
        Expression.Constant(GetFunctionName()),
        Expression.Lambda(
          typeof(Func<LispObject, LispObject>),
          GetLambdaBodyCSharp(argsParameter),
          GetFunctionName().Identifier,
          ImmutableList.Create(argsParameter)));

    }

    private Expression GetLambdaBodyCSharp(ParameterExpression argsParameter) {
      var returnTarget = Expression.Label(typeof(LispObject));

      return Expression.Block(
        typeof(LispObject),
        Args.Select(b => (ParameterExpression)b.CompileCSharp()),
        ValueOrReturn(GetBindArgsBlockCSharp(argsParameter), returnTarget),
        Expression.Return(returnTarget, GetFormsCSharp()),
        Expression.Label(returnTarget, Expression.Constant(null, typeof(LispObject))));
    }

    private Expression GetBindArgsBlockCSharp(ParameterExpression argsParameter) {
      var returnTarget = Expression.Label(typeof(LispObject));
      var argList = Expression.Variable(typeof(List<LispObject>), "Arg List");
      var argCount = Expression.Variable(typeof(int), "Arg Count");

      return Expression.Block(
        typeof(LispObject),
        ImmutableList.Create(argList, argCount),
        Expression.Assign(
          argList,
          Expression.Call(
            typeof(Arguments), "GetPositionalArguments", null, argsParameter)),
        // Check if the list is null and return null if it is.
        Expression.Condition(
          Expression.Equal(argList, Expression.Constant(null, typeof(List<LispObject>))),
          Expression.Return(returnTarget, Expression.Constant(null, typeof(LispObject))),
          Expression.Empty()),
        Expression.Assign(
          argCount,
          Expression.Property(argList, "Count")),
        // Check if the list has the correct number of arguments.
        Expression.Condition(
          Expression.NotEqual(argCount, Expression.Constant(Args.Count)),
          Expression.Block(
            Expression.Call(typeof(LispInterpreter), "SetException", null,
              Expression.Call(typeof(ExceptionType), "CreateTypeError", null,
                Expression.Constant(
                  "function " + GetFunctionName().Identifier + " expected " +
                  Args.Count + " arguments, got {0}"),
                Expression.NewArrayInit(
                  typeof(object),
                  Expression.Convert(argCount, typeof(object))))),
            Expression.Return(
              returnTarget, Expression.Constant(null, typeof(LispObject)))),
          Expression.Empty()),
        Expression.Return(returnTarget, GetInstantiateArgsBlock(argList)),
        Expression.Label(returnTarget, Expression.Constant(null, typeof(LispObject))));
    }

    private Expression GetInstantiateArgsBlock(ParameterExpression argList) {
      var returnTarget = Expression.Label(typeof(LispObject));

      var bindExpressions = new List<Expression>(Args.Count + 2);

      int item = 0;
      foreach (var arg in Args) {
        bindExpressions.Add(
          arg.SetValueCSharp(
            ValueOrReturn(
              Expression.Property(argList, "Item", Expression.Constant(item++)),
              returnTarget)));
      }

      bindExpressions.Add(
        Expression.Return(returnTarget, Expression.Constant(NilType.Nil)));
      bindExpressions.Add(
        Expression.Label(returnTarget, Expression.Constant(null, typeof(LispObject))));

      return Expression.Block(
        typeof(LispObject),
        bindExpressions);
    }
  }

  public class AstDefun : AstLambda {
    public Binding Name { get; }
    public AstDefun(Binding name, IList<Binding> args, IList<AstOp> forms)
        : base(args, forms) {
      Name = name;
    }

    public override void AppendAstStringIndented(StringBuilder sb, int baseIndent) {
      sb.AppendLine("[AST-Defun:");
      sb.Append(' ', baseIndent + 2);
      sb.Append("Name: ");
      Name.AppendAstStringIndented(sb, baseIndent + 4);
      sb.AppendLine();
      AppendArgs(sb, baseIndent);
      AppendForms(sb, baseIndent);
      sb.Append(' ', baseIndent);
      sb.Append("]");
    }

    protected override SymbolType GetFunctionName() {
      return Name.BoundSymbol;
    }

    public override Expression CompileCSharp() {
      var returnTarget = Expression.Label(typeof(LispObject));
      var intermediate = Expression.Variable(typeof(LispObject));
      return Expression.Block(
        typeof(LispObject),
        ImmutableList.Create(intermediate),
        Expression.Assign(
          intermediate, ValueOrReturn(base.CompileCSharp(), returnTarget)),
        ValueOrReturn(Name.SetValueCSharp(intermediate), returnTarget),
        Expression.Return(returnTarget, intermediate),
        Expression.Label(returnTarget, Expression.Constant(null, typeof(LispObject))));
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
      db.Add(SymbolType.Create("let"), new LetSpecialForm());
      db.Add(SymbolType.Create("progn"), new PrognSpecialForm());
      db.Add(SymbolType.Create("if"), new IfSpecialForm());
      specialForms = db.ToImmutable();
    }

    public static AstOp ExpressionToIntermediate(LispObject expression, Context context) {
      // TODO(zstewar1): macroexpand the expression first.
      if (expression is ConsType) {
        var op = ListOperations.GetCar(expression);
        if (op == null) throw new LispException();
        if (op is SymbolType) {
          SpecialForm sf;
          if (specialForms.TryGetValue((SymbolType)op, out sf)) {
            var args = ListOperations.GetCdr(expression);
            if (args == null) throw new LispException();
            return sf.ExpressionToIntermediate(args, context);
          }
        }
        return functionForm.ExpressionToIntermediate(expression, context);
      } else {
        return plainPrimitiveForm.ExpressionToIntermediate(expression, context);
      }
    }

    public static Func<LispObject> CompileExpression(
        LispObject expression, Context context) {
      var ast = ExpressionToIntermediate(expression, context);
      var compiler = Expression.Lambda<Func<LispObject>>(ast.CompileCSharp());
      return compiler.Compile();
    }
  }
}
