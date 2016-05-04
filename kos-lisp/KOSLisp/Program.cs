using NDesk.Options;
using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using ZStewart.KOSLisp.Interpreter;
using ZStewart.KOSLisp.Compile;
using ZStewart.KOSLisp.Compile.AST;
using ZStewart.KOSLisp.Compile.Contexts;
using ZStewart.KOSLisp.Compile.Generators.CSharp;
using ZStewart.KOSLisp.Types;
using ZStewart.KOSLisp.Parser;

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

      //string outputFile = null;

      var optset = new OptionSet {
        { "version", "Print the version and exit", v => version = v != null },
        { "h|help", "Print this help information and exit", h => help = h != null },
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
          TEMPRun(extra[0], file);
        }
      } else {
        TEMPRun("<stdin>", Console.In, true);
      }
    }

    private static void TEMPRun(
        string name, TextReader reader, bool interactive = false) {
      LispLexer lexer = LispLexer.Lex(name, reader);
      LispParser parser = new LispParser(lexer);

      var mainModule = ModuleType.Create(
        SymbolType.Create(name), LispInterpreter.Builtins);
      var context = new GlobalContext(mainModule);
      var compiler = new DefaultCompiler();
      var generatorFactory = new CSharpGeneratorFactory();
      for(;;) {
        try {
          var parsed = parser.Parse();
          if (parsed == null) break;
          var ast = compiler.ToAst(parsed, context);
          Console.WriteLine(ast);
          var generator = generatorFactory.Create(ast);
          var expression = generator.Emit();
          var func = Expression.Lambda<Func<LispObject>>(expression).Compile();
          Console.WriteLine(func());
        } catch (ParserError e) {
          Console.WriteLine("Exception while Parsing:");
          Console.WriteLine(e);
          if (!interactive) break;
        } catch (ExceptionWrapper ex) {
          Console.WriteLine("Exception while executing:");
          Console.WriteLine(ex.LispException);
          if (!interactive) break;
        }
      }
    }
  }
}
