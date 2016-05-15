using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZStewart.KOSLisp.Types.Helpers {

  public static class Arguments {

    /// <summary>
    /// Collects all of the keyword and positional arguments into a linear list. Creates
    /// errors for certain types of illegal argument lists, such as keywords duplicating
    /// positional arguments. Assumes that the passed ArgumentProperties are valid, i.e.
    /// that they have a valid ordering of keyword and positional arguments.
    /// </summary>
    /// <param name="pargs">
    /// The positional arguments from the interpreter.
    /// </param>
    /// <param name="kwargs">
    /// The keyword arguments from the interpreter.
    /// </param>
    /// <param name="arguments">
    /// ArgumentProperties of the arguments which should be collected. Assumed to be
    /// ordered in a valid way.
    /// PositionalOrKeyword(required)*,
    /// PositionalOrKeyword(optional)*,
    /// Rest[Capture,Ignore,Block]?,
    /// Keyword(optional,required)*,
    /// RestKw[Capture,Ignore]?
    /// </param>
    /// <returns>
    /// An array of collected arguments with the following properties:
    /// - The value of any required positional/keyword argument is a non-null LispObject.
    /// - Optional positional/keyword arguments are LispObjects or null if no value was
    ///   provided
    /// - Rest[Keyword][Ignore,Block] arguments are always null.
    /// - RestCapture arguments are always a non-null List(LispObject), but may be empty.
    /// - RestKwCapture arguments are always a non-null
    ///   Dictionary(SymbolType, LispObject) but may be empty.
    /// </returns>
    public static object[] CollectArguments(
        IList<LispObject> pargs,
        IDictionary<SymbolType, LispObject> kwargs,
        IList<ArgumentProperties> arguments) {
      var result = new object[arguments.Count];
      // Dictionary which will hold the keyword arguments so that we can remove them as we
      // take assignments from them. This allows us to track which ones are left at the
      // end to either set them to teh RestKwCapture/Ignore or raise an error.
      Dictionary<SymbolType, LispObject> restKwargs = null;
      for (int i = 0; i < result.Length; i++) {
        var arg = arguments[i];
        switch (arg.Type) {
          case ArgumentType.PositionalOrKeyword:
            {
              // Assume arguments is valid, so we must not have hit Rest yet.
              if (i < pargs.Count) {
                // Just grab the appropriate argument.
                if (kwargs.ContainsKey(arg.Name)) {
                  throw ExceptionType.ThrowTypeError(
                    "got multiple values for argument '{0}", arg.Name);
                }
                result[i] = pargs[i];
              } else {
                // Setup restKwargs if it hasn't been created yet, since now we know we will
                // need to do keyword arguments.
                restKwargs = restKwargs ?? new Dictionary<SymbolType, LispObject>(kwargs);
                // Out of positional arguments, try to assign from keywords.
                LispObject value;
                if (restKwargs.TryGetValue(arg.Name, out value)) {
                  result[i] = value;
                  restKwargs.Remove(arg.Name);
                } else if (!arg.IsOptional) {
                  throw ExceptionType.ThrowTypeError(
                    "missing value for required positional argument '{0}", arg.Name);
                }
              }
            }
            break;
          case ArgumentType.RestCapture:
            {
              // Arguments after this will all be Keyword or RestKw*, so we don't have
              // to worry about doing anything with pargs (like clearing it).
              var rest = new List<LispObject>(pargs.Count - i);
              for (int j = i; j < pargs.Count; j++) {
                rest.Add(pargs[j]);
              }
              result[i] = rest;
            }
            break;
          case ArgumentType.RestIgnore:
            {
              // ignore just puts in a null in that slot.
              result[i] = null;
            }
            break;
          case ArgumentType.RestBlock:
            {
              // block checks and errors if there are kwargs left.
              if (i < pargs.Count) {
                throw ExceptionType.ThrowTypeError(
                  "expected at most {0} positional arguments, but {1} were given",
                  i, pargs.Count);
              }
              result[i] = null;
            }
            break;
          case ArgumentType.Keyword:
            {
              restKwargs = restKwargs ?? new Dictionary<SymbolType, LispObject>(kwargs);
              LispObject value;
              if (restKwargs.TryGetValue(arg.Name, out value)) {
                result[i] = value;
                restKwargs.Remove(arg.Name);
              } else if (!arg.IsOptional) {
                throw ExceptionType.ThrowTypeError(
                  "missing value for required keyword argument '{0}", arg.Name);
              }
            }
            break;
          case ArgumentType.RestKwCapture:
            {
              restKwargs = restKwargs ?? new Dictionary<SymbolType, LispObject>(kwargs);
              result[i] = restKwargs;
              restKwargs = null;
            }
            break;
          case ArgumentType.RestKwIgnore:
            {
              // Ensure that restKwargs output is null.
              restKwargs = null;
              result[i] = null;
            }
            break;
          default:
            throw new InvalidOperationException("This should be impossible.");
        }
      }
      if (restKwargs != null && restKwargs.Count > 0) {
        throw ExceptionType.ThrowTypeError("got unexpected additional keyword arguments");
      }
      return result;
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
      if (ReferenceEquals(source, NilType.Nil)
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
        if (source is StringType) {
          return ((StringType)source).Value;
        }
      }

      if (destType == typeof(bool)) {
        if (source is BoolType) {
          return ((BoolType)source).Value;
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
    /// Checks if the given destination type can be converted to from a lisp object *in
    /// general*.
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
    /// Checks if the given destination type can be convrted to from a lisp object *in
    /// general*.
    ///
    /// Even if IsMarshalable returns true, Marshal may still fail for particular
    /// combinations of types and values.
    /// </summary>
    public static bool IsMarshalable<T>() {
      return IsMarshalable(typeof(T));
    }

    /// <summary>
    /// Checks if unmarshal can convert from the given type to a lisp object in general.
    /// </summary>
    public static bool IsUnmarshalable(Type sourceType) {
      return sourceType == typeof(double)
        || sourceType == typeof(bool)
        || sourceType == typeof(string)
        || typeof(LispObject).IsAssignableFrom(sourceType)
        || typeof(IEnumerable<LispObject>).IsAssignableFrom(sourceType)
        || typeof(IDictionary<LispObject, LispObject>).IsAssignableFrom(sourceType)
        || typeof(IDictionary<SymbolType, LispObject>).IsAssignableFrom(sourceType);
    }

    /// <summary>
    /// Checks if unmarshal can convert from the given type to a lisp object in general.
    /// </summary>
    public static bool IsUnmarshalable<T>() {
      return IsUnmarshalable(typeof(T));
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
        } else if (typeof(LispObject).IsAssignableFrom(type)) {
          return (LispObject)value;
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
