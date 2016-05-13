using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ZStewart.KOSLisp.Types.Attributes;
using ZStewart.KOSLisp.Types.Helpers;

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

        return _string;
      }
    }

    [BuiltinFunction(Name = "--new--")]
    private static StringType New(
        [PositionalArgument] LispType subtype,
        [RestArgument] List<LispObject> values) {
      if (values.Count > 1) {
        throw ExceptionType.ThrowTypeError(
          "--new-- takes at most 2 arguments, {0} given", values.Count + 1);
      }
      if (subtype == String) {
        if (values.Count == 1) {
          return GetStr(values[0]);
        } else {
          return Empty;
        }
      } else {
        if (!LispType.IsSubtype(subtype, String)) {
          throw ExceptionType.ThrowTypeError("type must be a subtype of str");
        }
        if (!IsCorrectInstanceType(subtype, String)) {
          throw ExceptionType.ThrowTypeError(
            "str.--new-- cannot be used to instantiate object of type {0}", subtype);
        }
        return new StringType(values.Count == 1 ? GetStrString(values[0]) : "") {
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
    /// Get the str value of this string. Which is just itself.
    /// </summary>
    [BuiltinFunction(Name = "--str--")]
    private static LispObject ToStr([PositionalArgument] StringType str) {
      return str;
    }

    /// <summary>
    /// Create a representation string of this string, with all quotes appearing escaped,
    /// and the value wrapped in more quotes.
    /// </summary>
    [BuiltinFunction(Name = "--repr--")]
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

    [BuiltinFunction(Name = "--bool--")]
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
      return new StringType(value) {
        __class__ = String,
      };
    }

    public static StringType Format(string format, params object[] args) {
      return Create(string.Format(format, args));
    }

    public static string GetStrString(LispObject obj) {
      return GetStr(obj).Value;
    }

    public static StringType GetStr (LispObject obj) {
      var str = LookupHelpers.Lookup(
        obj,
        unused => false,
        unused => { throw new InvalidOperationException(); }, // Should never happen.
        PropConsts.Str,
        () => ExceptionType.CreateAttributeError(
          "{0} object has no method {1}",
          obj.__class__, PropConsts.Str));
      if (!(str is StringType)) {
        throw ExceptionType.ThrowTypeError(
          "{0} returned non-string type (type {1})", PropConsts.Str, str.__class__);
      }
      return (StringType)str;
    }

    public static string GetReprString (LispObject obj) {
      return GetRepr(obj).Value;
    }

    public static StringType GetRepr(LispObject obj) {
      var repr = LookupHelpers.Lookup(
        obj,
        unused => false,
        unused => { throw new InvalidOperationException(); }, // Should never happen.
        PropConsts.Repr,
        () => ExceptionType.CreateAttributeError(
          "{0} object has no method {1}",
          obj.__class__, PropConsts.Repr));
      if (!(repr is StringType)) {
        throw ExceptionType.ThrowTypeError(
          "{0} returned non-string type (type {1})", PropConsts.Repr, repr.__class__);
      }
      return (StringType)repr;
    }
    #endregion Static Helper Methods

    private readonly string value;
    public string Value { get { return value; } }
  }
}
