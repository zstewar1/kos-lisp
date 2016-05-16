using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using static ZStewart.KOSLisp.Types.ExceptionType;
using static ZStewart.KOSLisp.Types.NotImplementedType;

using ZStewart.KOSLisp.Types.Attributes;
using ZStewart.KOSLisp.Types.Helpers;
using ZStewart.KOSLisp.Types.TypeCategories;

namespace ZStewart.KOSLisp.Types {
  public class StringType : LispObject {
    #region Static Type Setup
    private static LispType _string;
    public static LispType String {
      get {
        if (_string != null) return _string;

        _string = new LispType {
          __name__ = "str",
          _instance_type = typeof(StringType),
          _comparison_methods = new ComparisonMethods {
            __eq__ = Eq,
            __le__ = Le,
            __lt__ = Lt,
            __gt__ = Gt,
            __ge__ = Ge,
            __hash__ = Hash,
          },
        };
        _string.__class__ = LispType.Type;
        _string.__bases__ = IConsType.ToLispTuple(LispObject.Object);
        _string.__mro__ = IConsType.ToLispTuple(String, LispObject.Object);
        LispType.ConfigureType(_string);

        return _string;
      }
    }

    [BuiltinFunction(Name = "--new--")]
    private static StringType New(
        [Required] LispType subtype,
        [Optional(null)] LispObject value) {
      if (ReferenceEquals(subtype, String)) {
        if (value != null) {
          return GetStr(value);
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
        return new StringType(value != null ? GetStrString(value) : "") {
          __class__ = subtype,
          __dict__ = DictType.Create(),
        };
      }
    }

    [BuiltinFunction(Name = "--init--")]
    private static void Init([RestIgnore] byte ri, [RestKwIgnore] byte rki) {}

    [BuiltinFunction(Name = "--eq--")]
    private static LispObject Eq(
        [Required] LispObject self,
        [Required] LispObject other) {
      if (!(self is StringType)) {
        throw ExceptionType.ThrowTypeError("self must be a string");
      }
      if (other.__class__.RefEq(String)
          || !other.NotSubtypeOrRedefines(
            String, t => t._comparison_methods?.__eq__ != null, PropConsts.Eq)) {
        return BoolType.Create(
          comparer.Compare(
            ((StringType)self).Value, ((StringType)other).Value)
          == 0);
      }
      return NotImplemented;
    }

    [BuiltinFunction(Name = "--le--")]
    private static LispObject Le(
        [Required] LispObject self,
        [Required] LispObject other) {
      if (!(self is StringType)) {
        throw ExceptionType.ThrowTypeError("self must be a string");
      }
      if (other.__class__.RefEq(String)
          || !other.NotSubtypeOrRedefines(
            String, t => t._comparison_methods?.__le__ != null, PropConsts.Le)) {
        return BoolType.Create(
          comparer.Compare(
            ((StringType)self).Value, ((StringType)other).Value)
          <= 0);
      }
      return NotImplemented;
    }

    [BuiltinFunction(Name = "--lt--")]
    private static LispObject Lt(
        [Required] LispObject self,
        [Required] LispObject other) {
      if (!(self is StringType)) {
        throw ExceptionType.ThrowTypeError("self must be a string");
      }
      if (other.__class__.RefEq(String)
          || !other.NotSubtypeOrRedefines(
            String, t => t._comparison_methods?.__lt__ != null, PropConsts.Lt)) {
        return BoolType.Create(
          comparer.Compare(
            ((StringType)self).Value, ((StringType)other).Value)
          < 0);
      }
      return NotImplemented;
    }

    [BuiltinFunction(Name = "--gt--")]
    private static LispObject Gt(
        [Required] LispObject self,
        [Required] LispObject other) {
      if (!(self is StringType)) {
        throw ExceptionType.ThrowTypeError("self must be a string");
      }
      if (other.__class__.RefEq(String)
          || !other.NotSubtypeOrRedefines(
            String, t => t._comparison_methods?.__gt__ != null, PropConsts.Gt)) {
        return BoolType.Create(
          comparer.Compare(
            ((StringType)self).Value, ((StringType)other).Value)
          > 0);
      }
      return NotImplemented;
    }

    [BuiltinFunction(Name = "--ge--")]
    private static LispObject Ge(
        [Required] LispObject self,
        [Required] LispObject other) {
      if (!(self is StringType)) {
        throw ExceptionType.ThrowTypeError("self must be a string");
      }
      if (other.__class__.RefEq(String)
          || !other.NotSubtypeOrRedefines(
            String, t => t._comparison_methods?.__ge__ != null, PropConsts.Ge)) {
        return BoolType.Create(
          comparer.Compare(
            ((StringType)self).Value, ((StringType)other).Value)
          >= 0);
      }
      return NotImplemented;
    }

    [BuiltinFunction(Name = "--hash--")]
    private static LispObject Hash([Required] LispObject self) {
      if (!(self is StringType)) {
        throw ExceptionType.ThrowTypeError("self must be a number");
      }
      return NumberType.Create(((StringType)self).Value.GetHashCode());
    }

    #endregion

    /// <summary>
    /// String Comparer used for comparing lisp strings.
    /// </summary>
    private static StringComparer comparer => StringComparer.InvariantCultureIgnoreCase;

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
        unused => null,
        PropConsts.Str,
        () => ExceptionType.ThrowAttributeError(
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
        unused => null,
        PropConsts.Repr,
        () => ExceptionType.ThrowAttributeError(
          "{0} object has no method {1}",
          obj.__class__, PropConsts.Repr));
      if (!(repr is StringType)) {
        throw ExceptionType.ThrowTypeError(
          "{0} returned non-string type (type {1})", PropConsts.Repr, repr.__class__);
      }
      return (StringType)repr;
    }
    #endregion Static Helper Methods

    public string Value { get; }

    /// <summary>
    /// Since StringType is immutable, it provides a second constructor so that subtypes
    /// can initialize the value.
    /// </summary>
    protected StringType (string value) {
      Value = value;
    }

    #region Non-special Methods
    /// <summary>
    /// Get the str value of this string. Which is just itself.
    /// </summary>
    [BuiltinFunction(Name = "--str--")]
    private LispObject ToStr() {
      return this;
    }

    /// <summary>
    /// Create a representation string of this string, with all quotes appearing escaped,
    /// and the value wrapped in more quotes.
    /// </summary>
    [BuiltinFunction(Name = "--repr--")]
    private LispObject ToRepr() {
      return Create(
        new StringBuilder(Value)
          .Replace("\"", "\\\"")
          .Replace("\n", "\\n")
          .Replace("\r", "\\r")
          .Insert(0, "\"")
          .Append("\"")
          .ToString());
    }

    [BuiltinFunction(Name = "--bool--")]
    private LispObject ToBool() {
      return BoolType.Create(Value.Length != 0);
    }
    #endregion Non-special Methods
  }
}
