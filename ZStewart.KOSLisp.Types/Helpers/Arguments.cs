using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZStewart.KOSLisp.Types.Helpers {

  public static class Arguments {

    /// <summary>
    /// Split the given argument list and keyword arguments into rest arguments and
    /// keyword arguments, given the expected number of positional arguments and the known
    /// keyword argument names, and how to handle extra positional and keyword arguments.
    /// </summary>
    public static void SplitArguments(
        IList<LispObject> pargs,
        IDictionary<SymbolType, LispObject> kwargs,
        int numPositional,
        PositionalType hasRest,
        IList<SymbolType> namedKwargs,
        bool hasRestKwargs,
        out List<LispObject> pos,
        out List<LispObject> rest,
        out List<LispObject> knownKwargs,
        out Dictionary<SymbolType, LispObject> restKwargs) {

      if (pargs.Count < numPositional) {
        throw ExceptionType.ThrowTypeError(
          "expected {0} positional arguments, got {1}", numPositional, pargs.Count);
      }
      // Too many arguments if rest are non-capturing and non-ignored and the number of
      // passed positional arguments is longer than the positional or positional + keyword
      // lists.
      if ((hasRest == PositionalType.KeywordOverrun
            && numPositional + namedKwargs.Count < pargs.Count)
          || (hasRest == PositionalType.RestIllegal
            && numPositional < pargs.Count)) {

      }

      int argIndex = 0;
      pos = new List<LispObject>(numPositional);
      for (; argIndex < numPositional; argIndex++) {
        pos.Add(pargs[argIndex]);
      }

      if (hasRest == PositionalType.RestCapture) {
        rest = new List<LispObject>(pargs.Count - argIndex);
        for (; argIndex < pargs.Count; argIndex++) {
          rest.Add(pargs[argIndex]);
        }
      } else {
        rest = null;
        if (hasRest == PositionalType.RestIgnore) {
          argIndex = pargs.Count;
        }
      }

      knownKwargs = new List<LispObject>(namedKwargs.Count);
      // If overrun is disabled, rest check will have moved the index, if extra is illeal,
      // early check will have caught us.
      for (; argIndex < pargs.Count; argIndex++) {
        if (kwargs.ContainsKey(namedKwargs[knownKwargs.Count])) {
          throw ExceptionType.ThrowTypeError(
            "got duplicated keyword argument {0}", namedKwargs[knownKwargs.Count]);
        }
        knownKwargs.Add(pargs[argIndex]);
      }

      // don't modify argument.
      var kw = new Dictionary<SymbolType, LispObject>(kwargs);
      // Continue filling kwargs from the dictionary argument.
      for (int i = knownKwargs.Count; i < namedKwargs.Count; i++) {
        LispObject nextKwarg;
        if (kwargs.TryGetValue(namedKwargs[i], out nextKwarg)) {
          kw.Remove(namedKwargs[i]);
          knownKwargs.Add(nextKwarg);
        } else {
          knownKwargs.Add(null);
        }
      }

      if (!hasRestKwargs) {
        if(kwargs.Count > 0) {
          // TODO(zstewar1): be explicit.
          throw ExceptionType.ThrowTypeError("got unexpected keyword arguments");
        } else {
          restKwargs = null;
        }
      } else {
        // assign what's left of the duplicated kwargs dict.
        restKwargs = kw;
      }
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
        // Allow unmarshaling void return types (as Nil)
        || sourceType == typeof(void)
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
