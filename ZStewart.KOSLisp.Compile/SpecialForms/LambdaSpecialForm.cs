using System.Collections.Generic;
using System.Linq;

using static ZStewart.KOSLisp.Types.ExceptionType;

using ZStewart.KOSLisp.Compile.AST;
using ZStewart.KOSLisp.Compile.Contexts;
using ZStewart.KOSLisp.Types;
using ZStewart.KOSLisp.Types.Helpers;

namespace ZStewart.KOSLisp.Compile.SpecialForms {
  /// <summary>
  /// A form that parses an unnamed function definition.
  /// </summary>
  public class LambdaSpecialForm : PrognSpecialForm {
    /// <summary>
    /// Convert the given expression to an AST.
    /// </summary>
    /// <param name="expression">
    /// The lisp object representing the expression to convert.
    /// </param>
    /// <param name="context">
    /// Variable context of the outer expression. Can be used to look up variables which
    /// this expression does not itself bind.
    /// </param>
    /// <param name="compiler">
    /// The Lisp compiler, which this special form can use to parse sub-expressions.
    /// </param>
    public override AstOp ToAst(
        LispObject expression, Context context, SemanticAnalyzer compiler) {
      List<Tuple<ArgumentProperties, AstBinding, AstOp>> args;
      List<AstOp> forms;
      ParseArgsAndForms(expression, context, compiler, out args, out forms);
      return Ast.Lambda(args, forms);
    }

    /// <summary>
    /// Parses out just the arguments and forms of a lambda expression. This separates out
    /// parsing these parts of the expression from the ToAst logic to make it easier for
    /// derived classes to override the behavior of the lambda.
    /// </summary>
    protected void ParseArgsAndForms(
        LispObject expression, Context context, SemanticAnalyzer compiler,
        out List<Tuple<ArgumentProperties, AstBinding, AstOp>> args,
        out List<AstOp> forms) {
      int len;
      try {
        len = ListOperations.Count(expression);
      } catch (ExceptionWrapper ex) {
        throw ThrowSyntaxError(ex, "lambda expression must be a proper list");
      }

      if (len < 1) {
        throw ThrowSyntaxError("function definition requires argument list");
      }

      var innerContext = ParseArgumentList(
        ListOperations.GetCar(expression), context, compiler, out args);

      // Parse the forms in the context of the new bindings.
      forms = ParseForms(ListOperations.GetCdr(expression), innerContext, compiler);
    }

