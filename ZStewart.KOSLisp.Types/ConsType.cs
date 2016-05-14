using System;
using System.Collections.Generic;
using System.Text;

using ZStewart.KOSLisp.Types.Attributes;
using ZStewart.KOSLisp.Types.Helpers;
using ZStewart.KOSLisp.Types.TypeCategories;

namespace ZStewart.KOSLisp.Types {
  /// <summary>
  /// Represents the lisp cons type.
  /// </summary>
  public class ConsType : LispObject {
    protected ConsType () {
      // Ensure safety by preventing these from ever being read as null.
      Car = NilType.Nil;
      Cdr = NilType.Nil;
    }

    // Configuration for the static type object that represents this type.
    #region Static Type Setup
    private static LispType _cons;
    /// <summary>
    /// The singleton instance that represents the type "cons"
    /// </summary>
    public static LispType Cons {
      get {
        if (_cons != null) return _cons;

        _cons = new LispType {
          __name__ = "cons",
          _instance_type = typeof(ConsType),
          _list_methods = new ListMethods {
            __getcar__ = GetCar,
            __setcar__ = SetCar,
            __getcdr__ = GetCdr,
            __setcdr__ = SetCdr,
          },
        };
        _cons.__class__ = LispType.Type;
        _cons.__bases__ = IConsType.ToLispTuple(LispObject.Object);
        _cons.__mro__ = IConsType.ToLispTuple(_cons, LispObject.Object);
        _cons = LispType.ConfigureType(_cons);
        // TODO(zstewar1): Not sure how to handle errors in "static" setup.
        if (_cons == null) throw new InvalidOperationException();

        return _cons;
      }
    }

    [BuiltinFunction(Name = "--new--")]
    private static LispObject New(
        [Required] LispType subtype,
        [RestIgnore] byte ri, [RestKwIgnore] byte rki) {
      // Faster shortcut when it is the base type.
      if (subtype == Cons) {
        return new ConsType() {
          __class__ = Cons,
        };
      } else {
        if (!LispType.IsSubtype(subtype, Cons)) {
          throw ExceptionType.ThrowTypeError("type must be a subtype of cons");
        }
        if (!IsCorrectInstanceType(subtype, Cons)) {
          throw ExceptionType.ThrowTypeError(
              "cons.--new-- cannot be used to instantiate object of type {0}",
              subtype);
        }
        return new ConsType() {
          __class__ = subtype,
          __dict__ = DictType.Create(),
        };
      }
    }

    [BuiltinFunction(Name = "--init--")]
    private static void Init(
        [Required] ConsType self,
        [Required] LispObject car,
        [Optional(null)] LispObject cdr) {
      self.Car = car;
      self.Cdr = cdr ?? NilType.Nil;
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
        throw ExceptionType.ThrowTypeError(
          "instance must be of type {0}, was {1}", Cons, instance.__class__);
      }
    }

    private static LispObject GetCdr(LispObject instance) {
      if (instance is ConsType) {
        return (instance as ConsType).Cdr;
      } else {
        throw ExceptionType.ThrowTypeError(
          "instance must be of type {0}, was {1}", Cons, instance.__class__);
      }
    }

    private static LispObject SetCar(LispObject instance, LispObject value) {
      if (instance is ConsType) {
        (instance as ConsType).Car = value;
        return NilType.Nil;
      } else {
        throw ExceptionType.ThrowTypeError(
          "instance must be of type {0}, was {1}", Cons, instance.__class__);
      }
    }

    private static LispObject SetCdr(LispObject instance, LispObject value) {
      if (instance is ConsType) {
        (instance as ConsType).Cdr = value;
        return NilType.Nil;
      } else {
        throw ExceptionType.ThrowTypeError(
          "instance must be of type {0}, was {1}", Cons, instance.__class__);
      }
    }

    [BuiltinFunction(Name = "--repr--")]
    private static LispObject ToRepr([Required] ConsType self) {
      try {
        var len = ListOperations.Count(self);
        if (len == 2 && LispType.IsInstance(self.Car, SymbolType.Symbol)) {
          if (self.Car == SymbolType.Create("quote")) {
            return StringType.Create(
              "'" + StringType.GetReprString(ListOperations.GetCar(self.Cdr)));
          } else if (self.Car == SymbolType.Create("--backquote--")) {
            return StringType.Create(
              "`" + StringType.GetReprString(ListOperations.GetCar(self.Cdr)));
          } else if (self.Car == SymbolType.Create("--unquote--")) {
            return StringType.Create(
              "," + StringType.GetReprString(ListOperations.GetCar(self.Cdr)));
          } else if (self.Car == SymbolType.Create("--splice--")) {
            return StringType.Create(
              ",@" + StringType.GetReprString(ListOperations.GetCar(self.Cdr)));
          }
        }
      } catch (ExceptionWrapper ex) {
        if (!ExceptionType.Check(ex, ExceptionType.TypeError)) throw;
      }
      StringBuilder val = new StringBuilder("(");
      ConsType value = self;
      while (value != null) {
        val.Append(StringType.GetReprString(value.Car));
        LispObject cdr = value.Cdr;
        if (cdr is ConsType) {
          value = (ConsType)cdr;
          val.Append(" ");
        } else {
          value = null;
          if (cdr != NilType.Nil) {
            val.Append(" . ");
            val.Append(StringType.GetReprString(cdr));
          }
        }
      }
      val.Append(")");
      return StringType.Create(val.ToString());
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
      var cdr = ListOperations.GetCdr(originalList);
      var newcdr = Copy(cdr);
      return Create(car, newcdr);
    }

    /// <summary>
    /// Converts a list to a lisp list.
    /// </summary>
    /// <param name="list">The list to convert.</param>
    /// <returns>A lisp list with the same contents as the original list.</returns>
    public static LispObject ToLispList([RestCapture] IReadOnlyList<LispObject> list) {
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
  }
}
