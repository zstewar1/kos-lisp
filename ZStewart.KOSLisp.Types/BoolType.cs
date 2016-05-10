using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
          __new__ = new CallMagic(typeof(BoolType), "New"),
          _instance_type = typeof(BoolType),
        };
        _bool.__class__ = LispType.Type;
        _bool.__bases__ = IConsType.ToLispTuple(Symbol);
        _bool.__mro__ = IConsType.ToLispTuple(_bool, Symbol, LispObject.Object);
        _bool = LispType.ConfigureType(_bool);
        // TODO(zstewar1): Not sure how to handle errors in "static" setup.
        if (_bool == null) throw new InvalidOperationException();

        LispType.AddStatic(_bool, "ToBool", PropConsts.Bool);

        return _bool;
      }
    }

    private static LispObject New(
        [PositionalArgument] LispType subtype,
        [PositionalArgument] LispObject value) {
      if (subtype != Bool) {
        throw ExceptionType.ThrowTypeError("cannot create new instances of bool");
      }
      return From(value);
    }

    private static LispObject ToBool([PositionalArgument] BoolType obj) {
      return obj;
    }
    #endregion Static Type Setup

    #region Static Helper Methods
    public static BoolType Create(bool value) {
      return value ? T : F;
    }

    /// <summary>Convert an arbitrary lisp object to a boolean.</summary>
    /// <param name="obj">The object to convert.</param>
    /// <returns>True if the object's __bool__ is true, false if it is false
    public static BoolType From(LispObject obj) {
      var b = LookupHelpers.Lookup(
        obj,
        unused => false,
        unused => { throw new InvalidOperationException(); }, // Should never happen.
        PropConsts.Bool,
        () => ExceptionType.CreateAttributeError(
          "{0} object has no attribute {1}",
          obj.__class__, PropConsts.Bool));
      if (b == T) return T;
      if (b == F) return F;
      throw ExceptionType.ThrowTypeError(
        "unable to convert {0} object to bool.", obj.__class__);
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