    /// <summary>
    /// Convert from the arglist to a collection of argument tuples, which contain the
    /// argument properties, binding, and default values, and return a context which
    /// should be the context that forms are evaluated in.
    /// </summary>
    protected Context ParseArgumentList(
        LispObject arglist, Context outerContext, SemanticAnalyzer compiler,
        out List<Tuple<ArgumentProperties, AstBinding, AstOp>> args) {
      args = new List<Tuple<ArgumentProperties, AstBinding, AstOp>>();
      var innerContext = new ClosuredScopedContext(outerContext);

      if (!ListOperations.Proper(arglist)) {
        throw ThrowSyntaxError("argument list must be a proper list");
      }

      var existingArgs = new HashSet<SymbolType>();

      // Compute the arguments and any defaults, and inductively assert that the argument
      // list is in a valid order.
      var lastType = ArgumentType.PositionalOrKeyword;
      // The optionality of the previous argument is only relevant to required-positional
      // arguments. All other argument types only care about the previous argument's type,
      // so this is set to false here and only changed once (to true) by the first
      // optional argument.
      var lastOptional = false;
      for (; !ReferenceEquals(arglist, NilType.Nil);
          arglist = ListOperations.GetCdr(arglist)) {
        var current = ListOperations.GetCar(arglist);
        if (current is SymbolType) {
          var sym = (SymbolType)current;
          if (sym == KeywordSymbolType.Create("&rest+")) {
            CheckRestAllowed(lastType);
            args.Add(
              Tuple.Create<ArgumentProperties, AstBinding, AstOp>(
                new ArgumentProperties(ArgumentType.RestIgnore), null, null));
            lastType = ArgumentType.RestIgnore;
          } else if (sym == KeywordSymbolType.Create("&rest-")) {
            CheckRestAllowed(lastType);
            args.Add(
              Tuple.Create<ArgumentProperties, AstBinding, AstOp>(
                new ArgumentProperties(ArgumentType.RestBlock), null, null));
            lastType = ArgumentType.RestBlock;
          } else if (sym == KeywordSymbolType.Create("&rest")) {
            CheckRestAllowed(lastType);
            arglist = ListOperations.GetCdr(arglist);
            if (ReferenceEquals(arglist, NilType.Nil)) {
              throw ThrowSyntaxError(
                "found &rest at end of list; a symbol must be specified to capture " +
                "rest args (did you mean to specify &rest+ or &rest-?)");
            }
            var restArg = ListOperations.GetCar(arglist);
            if (!(restArg is SymbolType)) {
              throw ThrowSyntaxError(
                "argument to capture rest args must be a symbol, got object of type {0}",
                restArg.__class__);
            }
            var restSym = (SymbolType)restArg;
            if (restSym.IsSelfEvaluating) {
              throw ThrowSyntaxError(
                "cannot capture rest args in self-evaluating symbol '{0}", restSym);
            }
            if (existingArgs.Contains(restSym)) {
              throw ThrowSyntaxError(
                "argument list contains argument '{0} more than once", restSym)
            }
            var binding = innerContext.AddBinding(restSym);
            args.Add(
              Tuple.Create<ArgumentProperties, AstBindng, AstOp>(
                new ArgumentProperties(ArgumentType.RestCapture), binding, null));
            existingArgs.Add(restSym);
            lastType = ArgumentType.RestCapture;
          } else if (sym == KeywordSymbolType.Create("&kw-")) {
            CheckRestKwAllowed(lastType);
            args.Add(
              Tuple.Create<ArgumentProperties, AstBinding, AstOp>(
                new ArgumentProperties(ArgumentType.RestKwIgnore), null, null));
            lastType = ArgumentType.RestKwIgnore;
          } else if (sym == KeywordSymbolType.Create("&kw")) {
            CheckRestKwAllowed(lastType);
            arglist = ListOperations.GetCdr(arglist);
            if (ReferenceEquals(arglist, NilType.Nil)) {
              throw ThrowSyntaxError(
                "found &kw at end of arg list; a symbol must be specified to capture " +
                "keyword args (did you mean to specify &kw-?)");
            }
            var restKwArg = ListOperations.GetCar(arglist);
            if (!(restKwArg is SymbolType)) {
              throw ThrowSyntaxError(
                "argument to capture rest-keyword args must be a symbol, got object of " +
                "type {0}",
                restKwArg.__class__);
            }
            var restKwSym = (SymbolType)restKwSym;
            if (restKwSym.IsSelfEvaluating) {
              throw ThrowSyntaxError(
                "cannot capture rest-keyword args in self-evalutating symbol '{0}",
                restKwSym);
            }
            if (existingArgs.Contains(restKwSym)) {
              throw ThrowSyntaxError(
                "argument list contains argument '{0} more than once", restKwSym)
            }
            var binding = innerContext.AddBinding(restKwSym);
            args.Add(
              Tuple.Create<ArgumentProperties, AstBinding, AstOp>(
                new ArgumentProperties(ArgumentType.RestKwCapture), binding, null));
            existingArgs.Add(restKwSym);
            lastType = ArgumentType.RestKwCapture;
          } else if (sym.IsSelfEvaluating) {
            throw ThrowSyntaxError(
              "found invalid argument (self-evaluating symbol) '{0}", sym);
          } else if (existingArgs.Contains(sym)) {
            throw ThrowSyntaxError(
              "argument list contains argument '{0} more than once", sym)
          } else {
            var argType = GetRequiredType(lastType, lastOptional);
            var binding = innerContext.AddBinding(sym);
            args.Add(
              Tuple.Create<ArgumentProperties, AstBinding, AstOp>(
                new ArumentProperties(argType, name: sym, isOptional: false),
                binding, null));
            existingArgs.Add(sym);
            lastType = argType;
          }
        } else if (current is ConsType) {
          int len;
          try {
          } catch (ExceptionWrapper ex) {
            throw ThrowSyntaxError(ex, "default-valued argument must be a proper list");
          }
          if (len != 2) {
            throw ThrowSyntaxError("default-valued argument must be a list of length 2");
          }
          var arg = ListOperations.GetCar(current);
          if (!(arg is SymbolType)) {
            throw ThrowSyntaxError(
              "argument must be a symbol, got object of type {0}", arg.__class__);
          }
          var sym = (SymbolType)arg;
          if (sym.IsSelfEvaluating) {
            throw ThrowSyntaxError(
              "cannot use self-evaluating symbol '{0} as argument", sym);
          }

          if (existingArgs.Contains(sym)) {
            throw ThrowSyntaxError(
              "argument list contains argument '{0} more than once", sym)
          }
          var val = ListOperations.GetCar(ListOperations.GetCdr(current));

          var argType = GetOptionalArgumentType(lastType);

          var @default = compiler.ToAst(val, outerContext);
          var binding = innerContext.AddBinding(sym);
          args.Add(
            Tuple.Create(
              new ArgumentProperties(argType, name: sym, isOptional: true),
              binding, @default));
          existingArgs.Add(sym);
          lastType = argType;
          lastOptional = true;
        }
      }
    }

    /// <summary>
    /// Ensures that it is legal for an optional argument to follow the given previous
    /// argument, and returns the type that such an optional argument should be.
    /// </summary>
    protected ArgumentType GetOptionalArgumentType(ArgumentType lastType) {
      switch (lastType) {
        case ArgumentType.PositionalOrKeyword:
          return ArgumentType.PositionalOrKeyword;
        case ArgumentType.RestCapture:
        case ArgumentType.RestIgnore:
        case ArgumentType.RestBlock:
        case ArgumentType.Keyword:
          return ArgumentType.Keyword;
        case ArgumentType.RestKwCapture:
        case ArgumentType.RestKwIgnore:
          throw ThrowSyntaxError("found optional argument after rest-keyword argument");
        default:
          throw InvalidOperationException("this should be impossible.");
      }
    }
  }
}
