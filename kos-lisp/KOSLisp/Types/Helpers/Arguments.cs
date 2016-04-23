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
        out Dictionary<SymbolType, LispObject> keywordArgs) {
      // Change null to an empty set for convenience.
      var pargs = new List<LispObject>();
      var kwargs = new Dictionary<SymbolType, LispObject>();

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
            // TODO(zstewar1): Change when actual keywords exist.
            var keyword = (SymbolType)lastKeyword;
            if (kwargs.ContainsKey(keyword)) {
              LispInterpreter.SetException(ExceptionType.CreateTypeError(string.Format(
                "got multiple values for keyword argument {0}", lastKeyword)));
              // TODO(zstewar1): Find the current function name somehow, to insert it in
              // the error message. Maybe read it from the call stack, once we have that.
              return;
            }
            kwargs.Add(keyword, arg);
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
      Dictionary<SymbolType, LispObject> kwargs;
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
      Dictionary<SymbolType, LispObject> kwargs;
      GetArguments(args, out pargs, out kwargs);
      if (pargs == null || kwargs == null) return;

      var convertedPargs = IConsType.ToLispTuple(pargs);
      if (convertedPargs == null) return;
      // TODO(zstewar1): Convert kwargs, check its value.
      positionalArgs = convertedPargs;
      keywordArgs = null;
    }

    /// <summary>
    /// Converts the given lisp object to the destination type.
    /// </summary>
    /// <param name="destType">The type of object to convert to.</param>
    /// <param name="source">The lisp object to convert.</param>
    /// <param name="marshaled">Output parameter for the marshaled object.</param>
    /// <returns>True if the conversion succeeded, false if an error was set.</returns>
    public static bool Marshal(Type destType, LispObject source, out object marshaled) {
      // Marshal any parameter which is a plain object or lisp object (or of a type
      // appropriate to recieve such) to the raw lisp object.
      if (destType.IsAssignableFrom(source.GetType())) {
        marshaled = source;
        return true;
      }
      // Marshal any nullable which received Nil as null (unless it was captured as a
      // lisp object, in which case it would be captured as itself.
      if (destType.IsClass || destType.IsInterface
          || (destType.IsGenericType
              && destType.GetGenericTypeDefinition() == typeof(Nullable<>))
          && source == NilType.Nil) {
        marshaled = null;
        return true;
      }

      // Explicitly check convertable types.
      if (destType == typeof(double)) {
        // TODO(zstewar1): Call number on the type to convert it.
      }

      // TODO(zstewar1): etc. for string and any other type which are reasonable to
      // convert.

      marshaled = null;
      LispInterpreter.SetException(ExceptionType.CreateTypeError(
        "Cannot marshal {0} (type {1}) as C# type {2}", source, source.__class__,
        destType));
      return false;
    }

    /// <summary>
    /// Converts the given lisp object to the destination type.
    /// </summary>
    /// <typeparam name="T">The type of object to convert to.</typeparam>
    /// <param name="source">The lisp object to convert.</param>
    /// <param name="marshaled">Output parameter for the marshaled object.</param>
    /// <returns>True if the conversion succeeded, false if an error was set.</returns>
    public static bool Marshal<T>(LispObject source, out T marshaled) {
      object m;
      if (!Marshal(typeof(T), source, out m)) {
        marshaled = default(T);
        return false;
      } else {
        marshaled = (T)m;
        return true;
      }
    }
  }
}
