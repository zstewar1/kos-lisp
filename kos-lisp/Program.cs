using NDesk.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using ZStewart.KOSLisp.Compile;
using ZStewart.KOSLisp.Compile.AST;
using ZStewart.KOSLisp.Compile.Contexts;
using ZStewart.KOSLisp.Compile.Generators.CSharp;
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

      var path = new List<string>();
      path.Add(Path.Combine(
        Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
        "lib"));

      var kosLispPath = Environment.GetEnvironmentVariable("KOS_LISP_PATH");
      if (kosLispPath != null) {
        path.AddRange(kosLispPath.Split(':').Where(s => !string.IsNullOrEmpty(s)));
      }

      path.Add(".");

      if (extra.Count > 1) {
        Console.Error.WriteLine(
          "Unexpected extra argument{0}: {1}",
          extra.Count == 2 ? "" : "s",
          String.Join(" ", extra.GetRange(1, extra.Count - 1)));
      } else if (extra.Count == 1) {
        RunFile(extra[0], path, printAst, printExpr, alwaysPrint);
      } else {
        RunInteractive(path, printAst, printExpr, alwaysPrint);
      }
    }

    private static void RunFile(
        string filename, IEnumerable<string> path,
        bool printAst, bool printExpr, bool alwaysPrint) {
      var eval = new CSharpEvaluator(path);

      if (printAst) {
        eval.OnSemantics += (ast) => Console.WriteLine(ast);
      }
      if (printExpr) {
        eval.OnParse += (expr) => Console.WriteLine(StringType.GetReprString(expr));
      }
      if (alwaysPrint) {
        eval.OnEvaluate += (res) => Console.WriteLine(StringType.GetReprString(res));
      }

      using (var file = File.OpenText(filename)) {
        var source = new TextReaderSource(filename, file);

        var module = eval.GetFreshModule("--main--", "--main--");

        eval.Evaluate(module, source);
      }
    }

    private static void RunInteractive(
        IEnumerable<string> path,
        bool printAst, bool printExpr, bool alwaysPrint) {
      // TODO(zstewar1): handle printing.
      var eval = new CSharpEvaluator(path);

      if (printAst) {
        eval.OnSemantics += (ast) => Console.WriteLine(ast);
      }
      if (printExpr) {
        eval.OnParse += (expr) => Console.WriteLine(StringType.GetReprString(expr));
      }
      if (alwaysPrint) {
        eval.OnEvaluate += (res) => Console.WriteLine(StringType.GetReprString(res));
      }

      var source = new GetlineSource("koslisp");
      var nextPrompt = "=> ";
      source.OnBeforeReadLine += () => {
        source.Prompt = nextPrompt;
        nextPrompt = ".. ";
      };

      var module = eval.GetFreshModule("--main--", "--main--");

      IEnumerator<LispObject> parseStream;
      Context context;
      eval.StartParse(module, source, out parseStream, out context);

      for (;;) {
        LispObject result;
        try {
          if (eval.Evaluate1(parseStream, context, out result)) {
            if (!ReferenceEquals(result, NilType.Nil)) {
              Console.WriteLine(StringType.GetReprString(result));
            }
            // Reset prompt for next time a line is read.
          } else {
            break;
          }
        } catch (ExceptionWrapper ex) {
          Console.Error.WriteLine(
            "{0}: {1}", ex.LispException.__class__.__name__, ex.LispException.Message);
          // Reset parsing when there is an error (this clears the current line)
          eval.StartParse(module, source, out parseStream, out context);
        } finally {
          // This resets the prompt for the next read. Unnecessary on a break, but we do
          // it in finally so it happens whenter we successfully read or have an error.
          nextPrompt = "=> ";
        }
      }
    }
  }
}
