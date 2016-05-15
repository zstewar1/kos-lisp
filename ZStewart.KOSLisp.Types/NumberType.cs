using System;
using System.Collections.Generic;
using System.Linq;

using static ZStewart.KOSLisp.Types.NotImplementedType;

using ZStewart.KOSLisp.Types.Helpers;
using ZStewart.KOSLisp.Types.Attributes;
using ZStewart.KOSLisp.Types.TypeCategories;

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
          _comparison_methods = new ComparisonMethods {
            __eq__ = Eq,
            __le__ = Le,
            __lt__ = Lt,
            __gt__ = Gt,
            __ge__ = Ge,
            __hash__ = Hash,
          },
        };
        _number.__class__ = LispType.Type;
        _number.__bases__ = IConsType.ToLispTuple(Object);
        _number.__mro__ = IConsType.ToLispTuple(_number, Object);
        LispType.ConfigureType(_number);

        return _number;
      }
    }

    [BuiltinFunction(Name = "--new--")]
    private static NumberType New(
        [Required] LispType subtype,
        [Optional(null)] LispObject value) {
      if (ReferenceEquals(subtype, Number)) {
        return Create(value != null ? GetValue(value) : 0.0);
      } else {
        if (!LispType.IsSubtype(subtype, Number)) {
          throw ExceptionType.ThrowTypeError("type must be a subtype of num");
        }
        if (!IsCorrectInstanceType(subtype, Number)) {
          throw ExceptionType.ThrowTypeError(
            "num.--new-- cannot be used to instantiate object of type {0}", subtype);
        }
        return new NumberType(value != null ? GetValue(value) : 0.0) {
          __class__ = subtype,
          __dict__ = DictType.Create(),
        };
      }
    }

    [BuiltinFunction(Name = "--init--")]
    private static void Init([RestIgnore] byte ri, [RestKwIgnore] byte rki) {}

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

    [BuiltinFunction(Name = "--eq--")]
    private static LispObject Eq(
        [Required] LispObject self,
        [Required] LispObject other) {
      if (!(self is NumberType)) {
        throw ExceptionType.ThrowTypeError("self must be a number");
      }
      if (other.__class__.RefEq(Number)
          || !other.NotSubtypeOrRedefines(
            Number, t => t._comparison_methods?.__eq__ != null, PropConsts.Eq)) {
        return BoolType.Create(((NumberType)self).Value == ((NumberType)other).Value);
      }
      return NotImplemented;
    }

    [BuiltinFunction(Name = "--le--")]
    private static LispObject Le(
        [Required] LispObject self,
        [Required] LispObject other) {
      if (!(self is NumberType)) {
        throw ExceptionType.ThrowTypeError("self must be a number");
      }
      if (other.__class__.RefEq(Number)
          || !other.NotSubtypeOrRedefines(
            Number, t => t._comparison_methods?.__le__ != null, PropConsts.Le)) {
        return BoolType.Create(((NumberType)self).Value <= ((NumberType)other).Value);
      }
      return NotImplemented;
    }

    [BuiltinFunction(Name = "--lt--")]
    private static LispObject Lt(
        [Required] LispObject self,
        [Required] LispObject other) {
      if (!(self is NumberType)) {
        throw ExceptionType.ThrowTypeError("self must be a number");
      }
      if (other.__class__.RefEq(Number)
          || !other.NotSubtypeOrRedefines(
            Number, t => t._comparison_methods?.__lt__ != null, PropConsts.Lt)) {
        return BoolType.Create(((NumberType)self).Value < ((NumberType)other).Value);
      }
      return NotImplemented;
    }

    [BuiltinFunction(Name = "--gt--")]
    private static LispObject Gt(
        [Required] LispObject self,
        [Required] LispObject other) {
      if (!(self is NumberType)) {
        throw ExceptionType.ThrowTypeError("self must be a number");
      }
      if (other.__class__.RefEq(Number)
          || !other.NotSubtypeOrRedefines(
            Number, t => t._comparison_methods?.__gt__ != null, PropConsts.Gt)) {
        return BoolType.Create(((NumberType)self).Value > ((NumberType)other).Value);
      }
      return NotImplemented;
    }

    [BuiltinFunction(Name = "--ge--")]
    private static LispObject Ge(
        [Required] LispObject self,
        [Required] LispObject other) {
      if (!(self is NumberType)) {
        throw ExceptionType.ThrowTypeError("self must be a number");
      }
      if (other.__class__.RefEq(Number)
          || !other.NotSubtypeOrRedefines(
            Number, t => t._comparison_methods?.__ge__ != null, PropConsts.Ge)) {
        return BoolType.Create(((NumberType)self).Value >= ((NumberType)other).Value);
      }
      return NotImplemented;
    }

    [BuiltinFunction(Name = "--hash--")]
    private static LispObject Hash([Required] LispObject self) {
      if (!(self is NumberType)) {
        throw ExceptionType.ThrowTypeError("self must be a number");
      }
      return Create(((NumberType)self).Value.GetHashCode());
    }

    [BuiltinFunction(Name = "--bool--")]
    private static LispObject ToBool([Required] double value) {
      if (value == 0) return BoolType.F;
      return BoolType.T;
    }

    [BuiltinFunction(Name = "--repr--")]
    private static LispObject ToRepr([Required] double value) {
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
