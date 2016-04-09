using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZStewart.KOSLisp.Types {
  public class StringType : LispObject {
    protected StringType () { }

    /// <summary>
    /// Since StringType is immutable, it provides a second constructor so that subtypes
    /// can initialize the value.
    /// </summary>
    protected StringType (string value) {
      this.value = value;
    }

    #region Static Type Setup
    private static LispTypeObject _string;
    public static LispTypeObject String {
      get {
        if (_string != null) return _string;

        _string = new LispTypeObject {
          __name__ = "str",
        };
        _string.__class__ = TypeType.Type;
        _string.__bases__ = IConsType.ToLispTuple(ObjectType.Object);
        _string.__mro__ = IConsType.ToLispTuple(String, ObjectType.Object);
        _string = LispTypeObject.ConfigureType(_string);
        // TODO(zstewar1): Not sure how to handle errors in "static" setup.
        if (_string == null) throw new InvalidOperationException();
        return _string;
      }
    }
    #endregion

    #region Static Helper Methods
    public static LispObject Create(string value) {
      return new StringType {
        __class__ = String,
        value = value,
      };
    }
    #endregion Static Helper Methods

    private string value;
    public string Value { get { return value; } }

    public override string ToString () {
      StringBuilder val = new StringBuilder(Value);
      val.Replace("\"", "\\\"");
      val.Replace("\n", "\\n");
      val.Replace("\r", "\\r");
      val.Insert(0, "\"");
      val.Append("\"");
      return val.ToString();
    }
  }
}
