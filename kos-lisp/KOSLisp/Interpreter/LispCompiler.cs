using System;
using System.Text;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;

using ZStewart.KOSLisp.Types;
using ZStewart.KOSLisp.Types.Helpers;
using ZStewart.KOSLisp.Compiler;
using ZStewart.KOSLisp.Compiler.AST;
using ZStewart.KOSLisp.Compiler.Generators.CSharp;

namespace ZStewart.KOSLisp.Interpreter {

  public class CompilerError : Exception {}

  /// <summary>
  /// The base, or global context. No symbols are ever bound in this context.
  /// </summary>
  public class GlobalContext : Context {
    private ModuleType module;
    public GlobalContext(ModuleType module) {
      this.module = module;
    }
    public virtual AstBinding GetBinding(SymbolType symbol) {
      return AddBinding(symbol);
    }

    public virtual AstBinding AddBinding(SymbolType symbol) {
      return Ast.BindGlobal(module, symbol);
    }
  }

  public class ScopedContext : Context {
    private readonly Context parentScope;
    protected readonly Dictionary<SymbolType, AstBinding> bindings =
      new Dictionary<SymbolType, AstBinding>();

    public ScopedContext(Context parentScope, IEnumerable<SymbolType> newBindings) {
      this.parentScope = parentScope;

      foreach (var symbol in newBindings) {
        if (bindings.ContainsKey(symbol)) {
          // TODO(zstewar1): Better error messages
          throw new CompilerError();
        }
        bindings.Add(symbol, Ast.BindLocal(symbol));
      }
    }

    public ScopedContext(Context parentScope, params SymbolType[] newBindings)
        : this(parentScope, (IEnumerable<SymbolType>)newBindings) {}

    public virtual AstBinding GetBinding(SymbolType symbol) {
      var local = GetLocalBinding(symbol);
      if (local == null) return GetParentBinding(symbol);
      return local;
    }

    public virtual AstBinding AddBinding(SymbolType symbol) {
      AstBinding newbind = GetLocalBinding(symbol);
      if (newbind != null) return newbind;
      newbind = Ast.BindLocal(symbol);
      bindings.Add(symbol, newbind);
      return newbind;
    }

    protected virtual AstBinding GetLocalBinding(SymbolType symbol) {
      AstBinding binding;
      bindings.TryGetValue(symbol, out binding);
      // Return a binding or null if unbound. (default(binding) is null).
      return binding;
    }

    protected virtual AstBinding GetParentBinding(SymbolType symbol) {
      return parentScope.GetBinding(symbol);
    }
  }

  public class ClosuredScopedContext : ScopedContext {
    public ClosuredScopedContext(Context parentScope, IEnumerable<SymbolType> newBindings)
        : base(parentScope, newBindings) {}
    public ClosuredScopedContext(Context parentScope, params SymbolType[] newBindings)
        : this(parentScope, (IEnumerable<SymbolType>)newBindings) {}

    protected override AstBinding GetParentBinding(SymbolType symbol) {
      var b = base.GetParentBinding(symbol);
      if (b != null)
        b.HasClosure = true;
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
      if (len < 2) throw new CompilerError();
      if (len > 3) throw new CompilerError();

      var cond = ListOperations.GetCar(expression);
      expression = ListOperations.GetCdr(expression);
      var ifTrue = ListOperations.GetCar(expression);
      expression = ListOperations.GetCdr(expression);
      if (len == 3) {
        // Expression will be used for value-if-false. If the list was < 3, this is Nil.
        // If it is 3, we take it from the car of the third cons.
        expression = ListOperations.GetCar(expression);
      }

      var astcond = LispCompiler.ExpressionToIntermediate(cond, context);
      if (astcond is AstConst) {
        LispObject b = null;
        try {
          b = BoolType.From(((AstConst)astcond).Value);
        } catch (ExceptionWrapper) {}
        if (b == BoolType.T) return LispCompiler.ExpressionToIntermediate(ifTrue, context);
        if (b == BoolType.F)
          return LispCompiler.ExpressionToIntermediate(expression, context);
      }

      return Ast.If(
        astcond,
        LispCompiler.ExpressionToIntermediate(ifTrue, context),
        LispCompiler.ExpressionToIntermediate(expression, context));
    }
  }

  public class PrognSpecialForm : SpecialForm {

    public virtual AstOp ExpressionToIntermediate(
        LispObject expression, Context context) {
      return Ast.Progn(ParseForms(expression, context));
    }

    protected virtual List<AstOp> ParseForms(LispObject forms, Context context) {
      if (!ListOperations.Proper(forms)) throw new CompilerError();
      return ListOperations.IterList(forms)
        .Select(form => LispCompiler.ExpressionToIntermediate(form, context))
        .ToList();
    }
  }

  public class LetSpecialForm : PrognSpecialForm {
    public override AstOp ExpressionToIntermediate(
        LispObject expression, Context context) {
      var len = ListOperations.Count(expression);
      if (len < 1) throw new CompilerError();
      var bindinglist = ListOperations.GetCar(expression);
      var rest = ListOperations.GetCdr(expression);

      List<Tuple<AstBinding, AstOp>> bindings;
      var innerContext = ParseBindingList(bindinglist, context, out bindings);
      var forms = ParseForms(rest, innerContext);
      return Ast.Let(bindings, forms);
    }

