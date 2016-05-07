using System;
using System.Collections.Generic;
using System.Linq;

using ZStewart.KOSLisp.Types.Attributes;

namespace ZStewart.KOSLisp.Types {
  public class NumberType : LispObject {
    protected NumberType () { }

    /// <summary>
    /// Since NumberType is immutable, it provides a second constructor so that subtypes
    /// can initialize the value.
    /// </summary>
    protected NumberType (double value) {
      this.value = value;
    }

    #region Static Type Setup
    private static LispType _number;
    public static LispType Number {
      get {
        if (_number != null) return _number;

        _number = new LispType {
          __name__ = "num",
          _instance_type = typeof(NumberType),
        };
        _number.__class__ = LispType.Type;
        _number.__bases__ = IConsType.ToLispTuple(LispObject.Object);
        _number.__mro__ = IConsType.ToLispTuple(_number, LispObject.Object);
        _number = LispType.ConfigureType(_number);
        // TODO(zstewar1): Not sure how to handle errors in "static" setup.
        if (_number == null) throw new InvalidOperationException();

        LispType.AddStatic(_number, "ToBool", PropConsts.Bool);
        LispType.AddStatic(_number, "ToStr", PropConsts.Str);

        return _number;
      }
    }

    private static LispObject ToBool([PositionalArgument] double value) {
      if (value == 0) return BoolType.F;
      return BoolType.T;
    }

    private static LispObject ToStr([PositionalArgument] double value) {
      return StringType.Create(value.ToString());
    }
    #endregion

    #region Static Helper Methods
    public static LispObject Create(double value) {
      return new NumberType {
        __class__ = Number,
        value = value,
      };
    }
    #endregion Static Helper Methods

    private double value;
    public double Value { get { return value; } }

    public override string ToString () {
      return Value.ToString();
    }
  }
}
