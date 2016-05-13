using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using ZStewart.KOSLisp.Types.Attributes;
using ZStewart.KOSLisp.Types.Helpers;

namespace ZStewart.KOSLisp.Types {
  /// <summary>
  /// A class that magically calls a C# (static) function with lisp arguments including
  /// type conversion and other magic.
  /// </summary>
  public class CallMagic {

    /// <summary>
    /// Function used by CallMagic to dispatch the wrapped function. It can assume that
    /// positionalArguments.Length + (restArguments != null ? 1 : 0) +
    /// keywordArguments.Length + (restKeywordArguments != null ? 1 : 0)
    /// is equal to the number of parameters required by the method it wraps.
    ///
    /// Some elements of keywordArguments may be null, meaning to use the default value.
    /// </summary>
    private delegate LispObject MagicFunction(
        object[] positionalArguments,
        List<LispObject> restArguments,
        object[] keywordArguments,
        Dictionary<SymbolType, LispObject> restKeywordArguments);

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
      if (!(Arguments.IsUnmarshalable(boundMethod.ReturnType)
            || boundMethod.ReturnType == typeof(void))) {
        throw new ArgumentException(
          "The return type of boundMethod must be unmarshalable or void.");
      }

      var paraminfos = boundMethod.GetParameters();
      int index = 0;
      // Read the paraminfos until expended. First postional arguments, then Rest
      // arguments, followed by named keywords, followed by kwargs.

      var pargs = ImmutableList.CreateBuilder<Type>();
      for (; index < paraminfos.Length; index++) {
        var p = paraminfos[index];
        if (p.GetCustomAttribute<PositionalArgumentAttribute>() != null) {
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
        if (p.GetCustomAttribute<RestArgumentAttribute>() != null) {
          index++;
          if (!p.ParameterType.IsAssignableFrom(typeof(List<LispObject>)))
            throw new ArgumentException(
              "RestArgument must accept a list of lisp objects.");
          else rest = true;
        }
      }

      var kwargsNames = ImmutableList.CreateBuilder<SymbolType>();
      var kwargsTypes = ImmutableList.CreateBuilder<Type>();
      for (; index < paraminfos.Length; index++) {
        var p = paraminfos[index];
        var pattr = p.GetCustomAttribute<KeywordArgumentAttribute>();
        if (pattr != null) {
          if (!Arguments.IsMarshalable(p.ParameterType)) {
            throw new ArgumentException("Keyword argument must be marshalable.");
          }
          // Coalesce name from the argument name on the attribute and the name of the
          // parameter.
          var name = SymbolType.Create(pattr.ArgumentName ?? p.Name);
          if (kwargsNames.Contains(name))
            throw new ArgumentException(string.Format(
              "Found duplicate keyword argument {0}", name));
          kwargsNames.Add(name);
          kwargsTypes.Add(p.ParameterType);
        } else {
          break;
        }
      }
      keywordArgumentNames = kwargsNames.ToImmutable();
      keywordArgumentTypes = kwargsTypes.ToImmutable();

      if (index < paraminfos.Length) {
        var p = paraminfos[index];
        if (p.GetCustomAttribute<RestKeywordArgumentAttribute>() != null) {
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

      // Generate the magic function which this call magic will use to make function
      // calls.
      var posParam = Expression.Parameter(typeof(object[]), "pos");
      var restParam = Expression.Parameter(typeof(List<LispObject>), "rest");
      var kwParam = Expression.Parameter(typeof(object[]), "kw");
      var restKwParam = Expression.Parameter(typeof(Dictionary<SymbolType, LispObject>), "restKw");

      var argumentExpressions = new List<Expression>();

      // Positional arguments are marshaled, we just need to cast.
      for (int i = 0; i < positionalArguments.Count; i++) {
        argumentExpressions.Add(
          Expression.Convert(
            Expression.ArrayAccess(posParam, Expression.Constant(i)),
            positionalArguments[i]));
      }

      if (rest) {
        argumentExpressions.Add(restParam);
      }

      // Again, just need to cast, already marshaled.
      for (int i = 0; i < keywordArgumentTypes.Count; i++) {
        argumentExpressions.Add(
          Expression.Convert(
            Expression.ArrayAccess(kwParam, Expression.Constant(i)),
            keywordArgumentTypes[i]));
      }

      if (restKwargs) {
        argumentExpressions.Add(restKwParam);
      }

      Expression innerCall = Expression.Call(boundMethod, argumentExpressions);

      Expression lambdaBody;
      if (boundMethod.ReturnType == typeof(void)) {
        lambdaBody = Expression.Block(
          typeof(LispObject),
          innerCall,
          Expression.Constant(NilType.Nil));
      } else {
        lambdaBody = Expression.Call(
          typeof(Arguments).GetMethod("Unmarshal", new Type[]{typeof(object)}),
          innerCall);
      }

      implementation = Expression.Lambda<MagicFunction>(
        lambdaBody,
        string.Format(
          "CallMagic magicFunction wrapping {0}.{1}",
          boundMethod.DeclaringType.Name, boundMethod.Name),
        ImmutableList.Create(posParam, restParam, kwParam, restKwParam)).Compile();
    }

    private readonly MagicFunction implementation;

    private readonly ImmutableList<Type> positionalArguments;
    private readonly bool rest;
    private readonly ImmutableList<SymbolType> keywordArgumentNames;
    private readonly ImmutableList<Type> keywordArgumentTypes;
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

      List<LispObject> pos, restList, kw;
      Dictionary<SymbolType, LispObject> restKwList;
      Arguments.SplitArguments(
          pargs, kwargs,
          positionalArguments.Count,
          rest ? PositionalType.RestCapture : PositionalType.RestIllegal,
          keywordArgumentNames,
          restKwargs,
          out pos, out restList, out kw, out restKwList);

      object[] marshaledPos = new object[pos.Count];
      for (int i = 0; i < pos.Count; i++) {
        marshaledPos[i] = Arguments.Marshal(positionalArguments[i], pos[i]);
      }

      object[] marshaledKw = new object[kw.Count];
      for (int i = 0; i < kw.Count; i++) {
        if (kw[i] != null) {
          marshaledKw[i] = Arguments.Marshal(keywordArgumentTypes[i], kw[i]);
        } else {
          marshaledKw[i] = Type.Missing;
        }
      }

      return implementation(marshaledPos, restList, marshaledKw, restKwList);
    }
  }
}
