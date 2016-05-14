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
    /// arguments.Length is equal to the number of arguments taken by the wrapped
    /// function, and that they have been marshaled to the correct type and values.
    /// </summary>
    private delegate LispObject MagicFunction(object[] arguments);

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

    #region Internal Static Helpers
    private static ArgumentType GetRestArgumentType(RestArgumentAttribute attr) {
      if (attr.IsKeyword) {
        switch (attr.Type) {
          case RestArgumentType.Capture:
            return ArgumentType.RestKwCapture;
          case RestArgumentType.Ignore:
            return ArgumentType.RestKwIgnore;
          case RestArgumentType.Block:
            throw new ArgumentException(
              "to block extra keyword arguments, just don't include a capture " +
              "for them. There should be no IsKeyword + Block attribute...");
          default:
            throw new InvalidOperationException("This should be impossible.");
        }
      } else {
        switch (attr.Type) {
          case RestArgumentType.Capture:
            return ArgumentType.RestCapture;
          case RestArgumentType.Ignore:
            return ArgumentType.RestIgnore;
          case RestArgumentType.Block:
            return ArgumentType.RestBlock;
          default:
            throw new InvalidOperationException("This should be impossible.");
        }
      }
    }
    #endregion Internal Static Helpers
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
      if (!(boundMethod.IsStatic)) {
        throw new ArgumentException("boundMethod must be static");
      }
      if (!(Arguments.IsUnmarshalable(boundMethod.ReturnType)
            || boundMethod.ReturnType == typeof(void))) {
        throw new ArgumentException(
          "The return type of boundMethod must be unmarshalable or void.");
      }

      var paraminfos = boundMethod.GetParameters();
      var args = ImmutableArray.CreateBuilder<ArgumentProperties>(paraminfos.Length);
      var defaults = new object[paraminfos.Length];
      var types = new Type[paraminfos.Length];

      for (int i = 0; i < paraminfos.Length; i++) {
        var param = paraminfos[i];
        // These block ensure that the argument ordering is valid inductively, by
        // rejecting the current argument if the proceeding argument is one that it would
        // be illegal for it to come after.
        { // Block for handling LispArguments (marshalable things)
          var attr = param.GetCustomAttribute<LispArgumentAttribute>();
          if (attr != null) {
            if (!Arguments.IsMarshalable(param.ParameterType)) {
              throw new ArgumentException(string.Format(
                "cannot marshal parameter of type {0}", param.ParameterType));
            }
            ArgumentType type;
            if (args.Count == 0) {
              type = ArgumentType.PositionalOrKeyword;
            } else {
              var previous = args[i-1];
              switch(previous.Type) {
                case ArgumentType.PositionalOrKeyword:
                  if (attr.IsOptional && previous.IsOptional) {
                    throw new ArgumentException(
                      "found required positional argument after optional positional " +
                      "argument");
                  }
                  type = ArgumentType.PositionalOrKeyword;
                  break;
                case ArgumentType.RestCapture:
                case ArgumentType.RestIgnore:
                case ArgumentType.RestBlock:
                case ArgumentType.Keyword:
                  type = ArgumentType.Keyword;
                  break;
                case ArgumentType.RestKwCapture:
                case ArgumentType.RestKwIgnore:
                  throw new ArgumentException(
                    "found unexpected argument after keyword grouping argument");
                default:
                  throw new InvalidOperationException("This should be impossible.");
              }
            }
            var name = attr.Name ?? param.Name;
            args.Add(new ArgumentProperties(
              type, name: SymbolType.Create(name), isOptional: attr.IsOptional));
            if (attr.IsOptional && attr.Default != null
                && !param.ParameterType.IsAssignableFrom(attr.Default.GetType())) {
              throw new ArgumentException(
                "default value must be null or assignable to the type of the argument");
            }
            defaults[i] = attr.Default;
            types[i] = param.ParameterType;
            continue;
          }
        }
        { // Block for handling RestArguments (both keywords and positional)
          var attr = param.GetCustomAttribute<RestArgumentAttribute>();
          if (attr != null) {
            if (attr.IsKeyword && attr.Type == RestArgumentType.Capture
                && !param.ParameterType.IsAssignableFrom(
                  typeof(Dictionary<SymbolType, LispObject>))) {
              throw new ArgumentException(
                "keyword capture must accept a Dictionary<SymbolType, LispObject>");
            } else if (!attr.IsKeyword && attr.Type == RestArgumentType.Capture
                && !param.ParameterType.IsAssignableFrom(typeof(List<LispObject>))) {
              throw new ArgumentException(
                "positional capture must accept a List<LispObject>");
            }
            ArgumentType type;
            if (args.Count == 0) {
              // the case when the rest argument was the first argument to be passed.
              type = GetRestArgumentType(attr);
            } else {
              // The case when the rest argument was not the first argument in the list.
              var previous = args[i-1];
              switch(previous.Type) {
                case ArgumentType.PositionalOrKeyword:
                  type = GetRestArgumentType(attr);
                  break;
                case ArgumentType.Keyword:
                  if (attr.IsKeyword) {
                    throw new ArgumentException(
                      "found rest positional argument after keyword arguments.");
                  }
                  type = GetRestArgumentType(attr);
                  break;
                case ArgumentType.RestCapture:
                case ArgumentType.RestIgnore:
                case ArgumentType.RestBlock:
                  if (!attr.IsKeyword) {
                    throw new ArgumentException(
                      "found duplicate rest positional argument.");
                  }
                  type = GetRestArgumentType(attr);
                  break;
                case ArgumentType.RestKwCapture:
                case ArgumentType.RestKwIgnore:
                  throw new ArgumentException(
                    "found extra rest argument after rest keywords.");
                default:
                  throw new InvalidOperationException("This should be impossible.");
              }
            }
            args.Add(new ArgumentProperties(type));
            defaults[i] = null;
            types[i] = param.ParameterType;
            continue;
          }
        }
        throw new ArgumentException("found unannotated parameter {0}", param.Name);
      }

      arguments = args.ToImmutable();

      // Generate the magic function which this call magic will use to make function
      // calls.
      // Lambda parameter for the magic function which will take all of the arguments.
      var argsParam = Expression.Parameter(typeof(object[]), "args");

      var argumentExpressions = new List<Expression>();

      for (int i = 0; i < arguments.Length; i++) {
        var arg = arguments[i];
        switch (arg.Type) {
          case ArgumentType.PositionalOrKeyword:
          case ArgumentType.Keyword:
            {
              // create an expression which represents marshaling the value in the correct
              // array index as the given type.
              Expression exp = Expression.Call(
                typeof(Arguments), "Marshal", new Type[]{types[i]},
                Expression.Convert(
                  Expression.ArrayAccess(argsParam, Expression.Constant(i)),
                  typeof(LispObject)));
              if (arg.IsOptional) {
                // if the argument is optional, first null check it and provide the
                // default.
                Expression @default;
                if (defaults[i] == null) {
                  @default = Expression.Default(types[i]);
                } else {
                  @default = Expression.Constant(defaults[i], types[i]);
                }
                exp = Expression.Condition(
                  Expression.Equal(
                    Expression.ArrayAccess(argsParam, Expression.Constant(i)),
                    Expression.Constant(null, typeof(object))),
                  @default,
                  exp);
              }
              argumentExpressions.Add(exp);
            }
            break;
          case ArgumentType.RestIgnore:
          case ArgumentType.RestBlock:
          case ArgumentType.RestKwIgnore:
            // All ignorers and blocks result in a plain default(T)
            argumentExpressions.Add(Expression.Default(types[i]));
            break;
          case ArgumentType.RestCapture:
          case ArgumentType.RestKwCapture:
            argumentExpressions.Add(
              Expression.Convert(
                Expression.ArrayAccess(argsParam, Expression.Constant(i)),
                types[i]));
            break;
          default:
            throw new InvalidOperationException("This should be impossible.");
        }
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
          "MagicFunction wrapping {0}.{1}",
          boundMethod.DeclaringType.Name, boundMethod.Name),
        ImmutableList.Create(argsParam)).Compile();
    }

    private readonly MagicFunction implementation;

    private readonly ImmutableArray<ArgumentProperties> arguments;

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
      return implementation(Arguments.CollectArguments(pargs, kwargs, arguments));
    }
  }
}
