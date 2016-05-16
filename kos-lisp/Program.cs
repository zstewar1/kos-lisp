using NDesk.Options;
using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using ZStewart.KOSLisp.Compile;
using ZStewart.KOSLisp.Compile.AST;
using ZStewart.KOSLisp.Compile.Contexts;
using ZStewart.KOSLisp.Compile.Generators.CSharp;
using ZStewart.KOSLisp.Modules.Builtins;
using ZStewart.KOSLisp.Parse;
using ZStewart.KOSLisp.Parse.Lisp;
using ZStewart.KOSLisp.Types;

namespace ZStewart.KOSLisp {

  enum ExitCode {
    SUCCESS = 0,
    UNRECOGNIZED_OPTION = 1,
    EXTRA_INPUT = 3,
  }

  class MainClass {
    public static void Main (string[] args) {
      bool version = false;
      bool help = false;
      bool printExpr = false;
      bool printAst = false;
      bool alwaysPrint = false;

      //string outputFile = null;

      var optset = new OptionSet {
        { "version", "Print the version and exit", v => version = v != null },
        { "h|help", "Print this help information and exit", h => help = h != null },
        { "ast", "Print the AST before evaluating", a => printAst = a != null },
        { "expr", "Print the Expression before evaluating", e => printExpr = e != null },
        {
          "print-all",
          "Print the evaluation result even if it is nil or the interpreter is not " +
            "running in interactive mode",
          p => alwaysPrint = p != null
        },
        //{ "o|outfile=", "Where to save the output file. Ignored if not compiling", o => outputFile = o },
      };
      var extra = optset.Parse(args);
      if (version) {
        var title = ((AssemblyTitleAttribute)Attribute.GetCustomAttribute(
                      Assembly.GetEntryAssembly(), typeof(AssemblyTitleAttribute), false))
          .Title;
        var ver = Assembly.GetEntryAssembly().GetName().Version;
        Console.Error.WriteLine("{0} {1}", title, ver);
        Environment.Exit((int)ExitCode.SUCCESS);
      }
      if (help) {
        Console.Error.WriteLine(
          "Usage: {0} [options] [file]", System.AppDomain.CurrentDomain.FriendlyName);
        optset.WriteOptionDescriptions(Console.Out);
        Environment.Exit((int)ExitCode.SUCCESS);
      }
      var unrecognized = extra.Where(arg => arg.StartsWith("-")).ToList();
      if (unrecognized.Count > 0) {
        Console.WriteLine(
          "Unrecognized option{0}: {1}", unrecognized.Count == 1 ? "" : "s",
          String.Join(" ", unrecognized));
        Environment.Exit((int)ExitCode.UNRECOGNIZED_OPTION);
      }
      if (extra.Count > 1) {
        Console.Error.WriteLine(
          "Unexpected extra argument{0}: {1}",
          extra.Count == 2 ? "" : "s",
          String.Join(" ", extra.GetRange(1, extra.Count - 1)));
      } else if (extra.Count == 1) {
        using (var file = File.OpenText(extra[0])) {
          TEMPRun(extra[0], file, printAst, printExpr, alwaysPrint);
        }
      } else {
        TEMPRun("<stdin>", Console.In, printAst, printExpr, alwaysPrint, true);
      }
    }

    private static void TEMPRun(
        string name, TextReader reader, bool printAst, bool printExpr, bool alwaysPrint,
        bool interactive = false) {
      bool isExpressionFirstLine = true;
      LispLexer lexer = new LispLexer(name, reader);
      if (interactive) {
        lexer.BeforeReadLine += () => {
          if (isExpressionFirstLine) {
            isExpressionFirstLine = false;
            Console.Error.Write("=> ");
          } else {
            Console.Error.Write(".. ");
          }
        };
      }

      LispParser parser = new LispParser(lexer);

      var mainModule = ModuleType.Create(
        SymbolType.Create(name), BuiltinsModule.Builtins);
      var context = new GlobalContext(mainModule);
      var semantizer = BaseSemanticAnalyzer.CreateDefaultAnalyzer();
      var generatorFactory = new CSharpGeneratorFactory();
      for(;;) {
        LispObject parsed;
        try {
          isExpressionFirstLine = true;
          parsed = parser.ParseNext();
          if (printExpr && parsed != null) {
            Console.Error.WriteLine(StringType.GetReprString(parsed));
          }
        } catch (ExceptionWrapper ex) {
          Console.Error.WriteLine("Exception while paring:");
          Console.Error.WriteLine(
            "{0}: {1}", ex.LispException.__class__.__name__, ex.LispException.ToString());
          if (interactive) {
            lexer.ClearLine();
            continue;
          } else {
            break;
          }
        }
        if (parsed == null) break;
        // The function that represents evaluating the expression.
        Func<LispObject> func;
        try {
          var ast = semantizer.ToAst(parsed, context);
          if (printAst) {
            Console.Error.WriteLine(ast);
          }
          var generator = generatorFactory.Create(ast);
          var expression = generator.Emit();
          func = Expression.Lambda<Func<LispObject>>(expression).Compile();
        } catch (ExceptionWrapper ex) {
          Console.Error.WriteLine("Exception while compiling:");
          Console.Error.WriteLine(
            "{0}: {1}", ex.LispException.__class__.__name__, ex.LispException.ToString());
          if (interactive) {
            continue;
          } else {
            break;
          }
        }
        try {
          var result = func();
          if ((interactive && result != NilType.Nil) || alwaysPrint) {
            Console.WriteLine(StringType.GetReprString(result));
          }
        } catch (ExceptionWrapper ex) {
          Console.Error.WriteLine("Exception while evaluating:");
          Console.Error.WriteLine(
            "{0}: {1}", ex.LispException.__class__.__name__, ex.LispException.ToString());
          if (interactive) {
            continue;
          } else {
            break;
          }
        }
      }
      Console.WriteLine();
    }
  }
}