    protected virtual Context ParseBindingList(
        LispObject bindinglist, Context outerContext,
        out List<Tuple<AstBinding, AstOp>> bindings) {
      bindings = new List<Tuple<AstBinding, AstOp>>();
      Context context = new ScopedContext(outerContext);
      foreach (var newbind in ListOperations.IterList(bindinglist)) {
        if (newbind is SymbolType) {
          if (SymbolType.IsSelfEvaluating((SymbolType)newbind))
            throw new CompilerError();
          var binding = context.AddBinding((SymbolType)newbind);
          bindings.Add(Tuple.Create<AstBinding, AstOp>(binding, Ast.Const(NilType.Nil)));
        } else {
          var len = ListOperations.Count(newbind);
          if (len == 0) throw new CompilerError();
          if (len > 2) throw new CompilerError();
          var symb = ListOperations.GetCar(newbind);
          if (!(symb is SymbolType) || SymbolType.IsSelfEvaluating((SymbolType)symb))
            throw new CompilerError();
          LispObject value = NilType.Nil;
          if (len == 2) {
            value = ListOperations.GetCar(ListOperations.GetCdr(newbind));
          }
          var binding = context.AddBinding((SymbolType)symb);
          var boundValue = LispCompiler.ExpressionToIntermediate(value, context);
          bindings.Add(Tuple.Create(binding, boundValue));
        }
      }
      return context;
    }
  }

  public class LambdaSpecialForm : PrognSpecialForm {
    public override AstOp ExpressionToIntermediate(
        LispObject expression, Context context) {
      List<AstBinding> args;
      List<AstOp> forms;
      ParseArgsAndForms(expression, context, out args, out forms);
      return Ast.Lambda(args, forms);
    }

    protected void ParseArgsAndForms(
        LispObject expression, Context context,
        out List<AstBinding> args, out List<AstOp> forms) {
      var len = ListOperations.Count(expression);
      if (len < 1) throw new CompilerError();
      var arglist = ListOperations.GetCar(expression);
      var rest = ListOperations.GetCdr(expression);

      var symargs = ParseArgumentList(arglist);
      var innerContext = new ClosuredScopedContext(context, symargs);
      args = symargs.Select(s => innerContext.GetBinding(s)).ToList();
      forms = ParseForms(rest, innerContext);
    }

    protected virtual List<SymbolType> ParseArgumentList(LispObject arglist) {
      // TODO(zstewar1): We'll require a more advanced notation (for both definition and
      // calling) once we start supporting keyword arguments.
      if (!ListOperations.Proper(arglist)) throw new CompilerError();
      return ListOperations.IterList<SymbolType>(arglist).ToList();
    }
  }

  public class DefunSpecialForm : LambdaSpecialForm {
    public override AstOp ExpressionToIntermediate(
        LispObject expression, Context context) {
      var len = ListOperations.Count(expression);
      if (len < 2) throw new CompilerError();

      var nameObj = ListOperations.GetCar(expression);
      if (!(nameObj is SymbolType) || SymbolType.IsSelfEvaluating((SymbolType)nameObj))
        throw new CompilerError();
      var name = (SymbolType)nameObj;
      var rest = ListOperations.GetCdr(expression);

      var binding = context.AddBinding(name);

      List<AstBinding> args;
      List<AstOp> forms;
      ParseArgsAndForms(rest, context, out args, out forms);
      return Ast.Defun(binding, args, forms);
    }
  }

  public static class LispCompiler {

    private static readonly ImmutableDictionary<SymbolType, SpecialForm> specialForms;

    private static SpecialForm functionForm = new DelegateSpecialForm((exp, ctx) => {
      // TODO(zstewar1): Better error message.
      if (!ListOperations.Proper(exp)) throw new CompilerError();
      var fn = ExpressionToIntermediate(ListOperations.GetCar(exp), ctx);
      var args = ListOperations.IterList(ListOperations.GetCdr(exp))
        .Select(arg => ExpressionToIntermediate(arg, ctx))
        .ToList();
      return Ast.Call(fn, args);
    });

    private static SpecialForm plainPrimitiveForm =
      new DelegateSpecialForm((exp, ctx) => {
        if (!(exp is SymbolType) || SymbolType.IsSelfEvaluating((SymbolType)exp)) {
          return Ast.Const(exp);
        }
        return ctx.GetBinding((SymbolType)exp);
      });

    static LispCompiler () {
      var db =
        ImmutableDictionary.CreateBuilder<SymbolType, SpecialForm>();
      db.Add(SymbolType.Create("quote"), new DelegateSpecialForm((exp, ctx) => {
        var len = ListOperations.Count(exp);
        // TODO(zstewar1): More specific error?
        if (len != 1) throw new CompilerError();
        return Ast.Const(ListOperations.GetCar(exp));
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
        if (op is SymbolType) {
          SpecialForm sf;
          if (specialForms.TryGetValue((SymbolType)op, out sf)) {
            var args = ListOperations.GetCdr(expression);
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
      Console.WriteLine(ast);
      var expressionGenerator = new CSharpGeneratorFactory().Create(ast);
      var compiler = Expression.Lambda<Func<LispObject>>(expressionGenerator.Emit());
      return compiler.Compile();
    }
  }
}
