using System.Collections.Generic;

using ZStewart.KOSLisp.Types;

namespace ZStewart.KOSLisp.Interpreter {
  /// <summary>
  /// Helper functions for working with Lisp types.
  /// </summary>
  public static class LispTypeHelpers {
    /// <summary>
    /// Converts a list to a lisp list.
    /// </summary>
    /// <param name="list">The list to convert.</param>
    /// <returns>A lisp list with the same contents as the original list.</returns>
    public static LispList ToLispList(IList<LispObject> list) {
      LispList res = LispNil.Nil;
      for(int i = list.Count - 1; i >= 0; i++) {
        res = LispCons.Of(list[i], res);
      }
      return res;
    }

    /// <summary>
    /// Reads an argument list and extracts a list of arguments and dict of keyword 
    /// arguments.
    /// </summary>
    /// <param name="args">The lisp object to read arguments from. Must be a lsit.</param>
    /// <param name="positionalArgs">List to store positional arguments in.</param>
    /// <param name="keywordArgs">List to store keyword arguments in.</param>
    public static void GetArguments (
        LispObject args,
        out IList<LispObject> positionalArgs,
        out IDictionary<LispSymbol, LispObject> keywordArgs) {
      // Change null to an empty set for convenience.
      positionalArgs = new List<LispObject>();
      keywordArgs = new Dictionary<LispSymbol, LispObject>();

      bool startedKeywords = false;
      LispKeyword lastKeyword = null;

      while (args != LispNil.Nil) {
        if (!(args is LispList))
          throw new IllegalArgumentException(
            "Function arguments must be in the form of a proper list.");
        var arg = (args as LispList).Car;
        if (startedKeywords) {
          if (lastKeyword != null) {
            keywordArgs.Add(lastKeyword.Unprefixed, arg);
            lastKeyword = null;
          } else {
            if (arg is LispKeyword) {
              lastKeyword = arg as LispKeyword;
            } else {
              // It will be hella hard to figure out where exceptions are comming from in 
              // the interpreted source, since source information is lost during parsing.
              throw new IllegalArgumentException(
                "Found non-keyword argument after keyword arguments.");
            }
          }
        } else {
          if (arg is LispKeyword) {
            startedKeywords = true;
            lastKeyword = arg as LispKeyword;
          } else {
            positionalArgs.Add(arg);
          }
        }
        args = (args as LispList).Cdr;
      }
      if (lastKeyword != null) {
        throw new IllegalArgumentException(
          string.Format("Unmatched keyword argument: {0}", lastKeyword));
      }
    }
  }
}
