using System;
using System.Collections.Generic;
using System.Text;
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
    private static LispTypeObject _icons;
    /// <summary>
    /// The singleton instance that represents the type "cons"
    /// </summary>
    public static LispTypeObject ICons {
      get {
        if (_icons != null) return _icons;

        _icons = new LispTypeObject {
          __name__ = "icons",
          __new__ = New,
          _list_methods = new ListMethods {
            __getcar__ = GetCar,
            __getcdr__ = GetCdr,
          },
        };
        _icons.__class__ = TypeType.Type;
        _icons.__bases__ = ToLispTuple(ObjectType.Object);
        _icons.__mro__ = ToLispTuple(_icons, ObjectType.Object);
        _icons = LispTypeObject.ConfigureType(_icons);
        // TODO(zstewar1): Not sure how to handle errors in "static" setup.
        if (_icons == null) throw new InvalidOperationException();
        return _icons;
      }
    }

    private static LispObject New (LispObject subtype, LispObject args) {
      // TODO(zstewar1)
      return null;
    }

    private static LispObject GetCar (LispObject instance) {
      // TODO(zstewar1): Maybe check isinstance? Maybe not though, all subtypes should
      // have initialized with our New, so instance must be a ConsType. If a subtype, then
      // it would either have an __dict__ if a dynamic type, or be a real C# subclass if
      // a static subtype. Either way, it should be safe to use a check/convert on the C#
      // type.
      if (instance is IConsType) {
        return (instance as IConsType).Car;
      } else {
        // TODO(zstewar1): Set error.
        return null;
      }
    }

    private static LispObject GetCdr (LispObject instance) {
      if (instance is IConsType) {
        return (instance as IConsType).Cdr;
      } else {
        // TODO(zstewar1): Set Error.
        return null;
      }
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
      if (car == null) return null;
      var cdr = ListOperations.GetCdr(originalList);
      if (cdr == null) return null;
      var newcdr = Copy(cdr);
      if (newcdr == null) return null;
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

    public override string ToString () {
      StringBuilder val = new StringBuilder("(tuple ");
      IConsType value = this;
      while (value != null) {
        val.Append(value.Car.ToString());
        LispObject cdr = value.Cdr;
        if (cdr is IConsType) {
          value = (IConsType)cdr;
          val.Append(" ");
        } else {
          value = null;
          if (cdr != NilType.Nil) {
            val.Append(" . ");
            val.Append(cdr.ToString());
          }
        }
      }
      val.Append(")");
      return val.ToString();
    }
  }
}
