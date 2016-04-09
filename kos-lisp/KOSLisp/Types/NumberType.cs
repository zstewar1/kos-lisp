using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    private static LispTypeObject _number;
    public static LispTypeObject Number {
      get {
        if (_number != null) return _number;

        _number = new LispTypeObject {
          __name__ = "num",
        };
        _number.__class__ = TypeType.Type;
        _number.__bases__ = IConsType.ToLispTuple(ObjectType.Object);
        _number.__mro__ = IConsType.ToLispTuple(_number, ObjectType.Object);
        _number = LispTypeObject.ConfigureType(_number);
        // TODO(zstewar1): Not sure how to handle errors in "static" setup.
        if (_number == null) throw new InvalidOperationException();
        return _number;
      }
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
