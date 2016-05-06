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

      bool startedKeywords = false;
      LispObject lastKeyword = null;

      while (args != NilType.Nil) {
        var arg = ListOperations.GetCar(args);
        if (startedKeywords) {
          if (lastKeyword != null) {
            // TODO(zstewar1): Change when actual keywords exist.
            var keyword = (SymbolType)lastKeyword;
            if (kwargs.ContainsKey(keyword)) {
              throw ExceptionType.ThrowTypeError(
                "got multiple values for keyword argument {0}", lastKeyword);
              // TODO(zstewar1): Find the current function name somehow, to insert it in
              // the error message. Maybe read it from the call stack, once we have that.
            }
            kwargs.Add(keyword, arg);
            lastKeyword = null;
          } else {
            // TODO(zstewar1): Change to the keyword symbol subclass once implemented.
            if (LispType.IsInstance(arg, SymbolType.Symbol)) {
              lastKeyword = arg;
            } else {
              // TODO(zstewar1): method name in exception.
              throw ExceptionType.ThrowTypeError(
                "positional argument follows keyword argument");
            }
          }
        } else {
          // TODO(zstewar1): Change to the keyword symbol subclass once implemented.
          if (false /*TypeType.IsInstance(arg, SymbolType.Symbol)*/) {
            //lastKeyword = arg;
            //startedKeywords = true;
          } else {
            pargs.Add(arg);
          }
        }
        args = ListOperations.GetCdr(args);
      }
      if (lastKeyword != null) {
        throw ExceptionType.ThrowTypeError("unmatched keyword argument {0}", lastKeyword);
        // TODO(zstewar1): Method name in exception.
      }
      positionalArgs = pargs;
      keywordArgs = kwargs;
    }

    public static List<LispObject> GetPositionalArguments(LispObject args) {
      List<LispObject> pargs;
      Dictionary<SymbolType, LispObject> kwargs;
      GetArguments(args, out pargs, out kwargs);
      if (kwargs.Count > 0) {
        // TODO(zstewar1): Set method name in exception, and add ability name the
        // unexpected argument values.
        throw ExceptionType.ThrowTypeError("unexpected keyword argument");
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
      List<LispObject> pargs;
      Dictionary<SymbolType, LispObject> kwargs;
      GetArguments(args, out pargs, out kwargs);

      positionalArgs = IConsType.ToLispTuple(pargs);
      keywordArgs = DictType.ToLispDict(kwargs);
    }

    /// <summary>
    /// Converts the given lisp object to the destination type.
    /// </summary>
    /// <param name="destType">The type of object to convert to.</param>
    /// <param name="source">The lisp object to convert.</param>
    /// <returns>The converted object.</returns>
    public static object Marshal(Type destType, LispObject source) {
      // Marshal any parameter which is a plain object or lisp object (or of a type
      // appropriate to recieve such) to the raw lisp object.
      if (destType.IsAssignableFrom(source.GetType())) {
        return source;
      }
      // Marshal any nullable which received Nil as null (unless it was captured as a
      // lisp object, in which case it would be captured as itself.
      if (source == NilType.Nil
          && !typeof(LispObject).IsAssignableFrom(destType)
          && (destType.IsClass || destType.IsInterface
              || (destType.IsGenericType
                  && destType.GetGenericTypeDefinition() == typeof(Nullable<>)))) {
        return null;
      }

      // Explicitly check convertable types.
      if (destType == typeof(double)) {
        // TODO(zstewar1): Call the Number type with the object as the argument.
        if (source is NumberType) {
          return ((NumberType)source).Value;
        }
      }

      if (destType == typeof(string)) {
        var str = LispObject.Call(source, "--str--", NilType.Nil);
        if (str is StringType) {
          return ((StringType)str).Value;
        }
      }

      if (destType == typeof(bool)) {
        var boolean = LispObject.Call(source, "--bool--", NilType.Nil);
        if (boolean is BoolType) {
          return ((BoolType)boolean).Value;
        }
      }

      if (destType.IsAssignableFrom(typeof(List<LispObject>))) {
        return ListOperations.IterList(source).ToList();
      }

      if (destType.IsAssignableFrom(typeof(Dictionary<LispObject, LispObject>))) {
        return ListOperations.IterList(LispObject.Call(source, "keys"))
          .ToDictionary(key => MappingOperations.GetItem(source, key));
      }

      if (destType.IsAssignableFrom(typeof(Dictionary<SymbolType, LispObject>))) {
        return ListOperations.IterList<SymbolType>(LispObject.Call(source, "keys"))
          .ToDictionary(key => MappingOperations.GetItem(source, key));
      }

      // TODO(zstewar1): etc. for string and any other type which are reasonable to
      // convert. (Maybe List?)

      throw ExceptionType.ThrowTypeError(
        "Cannot marshal {0} (type {1}) as C# type {2}", source, source.__class__,
        destType);
    }

    /// <summary>
    /// Converts the given lisp object to the destination type.
    /// </summary>
    /// <typeparam name="T">The type of object to convert to.</typeparam>
    /// <param name="source">The lisp object to convert.</param>
    /// <returns>The marshaled object.</returns>
    public static T Marshal<T>(LispObject source) {
      return (T)Marshal(typeof(T), source);
    }

    /// <summary>
    /// Checks if the given destination type can be marshaled to *in general*.
    ///
    /// Even if IsMarshalable returns true, Marshal may still fail for particular
    /// combinations of types and values.
    /// </summary>
    public static bool IsMarshalable(Type destType) {
      return destType == typeof(double)
        || destType == typeof(bool)
        || destType == typeof(string)
        || destType.IsAssignableFrom(typeof(List<LispObject>))
        || destType.IsAssignableFrom(typeof(Dictionary<LispObject, LispObject>))
        || destType.IsAssignableFrom(typeof(Dictionary<SymbolType, LispObject>))
        || typeof(LispObject).IsAssignableFrom(destType);
    }

    /// <summary>
    /// Checks if the given destination type can be marshaled to *in general*.
    ///
    /// Even if IsMarshalable returns true, Marshal may still fail for particular
    /// combinations of types and values.
    /// </summary>
    public static bool IsMarshalable<T>() {
      return IsMarshalable(typeof(T));
    }

    /// <summary>
    /// Unmarsharl the object. This inverts Marshal.
    ///
    /// For dictionary and list types, this accepts anything which is an
    /// IEnumerable(LispObject) for lists or IDictionary(LispObject, LispObject) or
    /// IDictionary(SymbolType, LispObject) for dictionaries.
    /// </summary>
    public static LispObject Unmarshal(object value) {
      if (value == null) {
        return NilType.Nil;
      } else {
        var type = value.GetType();
        if (type == typeof(double)) {
          return NumberType.Create((double)value);
        } else if (type == typeof(bool)) {
          return BoolType.Create((bool)value);
        } else if (type == typeof(string)) {
          return StringType.Create((string)value);
        } else if (typeof(IEnumerable<LispObject>).IsAssignableFrom(type)) {
          return ConsType.ToLispList(((IEnumerable<LispObject>)value).ToList());
        } else if (typeof(IDictionary<LispObject, LispObject>).IsAssignableFrom(type)) {
          return DictType.ToLispDict((IDictionary<LispObject, LispObject>)value);
        } else if (typeof(IDictionary<SymbolType, LispObject>).IsAssignableFrom(type)) {
          return DictType.ToLispDict((IDictionary<SymbolType, LispObject>)value);
        } else if (typeof(LispObject).IsAssignableFrom(type)) {
          return (LispObject)value;
        } else {
          throw ExceptionType.ThrowTypeError(
            "Cannot unmarshal C# type {0} as a lisp object", type);
        }
      }
    }
  }
}
