using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ZStewart.KOSLisp.Interpreter;

namespace ZStewart.KOSLisp.Types.Helpers {
  public static class Arguments {

    /// <summary>
    /// Reads an argument list and extracts a list of arguments and dict of keyword
    /// arguments.
    /// </summary>
    /// <param name="args">The lisp object to read arguments from. Must be a lsit.</param>
    /// <param name="positionalArgs">
    /// A C# list which will contain the read out positional arguments.
    /// </param>
    /// <param name="keywordArgs">
    /// A C# Dictionary that will contain the positional arguments.
    /// </param>
    public static void GetArguments (
        LispObject args,
        out List<LispObject> positionalArgs,
        out Dictionary<LispObject, LispObject> keywordArgs) {
      // Change null to an empty set for convenience.
      var pargs = new List<LispObject>();
      var kwargs = new Dictionary<LispObject, LispObject>();

      // Setup "on error" values, so we can just return if there's an error.
      positionalArgs = null;
      keywordArgs = null;

      bool startedKeywords = false;
      LispObject lastKeyword = null;

      while (args != NilType.Nil) {
        var arg = ListOperations.GetCar(args);
        if (arg == null) return;
        if (startedKeywords) {
          if (lastKeyword != null) {
            var keywordSymbol = ObjectType.Call(
              lastKeyword, "to-unprefixed", NilType.Nil);
            if (keywordSymbol == null) return;
            if (kwargs.ContainsKey(keywordSymbol)) {
              LispInterpreter.SetException(ExceptionType.CreateTypeError(string.Format(
                "got multiple values for keyword argument {0}", lastKeyword)));
              // TODO(zstewar1): Find the current function name somehow, to insert it in
              // the error message. Maybe read it from the call stack, once we have that.
              return;
            }
            kwargs.Add(keywordSymbol, arg);
            lastKeyword = null;
          } else {
            // TODO(zstewar1): Change to the keyword symbol subclass once implemented.
            var kwdTypeCheck = TypeType.IsInstance(arg, SymbolType.Symbol);
            if (!kwdTypeCheck.HasValue) return;
            if (kwdTypeCheck.Value) {
              lastKeyword = arg;
            } else {
              // TODO(zstewar1): method name in exception.
              LispInterpreter.SetException(ExceptionType.CreateTypeError(string.Format(
                "positional argument follows keyword argument")));
              return;
            }
          }
        } else {
          // TODO(zstewar1): Change to the keyword symbol subclass once implemented.
         var kwdTypeCheck = TypeType.IsInstance(arg, SymbolType.Symbol);
          if (!kwdTypeCheck.HasValue) return;
          if (kwdTypeCheck.Value) {
            lastKeyword = arg;
            startedKeywords = true;
          } else {
            pargs.Add(arg);
          }
        }
        args = ListOperations.GetCdr(args);
        if (args == null) return;
      }
      if (lastKeyword != null) {
        LispInterpreter.SetException(ExceptionType.CreateTypeError(string.Format(
          "unmatched keyword argument {0}", lastKeyword)));
        // TODO(zstewar1): Method name in exception.
        return;
      }
      positionalArgs = pargs;
      keywordArgs = kwargs;
    }

    public static List<LispObject> GetPositionalArguments(LispObject args) {
      List<LispObject> pargs;
      Dictionary<LispObject, LispObject> kwargs;
      GetArguments(args, out pargs, out kwargs);
      if (pargs == null || kwargs == null) return null;
      if (kwargs.Count > 0) {
        // TODO(zstewar1): Set method name in exception, and add ability name the
        // unexpected argument values.
        LispInterpreter.SetException(ExceptionType.CreateTypeError(string.Format(
          "unexpected keyword argument")));
        return null;
      }
      return pargs;
    }

    /// <summary>
    /// Reads an argument list and extracts a list of arguments and dict of keyword
    /// arguments.
    /// </summary>
    /// <param name="args">The lisp object to read arguments from. Must be a lsit.</param>
    /// <param name="positionalArgs">
    /// A Lisp tuple which will contain the read out positional arguments.
    /// </param>
    /// <param name="keywordArgs">
    /// A Lisp dict that will contain the positional arguments.
    /// </param>
    public static void GetArguments (
        LispObject args,
        out LispObject positionalArgs,
        out LispObject keywordArgs) {
      // preset error values.
      positionalArgs = null;
      keywordArgs = null;

      List<LispObject> pargs;
      Dictionary<LispObject, LispObject> kwargs;
      GetArguments(args, out pargs, out kwargs);
      if (pargs == null || kwargs == null) return;

      var convertedPargs = IConsType.ToLispTuple(pargs);
      if (convertedPargs == null) return;
      // TODO(zstewar1): Convert kwargs, check its value.
      positionalArgs = convertedPargs;
      keywordArgs = null;
    }
  }
}
