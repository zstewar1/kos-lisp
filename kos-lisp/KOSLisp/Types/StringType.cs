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
    public static readonly LispTypeObject String = new LispTypeObject();

    static StringType () {
      String.__name__ = "str";
      String.__class__ = TypeType.Type;
      String.__bases__ = IConsType.Create(ObjectType.Object, NilType.Nil);
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
    public string Value { get; }
  }
}
