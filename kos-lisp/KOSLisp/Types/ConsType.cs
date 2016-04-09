using System;
using System.Collections.Generic;
using System.Text;
using ZStewart.KOSLisp.Types.Helpers;
using ZStewart.KOSLisp.Types.TypeCategories;

namespace ZStewart.KOSLisp.Types {
  /// <summary>
  /// Represents the lisp cons type.
  /// </summary>
  public class ConsType : LispObject {
    protected ConsType () { }

    // Configuration for the static type object that represents this type.
    #region Static Type Setup
    private static LispTypeObject _cons;
    /// <summary>
    /// The singleton instance that represents the type "cons"
    /// </summary>
    public static LispTypeObject Cons {
      get {
        if (_cons != null) return _cons;

        _cons = new LispTypeObject {
          __name__ = "cons",
          __new__ = New,
          __init__ = Init,
          _list_methods = new ListMethods {
            __getcar__ = GetCar,
            __setcar__ = SetCar,
            __getcdr__ = GetCdr,
            __setcdr__ = SetCdr,
          },
        };
        _cons.__class__ = TypeType.Type;
        _cons.__bases__ = IConsType.ToLispTuple(ObjectType.Object);
        _cons.__mro__ = IConsType.ToLispTuple(_cons, ObjectType.Object);
        _cons = LispTypeObject.ConfigureType(_cons);
        // TODO(zstewar1): Not sure how to handle errors in "static" setup.
        if (_cons == null) throw new InvalidOperationException();
        return _cons;
      }
    }

    private static LispObject New (LispObject subtype, LispObject args) {
      // TODO(zstewar1): Check that subtype is really a subtype.
      var pargs = Arguments.GetPositionalArguments(args);
      if (pargs == null) return null;
      if (subtype != Cons && pargs.Count > 0) {
        // TODO(zstewar1): Expected no additional arguments.
        return null;
      } else if (subtype == Cons && pargs.Count != 2) {
        // TODO(zstewar1): Expected exactly two arguments.
        return null;
      }
      var result = new ConsType();
      result.__class__ = (LispTypeObject)subtype;
      // TODO(zstewar1): if subtype is the dynamic type type, add an __dict__
      return result;
    }

    private static LispObject Init (LispObject self, LispObject args) {
      var pargs = Arguments.GetPositionalArguments(args);
      if (pargs == null) return null;
      if (pargs.Count != 2) {
        // TODO(zstewar1): Wrong argument count
        return null;
      }
      ((ConsType)self).Car = pargs[0];
      ((ConsType)self).Cdr = pargs[1];
      return NilType.Nil;
    }

    private static LispObject GetCar(LispObject instance) {
      // TODO(zstewar1): Maybe check isinstance? Maybe not though, all subtypes should
      // have initialized with our New, so instance must be a ConsType. If a subtype, then
      // it would either have an __dict__ if a dynamic type, or be a real C# subclass if
      // a static subtype. Either way, it should be safe to use a check/convert on the C#
      // type.
      if (instance is ConsType) {
        return (instance as ConsType).Car;
      } else {
        // TODO(zstewar1): Set error.
        return null;
      }
    }

    private static LispObject GetCdr(LispObject instance) {
      if (instance is ConsType) {
        return (instance as ConsType).Cdr;
      } else {
        // TODO(zstewar1): Set Error.
        return null;
      }
    }

    private static LispObject SetCar(LispObject instance, LispObject value) {
      if (instance is ConsType) {
        (instance as ConsType).Car = value;
        return NilType.Nil;
      } else {
        // TODO(zstewar1): Set Error.
        return null;
      }
    }

    private static LispObject SetCdr(LispObject instance, LispObject value) {
      if (instance is ConsType) {
        (instance as ConsType).Cdr = value;
        return NilType.Nil;
      } else {
        // TODO(zstewar1): Set Error.
        return null;
      }
    }
    #endregion Static Type Setup

    // Methods to help other C# code interface with the cons type. These bypass the need
    // to retrieve the type object and call its __new__ method with appropriate arguments.
    #region Static Helper Methods
    public static ConsType Create(LispObject car, LispObject cdr) {
      return new ConsType {
        __class__ = Cons,
        Car = car,
        Cdr = cdr,
      };
    }

    /// <summary>
    /// Creates a copy of a lisp list consisting of all normal Conses.
    /// </summary>
    /// <param name="orignalList">The list to copy.</param>
    /// <returns>A copy of the given list as only normal conses.</returns>
    public static LispObject Copy(LispObject originalList) {
      if (originalList == NilType.Nil) return NilType.Nil;
      var car = ListOperations.GetCar(originalList);
      if (car == null) return null;
      var cdr = ListOperations.GetCdr(originalList);
      if (cdr == null) return null;
      var newcdr = Copy(cdr);
      return Create(car, newcdr);
    }

    /// <summary>
    /// Converts a list to a lisp list.
    /// </summary>
    /// <param name="list">The list to convert.</param>
    /// <returns>A lisp list with the same contents as the original list.</returns>
    public static LispObject ToLispList(IReadOnlyList<LispObject> list) {
      LispObject res = NilType.Nil;
      for(int i = list.Count - 1; i >= 0; i--) {
        res = Create(list[i], res);
      }
      return res;
    }

    /// <summary>
    /// Creates a list from an arbitrary number of lisp objects.
    /// </summary>
    /// <param name="args">The objects that should be contained in the list.</param>
    /// <returns>A lisp list containing the given objects.</returns>
    public static LispObject ToLispList(params LispObject[] args) {
      return ToLispList((IReadOnlyList<LispObject>)args);
    }
    #endregion

    /// <summary>
    /// The first element of the cons. In a list this is the pointer to the contents.
    /// </summary>
    public LispObject Car { get; set; }
    /// <summary>
    /// The second element of the cons. In a list this is the pointer to the next cons.
    /// </summary>
    public LispObject Cdr { get; set; }

    public override string ToString () {
      // TODO(zstewar1): This could raise an error. Later we should change these to call
      // the in-language --str-- method and return the value from that.
      var len = ListOperations.Count(this);
      // TODO(zstewar1): Check if this is an improper list error.
      // if (!len.HasValue) return null;
      if (len.HasValue && len.Value == 2) {
        var instance = TypeType.IsInstance(Car, SymbolType.Symbol);
        if (!instance.HasValue) return null;
        if (instance.Value) {
          var symb = (SymbolType)Car;
          if (string.Compare(
              symb.Identifier, "quote",
              StringComparison.InvariantCultureIgnoreCase) == 0) {
            return "'" + ListOperations.GetCar(Cdr).ToString();
          } else if (string.Compare(
              symb.Identifier, "--backquote--",
              StringComparison.InvariantCultureIgnoreCase) == 0) {
            return "`" + ListOperations.GetCar(Cdr).ToString();
          } else if (string.Compare(
              symb.Identifier, "--unquote--",
              StringComparison.InvariantCultureIgnoreCase) == 0) {
            return "," + ListOperations.GetCar(Cdr).ToString();
          } else if (string.Compare(
              symb.Identifier, "--splice--",
              StringComparison.InvariantCultureIgnoreCase) == 0) {
            return ",@" + ListOperations.GetCar(Cdr).ToString();
          }
        }
      }
      StringBuilder val = new StringBuilder("(");
      ConsType value = this;
      while (value != null) {
        val.Append(value.Car.ToString());
        LispObject cdr = value.Cdr;
        if (cdr is ConsType) {
          value = (ConsType)cdr;
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
