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
    public static readonly LispTypeObject Number = new LispTypeObject();

    static NumberType () {
      Number.__name__ = "num";
      Number.__class__ = TypeType.Type;
      Number.__bases__ = IConsType.ToLispTuple(ObjectType.Object);
      Number.__mro__ = IConsType.ToLispTuple(Number, ObjectType.Object);
      LispTypeObject.ConfigureType(Number);
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
