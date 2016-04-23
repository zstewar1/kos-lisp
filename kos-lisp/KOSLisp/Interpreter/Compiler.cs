using System;
using System.Collections.Generic;
using System.Collections.Immutable;

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

  public interface Binding {
    // TODO(zstewar1): Variable bindings
  }

  public interface Context {
    // TODO(zstewar1): binding context
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

  public interface AstOp {
    // TODO(zstewar1): Abstract Syntax Tree operation.
  }

  public class AstConst : AstOp {
    public LispObject Value { get; }
    public AstConst(LispObject value) {
      Value = value;
    }
  }

  public static class Compiler {

    private static readonly ImmutableDictionary<SymbolType, SpecialForm> specialForms;

    private SpecialForm functionForm = new DelegateSpecialForm((exp, ctx) => {
      return null;
    });

    private SpecialForm plainPrimitiveForm = new DelegateSpecialForm((exp, ctx) => {
      return null;
    });

    static Compiler () {
      var db =
        ImmutableDictionary.CreateBuilder<Symboltype, SpecialForm>(
          SymbolType.SymbolComparer);
      db.Add("quote", new DelegateSpecialForm((exp, ctx) => {
        var len = ListOperations.Count(exp);
        if (!len.HasValue) throw new LispException();
        // TODO(zstewar1): More specific error?
        if (len.Value != 1) throw new CompilerError();
        var val = ListOperations.GetCar(exp);
        if (val == null) throw new LispException();
        return new AstConst(val);
      }));
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
        return plainPrimitiveForm(expressionContext);
      }
    }
  }
}
