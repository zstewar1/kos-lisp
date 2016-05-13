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

        return _number;
      }
    }

    [BuiltinFunction(Name = "--new--")]
    private static NumberType New(
        [PositionalArgument] LispType subtype,
        [RestArgument] List<LispObject> values) {
      if (values.Count > 1) {
        throw ExceptionType.ThrowTypeError(
          "--new-- takes at most 2 arguments, {0} given", values.Count + 1);
      }
      if (subtype == Number) {
        return Create(values.Count == 1 ? GetValue(values[0]) : 0.0);
      } else {
        if (!LispType.IsSubtype(subtype, Number)) {
          throw ExceptionType.ThrowTypeError("type must be a subtype of num");
        }
        if (!IsCorrectInstanceType(subtype, Number)) {
          throw ExceptionType.ThrowTypeError(
            "num.--new-- cannot be used to instantiate object of type {0}", subtype);
        }
        return new NumberType(values.Count == 1 ? GetValue(values[0]) : 0.0) {
          __class__ = subtype,
          __dict__ = DictType.Create(),
        };
      }
    }

    [BuiltinFunction(Name = "--init--")]
    private static void Init(
        [RestArgument] List<LispObject> unusedPargs,
        [RestKeywordArgument] Dictionary<SymbolType, LispObject> unusedKwargs) {}

    /// <summary>
    /// Helper method for the --new-- method which gets the numeric value of the argument
    /// if the argument is a number or string.
    /// </summary>
    private static double GetValue(LispObject value) {
      if (value is NumberType) {
        return ((NumberType)value).Value;
      } else if (value is StringType) {
        double res;
        if (double.TryParse(((StringType)value).Value, out res)) {
          return res;
        } else {
          throw ExceptionType.ThrowValueError(
            "could not convert string to number: {0}", value);
        }
      } else {
        throw ExceptionType.ThrowTypeError(
          "num argument must be a string or number not {0}", value.__class__);
      }
    }

    [BuiltinFunction(Name = "--bool--")]
    private static LispObject ToBool([PositionalArgument] double value) {
      if (value == 0) return BoolType.F;
      return BoolType.T;
    }

    [BuiltinFunction(Name = "--repr--")]
    private static LispObject ToRepr([PositionalArgument] double value) {
      return StringType.Create(value.ToString());
    }
    #endregion

    #region Static Helper Methods
    public static NumberType Create(double value) {
      return new NumberType (value) {
        __class__ = Number,
      };
    }
    #endregion Static Helper Methods

    private readonly double value;
    public double Value { get { return value; } }
  }
}
