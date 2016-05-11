using System;
using System.Collections.Generic;
using System.Text;

using ZStewart.KOSLisp.Types.Attributes;
using ZStewart.KOSLisp.Types.Helpers;
using ZStewart.KOSLisp.Types.TypeCategories;

namespace ZStewart.KOSLisp.Types {
  /// <summary>
  /// Immutable cons type.
  /// </summary>
  public class IConsType : LispObject {
    protected IConsType () { }

    /// <summary>
    /// Since ICons is immutable, it provides a second constructor so subtypes can
    /// instantiate the car and cdr.
    /// </summary>
    protected IConsType (LispObject car, LispObject cdr) {
      this.car = car;
      this.cdr = cdr;
    }

    // Configuration for the static type object that represents this type.
    #region Static Type Setup
    private static LispType _icons;
    /// <summary>
    /// The singleton instance that represents the type "cons"
    /// </summary>
    public static LispType ICons {
      get {
        if (_icons != null) return _icons;

        _icons = new LispType {
          __name__ = "icons",
          _instance_type = typeof(IConsType),
          _list_methods = new ListMethods {
            __getcar__ = GetCar,
            __getcdr__ = GetCdr,
          },
        };
        _icons.__class__ = LispType.Type;
        _icons.__bases__ = ToLispTuple(LispObject.Object);
        _icons.__mro__ = ToLispTuple(_icons, LispObject.Object);
        _icons = LispType.ConfigureType(_icons);
        // TODO(zstewar1): Not sure how to handle errors in "static" setup.
        if (_icons == null) throw new InvalidOperationException();

        return _icons;
      }
    }

    [BuiltinFunction(Name = "--new--")]
    private static LispObject New (
        [PositionalArgument] LispType subtype,
        [PositionalArgument] LispObject car,
        [PositionalArgument] LispObject cdr) {
      if (!LispType.IsSubtype(subtype, ICons)) {
        throw ExceptionType.ThrowSyntaxError("type must be a subtype of icons");
      }
      if (!IsCorrectInstanceType(subtype, ICons)) {
        throw ExceptionType.ThrowTypeError(
            "icons.--new-- cannot be used to instantiate object of type {0}",
            subtype);
      }
      var result = new IConsType(car, cdr) {
        __class__ = subtype,
      };
      if (subtype != ICons) {
        result.__dict__ = DictType.Create();
      }
      return result;
    }

    private static LispObject GetCar (LispObject instance) {
      // TODO(zstewar1): Maybe check isinstance? Maybe not though, all subtypes should
      // have initialized with our New, so instance must be a ConsType. If a subtype, then
      // it would either have an __dict__ if a dynamic type, or be a real C# subclass if
      // a static subtype. Either way, it should be safe to use a check/convert on the C#
      // type.
      if (instance is IConsType) {
        return (instance as IConsType).Car;
      }
      throw ExceptionType.ThrowTypeError(
        "instance must be of type {0}, was {1}", ICons, instance.__class__);
    }

    private static LispObject GetCdr (LispObject instance) {
      if (instance is IConsType) {
        return (instance as IConsType).Cdr;
      }
      throw ExceptionType.ThrowTypeError(
        "instance must be of type {0}, was {1}", ICons, instance.__class__);
    }

    [BuiltinFunction(Name = "--repr--")]
    private static LispObject ToRepr ([PositionalArgument] IConsType self) {
      StringBuilder val = new StringBuilder("(tuple ");
      IConsType value = self;
      while (value != null) {
        val.Append(StringType.GetObjectRepr(value.Car));
        LispObject cdr = value.Cdr;
        if (cdr is IConsType) {
          value = (IConsType)cdr;
          val.Append(" ");
        } else {
          value = null;
          if (cdr != NilType.Nil) {
            val.Append(" . ");
            val.Append(StringType.GetObjectRepr(cdr));
          }
        }
      }
      val.Append(")");
      return StringType.Create(val.ToString());
    }
    #endregion Static Type Setup

    #region Static Helper Methods
    public static IConsType Create(LispObject car, LispObject cdr) {
      return new IConsType {
        __class__ = ICons,
        car = car,
        cdr = cdr,
      };
    }

    /// <summary>
    /// Converts the given lisp list to a tuple (consisting of only IConses). Attempts to
    /// reuse as many existing IConses as possible, so any tail portion of the list
    /// consisting only of IConses will return the same reference.
    /// </summary>
    /// <param name="originalList">The list to convert.</param>
    /// <returns>The converted list.</returns>
    public static LispObject Copy(LispObject originalList) {
      if (originalList == NilType.Nil) return NilType.Nil;
      var car = ListOperations.GetCar(originalList);
      var cdr = ListOperations.GetCdr(originalList);
      var newcdr = Copy(cdr);
      // We use exact type equality because any subtype of ICons should still be
      // converted.
      if (newcdr == cdr && originalList.__class__ == ICons) {
        return originalList;
      }
      return Create(car, newcdr);
    }

    /// <summary>
    /// Converts a list to a lisp tuple.
    /// </summary>
    /// <param name="list">The list to convert.</param>
    /// <returns>A lisp tuple with the same contents as the original list.</returns>
    public static LispObject ToLispTuple(IReadOnlyList<LispObject> list) {
      LispObject res = NilType.Nil;
      for(int i = list.Count - 1; i >= 0; i--) {
        res = Create(list[i], res);
      }
      return res;
    }

    /// <summary>
    /// Creates a tuple from an arbitrary number of lisp objects.
    /// </summary>
    /// <param name="args">The objects that should be contained in the tuple.</param>
    /// <returns>A lisp tuple containing the given objects.</returns>
    public static LispObject ToLispTuple(params LispObject[] args) {
      return ToLispTuple((IReadOnlyList<LispObject>)args);
    }
    #endregion Static Helper Methods

    private LispObject car;
    private LispObject cdr;

    /// <summary>
    /// This is the car of the icons.
    /// </summary>
    public LispObject Car { get { return car; } }
    /// <summary>
    /// This is the cdr of the icons.
    /// </summary>
    public LispObject Cdr { get { return cdr; } }
  }
}
