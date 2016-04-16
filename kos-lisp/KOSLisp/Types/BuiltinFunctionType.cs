using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using ZStewart.KOSLisp.Interpreter;
using ZStewart.KOSLisp.Types.Attributes;
using ZStewart.KOSLisp.Types.Helpers;

namespace ZStewart.KOSLisp.Types {
  public class BuiltinFunctionType : LispObject {
    #region Static Type Setup
    private static LispTypeObject _builtinFunction;
    public static LispTypeObject BuiltinFunction {
      get {
        if (_builtinFunction != null) return _builtinFunction;

        _builtinFunction = new LispTypeObject {
          __name__ = "BuiltinFunction",
          // TODO(zstewar1): New/Call
        };
        _builtinFunction.__class__ = TypeType.Type;
        _builtinFunction.__bases__ = IConsType.ToLispTuple(ObjectType.Object);
        _builtinFunction.__mro__ = IConsType.ToLispTuple(
          _builtinFunction, ObjectType.Object);
        _builtinFunction = LispTypeObject.ConfigureType(_builtinFunction);
        if (_builtinFunction == null) throw new InvalidOperationException();
        return _builtinFunction;
      }
    }

    private static LispObject Call (LispObject self, LispObject args) {
      if (self is BuiltinFunctionType) {
        return ((BuiltinFunctionType)self).Call(args);
      } else {
        LispInterpreter.SetException(ExceptionType.CreateTypeError(
          "First argument must be a BuiltinFunction"));
        return null;
      }
    }
    #endregion Static Type Setup

    #region Static Helpers
    /// <summary>
    /// Create a bound method for the specified methodinfo.
    /// </summary>
    /// <param name="boundMethod">
    /// The method to be called by this builtin function. This must be static.
    /// </param>
    /// <returns>
    /// A BuiltinFunction which calls the specified function.
    /// </returns>
    public BuiltinFunctionType Create(MethodInfo boundMethod) {
      return new BuiltinFunctionType(boundMethod) {
        __class__ = BuiltinFunction,
      };
    }
    #endregion Static Helpers

    protected BuiltinFunctionType(MethodInfo boundMethod) {
      if (!(boundMethod.IsStatic))
        throw new ArgumentException("boundMethod must be static");
      if (!(typeof(LispObject).IsAssignableFrom(boundMethod.ReturnType)))
        throw new ArgumentException("boundMethod must return a subclass of LispObject");
      this.boundMethod = boundMethod;

      paraminfos = ImmutableArray.CreateRange(boundMethod.GetParameters());
      int index = 0;
      // Read the paraminfos until expended. First postional arguments, then Rest 
      // arguments, followed by named keywords, followed by kwargs.

      var pargs = ImmutableList.CreateBuilder<Type>();
      for (; index < paraminfos.Length; index++) {
        var p = paraminfos[index];
        if (p.GetCustomAttribute<PositionalArgument>() != null) {
          pargs.Add(p.ParameterType);
        } else {
          // Next argument is unannotated (as positional).
          // Decrement so that the next operation gets the same argument.
          index--;
          break;
        }
      }
      positionalArguments = pargs.ToImmutable();

      if (index < paraminfos.Length) {
        var p = paraminfos[index];
        if (p.GetCustomAttribute<RestArgument>() != null) {
          index++;
          if (!p.ParameterType.IsAssignableFrom(typeof(IList<LispObject>)))
            throw new ArgumentException(
              "RestArgument must accept a list of lisp objects.");
          else rest = true;
        }
      }

      var kwargs = ImmutableList.CreateBuilder<Tuple<string, Type>>();
      for (; index < paraminfos.Length; index++) {
        var p = paraminfos[index];
        var pattr = p.GetCustomAttribute<KeywordArgument>();
        if (p.GetCustomAttribute<KeywordArgument>() != null) {
          // Coalesce name from the argument name on the attribute and the name of the
          // parameter.
          var name = pattr.ArgumentName ?? p.Name;
          if (kwargs.Select(t => t.Item1).Contains(name, SymbolType.SymbolComparer))
            throw new ArgumentException(
              "Found duplicate keyword argument {0}", name);
          kwargs.Add(Tuple.Create(name, p.ParameterType));
        } else {
          index--;
          break;
        }
      }
      keywordArguments = kwargs.ToImmutable();

      if (index < paraminfos.Length) {
        var p = paraminfos[index];
        if (p.GetCustomAttribute<RestKeywordArgument>() != null) {
          index++;
          if (!p.ParameterType.IsAssignableFrom(typeof(IDictionary<string, LispObject>)))
            throw new ArgumentException(
              "RestKeywordArgument must accept a dictionary of string->lisp object.");
          else restKwargs = true;
        }
      }

      if (index < paraminfos.Length)
        throw new ArgumentException(
          "boundMethod had unannotated or illegally anotated parameters.");
    }

    private readonly MethodInfo boundMethod;

    private readonly ImmutableArray<ParameterInfo> paraminfos;
    private readonly ImmutableList<Type> positionalArguments;
    private readonly bool rest;
    private readonly ImmutableList<Tuple<string, Type>> keywordArguments;
    private readonly bool restKwargs;


    private LispObject Call (LispObject args) {
      List<LispObject> pargs;
      Dictionary<string, LispObject> kwargs;
      Arguments.GetArguments(args, out pargs, out kwargs);
      if (pargs == null || kwargs == null) return null;

      if (pargs.Count < positionalArguments.Count) {
        LispInterpreter.SetException(ExceptionType.CreateTypeError(
          "not enough positional arguments, expected {0}, got {1}",
          positionalArguments.Count, pargs.Count));
        return null;
      }

      if (pargs.Count > positionalArguments.Count && !rest) {
        LispInterpreter.SetException(ExceptionType.CreateTypeError(
          "too many positional arguments, expected {0}, got {1}",
          positionalArguments.Count, pargs.Count));
      }

      if (!restKwargs) {
        // Later we may want to optimize the lookup of known keywords -- if necessary.
        var extraKwargs = kwargs.Keys.Where(k => !keywordArguments.Select(
          t => t.Item1).Contains(k, SymbolType.SymbolComparer)).ToList();
        if (extraKwargs.Count > 0) {
          LispInterpreter.SetException(ExceptionType.CreateTypeError(
            "got unexpected keyword arguments: ({0})", string.Join(" ", extraKwargs)));
          return null;
        }
      }

      object[] arguments = new object[paraminfos.Length];
      int index = 0;

      for (int i = 0; i < positionalArguments.Count; i++) {
        object marshaled;
        if(!Arguments.Marshal(positionalArguments[i], pargs[i], out marshaled))
          return null;
        arguments[index++] = marshaled;
      }

      if (rest) {
        // If there is a rest parameter is may be empty but never null.
        pargs.RemoveRange(0, positionalArguments.Count);
        arguments[index++] = pargs;
      }

      for (int i = 0; i < keywordArguments.Count; i++, index++) {
        LispObject arg;
        if (kwargs.TryGetValue(keywordArguments[i].Item1, out arg)) {
          object marshaled;
          if (!Arguments.Marshal(keywordArguments[i].Item2, arg, out marshaled))
            return null;
          arguments[index] = marshaled;
          kwargs.Remove(keywordArguments[i].Item1);
        }
      }

      if (restKwargs) {
        arguments[index++] = kwargs;
      }

      if (index < arguments.Length) {
        throw new InvalidOperationException(
          "After filling all arguments, index was still not at the end of the argument " +
          "list, which should be impossible.");
      }

      return (LispObject)boundMethod.Invoke(null, arguments);
    }
  }
}
