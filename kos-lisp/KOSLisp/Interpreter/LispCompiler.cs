using System;
using System.Text;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;

using ZStewart.KOSLisp.Types;
using ZStewart.KOSLisp.Types.Helpers;
using ZStewart.KOSLisp.Compile;
using ZStewart.KOSLisp.Compile.AST;
using ZStewart.KOSLisp.Compile.Contexts;
using ZStewart.KOSLisp.Compile.Generators.CSharp;
using ZStewart.KOSLisp.Compile.SpecialForms;

namespace ZStewart.KOSLisp.Interpreter {

  public class CompilerError : Exception {}

  public class DelegateSpecialForm : SpecialForm {
    private Func<LispObject, Context, AstOp> intermediateConverter;
    public DelegateSpecialForm(Func<LispObject, Context, AstOp> intermediateConverter) {
      this.intermediateConverter = intermediateConverter;
    }

    public AstOp ToAst(LispObject expression, Context context) {
      return intermediateConverter(expression, context);
    }
  }

  public class LispCompiler {

    private static readonly ImmutableDictionary<SymbolType, SpecialForm> specialForms;

    private static SpecialForm functionForm = new DelegateSpecialForm((exp, ctx) => {
      // TODO(zstewar1): Better error message.
      if (!ListOperations.Proper(exp)) throw new CompilerError();
      var fn = ToAst(ListOperations.GetCar(exp), ctx);
      var args = ListOperations.IterList(ListOperations.GetCdr(exp))
        .Select(arg => ToAst(arg, ctx))
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

    public static AstOp ToAst(LispObject expression, Context context) {
      // TODO(zstewar1): macroexpand the expression first.
      if (expression is ConsType) {
        var op = ListOperations.GetCar(expression);
        if (op is SymbolType) {
          SpecialForm sf;
          if (specialForms.TryGetValue((SymbolType)op, out sf)) {
            var args = ListOperations.GetCdr(expression);
            return sf.ToAst(args, context);
          }
        }
        return functionForm.ToAst(expression, context);
      } else {
        return plainPrimitiveForm.ToAst(expression, context);
      }
    }

    public static Func<LispObject> CompileExpression(
        LispObject expression, Context context) {
      var ast = ToAst(expression, context);
      Console.WriteLine(ast);
      var expressionGenerator = new CSharpGeneratorFactory().Create(ast);
      var compiler = Expression.Lambda<Func<LispObject>>(expressionGenerator.Emit());
      return compiler.Compile();
    }
  }
}
