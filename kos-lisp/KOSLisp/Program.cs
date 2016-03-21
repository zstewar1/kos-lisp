using NDesk.Options;
using System;
using System.Linq;
using System.Reflection;

using ZStewart.Compilers;

namespace ZStewart.KOSLisp {

  enum ExitCode {
    SUCCESS = 0,
    UNRECOGNIZED_OPTION = 1,
    NO_INPUT = 2,
  }

  class MainClass {
    public static void Main (string[] args) {
      bool version = false;
      bool help = false;

      string outputFile = null;

      var optset = new OptionSet {
        { "version", "Print the version and exit", v => version = v != null },
        { "h|help", "Print this help information and exit", h => help = h != null },
        { "o|outfile=", "Where to save the output file.", o => outputFile = o },
      };
      var extra = optset.Parse(args);
      if (version) {
        var title = ((AssemblyTitleAttribute)Attribute.GetCustomAttribute(
                      Assembly.GetEntryAssembly(), typeof(AssemblyTitleAttribute), false))
          .Title;
        var ver = Assembly.GetEntryAssembly().GetName().Version;
        Console.WriteLine("{0} {1}", title, ver);
        Environment.Exit((int)ExitCode.SUCCESS);
      }
      if (help) {
        Console.WriteLine(
          "Usage: {0} [options] files...", System.AppDomain.CurrentDomain.FriendlyName);
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
      if (extra.Count == 0) {
        Console.WriteLine("No input files. Exiting.");
        Environment.Exit((int)ExitCode.NO_INPUT);
      }

      Compiler compiler = new LispCompiler();
      if (outputFile != null) {
        compiler.Compile(extra, outputFile);
      } else {
        compiler.Compile(extra);
      }
    }
  }
}
