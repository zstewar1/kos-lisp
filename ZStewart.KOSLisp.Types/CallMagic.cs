using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

using ZStewart.KOSLisp.Types.Attributes;
using ZStewart.KOSLisp.Types.Helpers;

namespace ZStewart.KOSLisp.Types {
  /// <summary>
  /// A class that magically calls a C# (static) function with lisp arguments including
  /// type conversion and other magic.
  /// </summary>
  public class CallMagic {

    private static CallMagic _nop;
    public static CallMagic NOP {
      get {
        if (_nop != null) return _nop;
        _nop = new CallMagic(typeof(CallMagic), "Nop");
        return _nop;
      }
    }

    private static void Nop (
        [RestArgument] List<LispObject> unusedPargs,
        [RestKeywordArgument] Dictionary<SymbolType, LispObject> unusedKwargs) {}

    #region Static Helper Methods
    /// <summary>
    /// Shortcut to look up a static, non-public method on the given type.
    /// </summary>
    public static MethodInfo FindMethod(Type type, string name) {
      return type.GetMethod(
          name,
          BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static |
          BindingFlags.FlattenHierarchy);
    }
    #endregion Static Helper Methods

    /// <summary>
    /// Shortcut constructor to do a lookup of a potentially-private static method on the
    /// given type.
    /// </summary>
    public CallMagic(Type type, string name) : this(FindMethod(type, name)) {}

    /// <summary>
    /// Construct a magic caller for the given static method, which automagically marshals
    /// arguments and andles rest/keyword arguments.
    /// </summary>
    public CallMagic(MethodInfo boundMethod) {
      if (!(boundMethod.IsStatic))
        throw new ArgumentException("boundMethod must be static");
      if (!(Arguments.IsUnmarshalable(boundMethod.ReturnType))) {
        throw new ArgumentException(
          "The return type of boundMethod must be unmarshalable.");
      }
      this.boundMethod = boundMethod;

      paraminfos = ImmutableArray.CreateRange(boundMethod.GetParameters());
      int index = 0;
      // Read the paraminfos until expended. First postional arguments, then Rest
      // arguments, followed by named keywords, followed by kwargs.

      var pargs = ImmutableList.CreateBuilder<Type>();
      for (; index < paraminfos.Length; index++) {
        var p = paraminfos[index];
        if (p.GetCustomAttribute<PositionalArgument>() != null) {
          if (!Arguments.IsMarshalable(p.ParameterType)) {
            throw new ArgumentException(
              "Positional argument must be marshalable.");
          }
          pargs.Add(p.ParameterType);
        } else {
          // Next argument is unannotated (as positional).
          break;
        }
      }
      positionalArguments = pargs.ToImmutable();

      if (index < paraminfos.Length) {
        var p = paraminfos[index];
        if (p.GetCustomAttribute<RestArgument>() != null) {
          index++;
          if (!p.ParameterType.IsAssignableFrom(typeof(List<LispObject>)))
            throw new ArgumentException(
              "RestArgument must accept a list of lisp objects.");
          else rest = true;
        }
      }

      var kwargs = ImmutableList.CreateBuilder<Tuple<SymbolType, Type>>();
      for (; index < paraminfos.Length; index++) {
        var p = paraminfos[index];
        var pattr = p.GetCustomAttribute<KeywordArgument>();
        if (p.GetCustomAttribute<KeywordArgument>() != null) {
          if (!Arguments.IsMarshalable(p.ParameterType)) {
            throw new ArgumentException(
              "Keyword argument must be marshalable.");
          }
          // Coalesce name from the argument name on the attribute and the name of the
          // parameter.
          var name = SymbolType.Create(pattr.ArgumentName ?? p.Name);
          if (kwargs.Select(t => t.Item1).Contains(name))
            throw new ArgumentException(string.Format(
              "Found duplicate keyword argument {0}", name));
          kwargs.Add(Tuple.Create(name, p.ParameterType));
        } else {
          break;
        }
      }
      keywordArguments = kwargs.ToImmutable();

      if (index < paraminfos.Length) {
        var p = paraminfos[index];
        if (p.GetCustomAttribute<RestKeywordArgument>() != null) {
          index++;
          if (!p.ParameterType.IsAssignableFrom(typeof(Dictionary<SymbolType, LispObject>)))
            throw new ArgumentException(
              "RestKeywordArgument must accept a dictionary of symbol->lisp object.");
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
    private readonly ImmutableList<Tuple<SymbolType, Type>> keywordArguments;
    private readonly bool restKwargs;

    /// <summary>
    /// Calls the bound method extracting its arguments from a lisp list.
    /// </summary>
    /// <param name="args">The lisp argument list to extract arguments from.</param>
    /// <returns>
    /// The lisp object that results from the call or null if an error occurs.
    /// </returns>
    public LispObject Call (
        List<LispObject> pargs,
        Dictionary<SymbolType, LispObject> kwargs) {
      if (pargs.Count < positionalArguments.Count) {
        throw ExceptionType.ThrowTypeError(
          "not enough positional arguments, expected {0}, got {1}",
          positionalArguments.Count, pargs.Count);
      }

      // TODO(zstewar1): flow over into the kwargs. (will require duplicate-kwarg
      // checking.
      if (pargs.Count > positionalArguments.Count && !rest) {
        throw ExceptionType.ThrowTypeError(
          "too many positional arguments, expected {0}, got {1}",
          positionalArguments.Count, pargs.Count);
      }

      if (!restKwargs) {
        // Later we may want to optimize the lookup of known keywords -- if necessary.
        var extraKwargs = kwargs.Keys.Where(
          k => !keywordArguments.Select(t => t.Item1).Contains(k)).ToList();
        if (extraKwargs.Count > 0) {
          throw ExceptionType.ThrowTypeError(
            "got unexpected keyword arguments: ({0})", string.Join(" ", extraKwargs));
        }
      }

      object[] arguments = new object[paraminfos.Length];
      int index = 0;

      for (int i = 0; i < positionalArguments.Count; i++, index++) {
        arguments[index] = Arguments.Marshal(positionalArguments[i], pargs[i]);
      }

      if (rest) {
        // If there is a rest parameter is may be empty but never null.
        pargs.RemoveRange(0, positionalArguments.Count);
        arguments[index++] = pargs;
      }

      for (int i = 0; i < keywordArguments.Count; i++, index++) {
        LispObject arg;
        if (kwargs.TryGetValue(keywordArguments[i].Item1, out arg)) {
          arguments[index] = Arguments.Marshal(keywordArguments[i].Item2, arg);
          kwargs.Remove(keywordArguments[i].Item1);
        } else {
          // Set other keyword arguments to missing to allow using C# default parameters.
          arguments[index] = Type.Missing;
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

      try {
        return Arguments.Unmarshal(boundMethod.Invoke(null, arguments));
      } catch (TargetInvocationException ex) {
        // TODO(zstewar1): This loses the stack grace from the inner method. There is no
        // way to keep it directly, so we would like to replace the use of reflection with
        // runtime function generation through Expressions.
        throw ex.InnerException;
      }
    }
  }
}
