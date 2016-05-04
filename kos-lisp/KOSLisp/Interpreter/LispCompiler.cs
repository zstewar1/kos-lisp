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

  public class LispCompiler : Compiler {

    private readonly ImmutableDictionary<SymbolType, SpecialForm> specialForms;

    private readonly SpecialForm functionForm = new FuncCallSpecialForm();
    private readonly SpecialForm primitiveForm = new PrimitiveSpecialForm();

    public LispCompiler () {
      var db =
        ImmutableDictionary.CreateBuilder<SymbolType, SpecialForm>();
      db.Add(SymbolType.Create("quote"), new QuoteSpecialForm());
      db.Add(SymbolType.Create("lambda"), new LambdaSpecialForm());
      db.Add(SymbolType.Create("defun"), new DefunSpecialForm());
      db.Add(SymbolType.Create("let"), new LetSpecialForm());
      db.Add(SymbolType.Create("progn"), new PrognSpecialForm());
      db.Add(SymbolType.Create("if"), new IfSpecialForm());
      specialForms = db.ToImmutable();
    }

    public AstOp ToAst(LispObject expression, Context context) {
      // TODO(zstewar1): macroexpand the expression first.
      if (expression is ConsType) {
        var op = ListOperations.GetCar(expression);
        if (op is SymbolType) {
          SpecialForm sf;
          if (specialForms.TryGetValue((SymbolType)op, out sf)) {
            var args = ListOperations.GetCdr(expression);
            return sf.ToAst(args, context, this);
          }
        }
        return functionForm.ToAst(expression, context, this);
      } else {
        return primitiveForm.ToAst(expression, context, this);
      }
    }

    public static Func<LispObject> CompileExpression(
        LispObject expression, Context context) {
      var ast = new LispCompiler().ToAst(expression, context);
      Console.WriteLine(ast);
      var expressionGenerator = new CSharpGeneratorFactory().Create(ast);
      var compiler = Expression.Lambda<Func<LispObject>>(expressionGenerator.Emit());
      return compiler.Compile();
    }
  }
}
