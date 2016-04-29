using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ZStewart.KOSLisp.Types.Attributes;

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
    private static LispType _string;
    public static LispType String {
      get {
        if (_string != null) return _string;

        _string = new LispType {
          __name__ = "str",
          _instance_type = typeof(StringType),
        };
        _string.__class__ = LispType.Type;
        _string.__bases__ = IConsType.ToLispTuple(LispObject.Object);
        _string.__mro__ = IConsType.ToLispTuple(String, LispObject.Object);
        _string = LispType.ConfigureType(_string);
        // TODO(zstewar1): Not sure how to handle errors in "static" setup.
        if (_string == null) throw new InvalidOperationException();

        LispType.AddStatic(_string, "ToBool", PropConsts.Bool);
        LispType.AddStatic(_string, "ToStr", PropConsts.Str);

        return _string;
      }
    }

    private static LispObject ToStr([PositionalArgument] StringType str) {
      return str;
    }

    private static LispObject ToBool([PositionalArgument] string self) {
      return BoolType.Create(self.Length != 0);
    }
    #endregion

    #region Static Helper Methods
    private static StringType _empty;
    public static StringType Empty {
      get {
        if (_empty != null) return _empty;
        _empty = Create("");
        return _empty;
      }
    }

    public static StringType Create(string value) {
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
