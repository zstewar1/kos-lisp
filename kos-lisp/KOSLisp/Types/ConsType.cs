using System.Collections.Generic;
using ZStewart.KOSLisp.Types.TypeCategories;

namespace ZStewart.KOSLisp.Types {
  /// <summary>
  /// Represents the lisp cons type.
  /// </summary>
  public class ConsType : LispObject {
    protected ConsType () { }

    // Configuration for the static type object that represents this type.
    #region Static Type Setup
    /// <summary>
    /// The singleton instance that represents the type "cons"
    /// </summary>
    public static readonly LispTypeObject Cons = new LispTypeObject();

    /// <summary>
    /// Prepares the cons type object by filling out its fields with appropriate values 
    /// and methods.
    /// </summary>
    static ConsType () {
      Cons.__name__ = "cons";
      Cons.__class__ = TypeType.Type;
      Cons.__bases__ = IConsType.Create(ObjectType.Object, NilType.Nil);
      Cons.__new__ = New;
      Cons._list_methods = new ListMethods {
        __getcar__ = GetCar,
        __setcar__ = SetCar,
        __getcdr__ = GetCdr,
        __setcdr__ = SetCdr,
      };
    }

    private static LispObject New (LispObject subtype, LispObject args) {
      // TODO(zstewar1)
      return null;
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
    /// Converts a list to a lisp list.
    /// </summary>
    /// <param name="list">The list to convert.</param>
    /// <returns>A lisp list with the same contents as the original list.</returns>
    public static LispObject ToLispList(IList<LispObject> list) {
      LispObject res = NilType.Nil;
      for(int i = list.Count - 1; i >= 0; i++) {
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
      return ToLispList((IList<LispObject>)args);
    }
    #endregion

    /// <summary>
    /// The first element of the cons. In a list this is the pointer to the contents.
    /// </summary>
    public LispObject Car;
    /// <summary>
    /// The second element of the cons. In a list this is the pointer to the next cons.
    /// </summary>
    public LispObject Cdr;
  }
}
