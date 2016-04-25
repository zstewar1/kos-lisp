using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using ZStewart.KOSLisp.Interpreter;
using ZStewart.KOSLisp.Types.Helpers;
using ZStewart.KOSLisp.Types.Attributes;

namespace ZStewart.KOSLisp.Types {
  public sealed class BoolType : SymbolType {
    private BoolType (string name) : base(name) { }

    #region Static Type Setup
    private static LispType _bool;
    /// <summary>
    /// The singleton instance that represents the type "bool"
    /// </summary>
    public static LispType Bool {
      get {
        if (_bool != null) return _bool;

        _bool = new LispType {
          __name__ = "bool",
          __new__ = New,
          _instance_type = typeof(BoolType),
        };
        _bool.__class__ = LispType.Type;
        _bool.__bases__ = IConsType.ToLispTuple(Symbol);
        _bool.__mro__ = IConsType.ToLispTuple(_bool, Symbol, LispObject.Object);
        _bool = LispType.ConfigureType(_bool);
        // TODO(zstewar1): Not sure how to handle errors in "static" setup.
        if (_bool == null) throw new InvalidOperationException();

        LispType.AddStatic(_bool, "ToBool", "--bool--");

        return _bool;
      }
    }

    private static LispObject New (LispObject subtype, LispObject args) {
      // TODO(zstewar1): Allow a single argument, and check if it can be interpreted as a
      // boolean and return either T or F appropriately.
      if (args != NilType.Nil) {
        LispInterpreter.SetException(ExceptionType.CreateTypeError(
          "{0} takes no arguments.", subtype));
        return null;
      }
      return F;
    }

    private static LispObject ToBool([PositionalArgument] BoolType obj) {
      return obj;
    }
    #endregion Static Type Setup

    #region Static Helper Methods
    /// <summary>Convert an arbitrary lisp object to a boolean.</summary>
    /// <param name="obj">The object to convert.</param>
    /// <returns>True if the object's __bool__ is true, false if it is false
    public static BoolType From(LispObject obj) {
      var b = LispObject.Call(obj, "--bool--", NilType.Nil);
      if (b == null) return null;
      if (b == T) return T;
      if (b == F) return F;
      LispInterpreter.SetException(ExceptionType.CreateTypeError(
        "unable to convert {0} object to bool.", obj.__class__));
      return null;
    }
    #endregion

    private static BoolType _f;
    public static BoolType F {
      get {
        if (_f != null) return _f;

        _f = new BoolType("f");
        _f.__class__ = Bool;
        return _f;
      }
    }

    private static BoolType _t;
    public static BoolType T {
      get {
        if (_t != null) return _t;

        _t = new BoolType("t");
        _t.__class__ = Bool;
        return _t;
      }
    }

    public bool Value { get { return this == T; } }
  }
}
