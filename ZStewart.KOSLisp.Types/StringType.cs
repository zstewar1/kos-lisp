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
        LispType.AddStatic(_string, "ToRepr", PropConsts.Repr);

        return _string;
      }
    }

    /// <summary>
    /// Get the str value of this string. Which is just itself.
    /// </summary>
    private static LispObject ToStr([PositionalArgument] StringType str) {
      return str;
    }

    /// <summary>
    /// Create a representation string of this string, with all quotes appearing escaped,
    /// and the value wrapped in more quotes.
    /// </summary>
    private static LispObject ToRepr([PositionalArgument] StringType str) {
      return Create(
        new StringBuilder(str.Value)
          .Replace("\"", "\\\"")
          .Replace("\n", "\\n")
          .Replace("\r", "\\r")
          .Insert(0, "\"")
          .Append("\"")
          .ToString());
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

    public static StringType Format(string format, params object[] args) {
      return Create(string.Format(format, args));
    }

    public static string GetObjectStr(LispObject obj) {
      var str = Call(obj, "--str--");
      if (!(str is StringType)) {
        throw ExceptionType.ThrowTypeError(
          "result of --str-- must be a str, was {0}", obj.__class__);
      }
      return ((StringType)str).Value;
    }

    public static string GetObjectRepr(LispObject obj) {
      var repr = Call(obj, "--repr--");
      if (!(repr is StringType)) {
        throw ExceptionType.ThrowTypeError(
          "result of --repr-- must be a str, was {0}", obj.__class__);
      }
      return ((StringType)repr).Value;
    }
    #endregion Static Helper Methods

    private string value;
    public string Value { get { return value; } }
  }
}
