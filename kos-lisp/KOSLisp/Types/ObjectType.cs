using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZStewart.KOSLisp.Types.Helpers;

namespace ZStewart.KOSLisp.Types {
  public static class ObjectType {
    #region Static Type Setup
    /// <summary>
    /// The singleton instance that represents the type "object".
    /// </summary>
    public static readonly LispTypeObject Object = new LispTypeObject();

    static ObjectType () {
      // See the TypeType static initializer for a note on static initializers.
      Object.__name__ = "object";
      Object.__class__ = TypeType.Type;
      Object.__bases__ = NilType.Nil;
    }

    /// <summary>
    /// Instantiate an object of the given type.
    /// </summary>
    /// <param name="type">The type to instantiate.</param>
    /// <param name="args">The arguments to the __new__ method.</param>
    /// <returns>A created lisp object or null on error.</returns>
    private static LispObject New(LispObject type, LispObject args) {
      // TODO(zstewar1): Python like rules for "args" here:
      // - No args if doesn't override __init__ or __new__
      // - Ok to pass arbitrary args if overriding __init__ but not __new__
      // - No args if overriding __new__
      // See http://stackoverflow.com/a/19277824/1036501

      // TODO(zstewar1): Check that type is a "type" (Correctly, using isinstance)
      if (type is LispTypeObject) {
        var t = (type as LispTypeObject);
        if (t == Object) {
          // Check no args.
          return new LispObject {
            __class__ = Object,
          };
        } else {
          // TODO(zstewar1): Dynamic object.
          return null;
        }
      } else {
        // TODO(zstewar1): Set an error.
        return null;
      }
    }
    #endregion Static Type Setup

    #region Static Helper Methods
    // in-lang --getattr-- is a different method. --getattribute-- is the real 
    // unconditional lookup function in the language, as in Python.
    private static LispObject getattrattr = StringType.Create("--getattribute--");

    /// <summary>
    /// Call a method on the given object.
    /// </summary>
    /// <param name="obj">The object to call a method on.</param>
    /// <param name="method">The name of the method to call.</param>
    /// <param name="args">The arguments to the method.</param>
    /// <returns>The result of calling method method of object</returns>
    public static LispObject Call(LispObject obj, string method, LispObject args) {
      var m = GetAttribute(obj, StringType.Create(method));
      if (m == null) return null;
      return CallableOperations.Call(m, args);
    }

    public static LispObject GetAttribute(LispObject obj, LispObject attribute) {
      // TODO(zstewar1): maybe check that attribute is a string?
      LispTypeObject objType = obj.__class__;
      LispObject __mro__ = objType.__mro__;
      for (;;) {
        if (objType.__getattr__ != null) {
          return objType.__getattr__(obj, attribute);
        } else {
          LispObject __getattr__ = MappingOperations.GetItem(
            objType.__dict__, getattrattr);
          if (__getattr__ == null) {
            // TODO(zstewar1): Check the error. Continue for KeyError, abort for all else.
          } else {
            return CallableOperations.Call(
              __getattr__, IConsType.ToLispTuple(obj, attribute));
          }
        }
        if (__mro__ == NilType.Nil) {
          // TODO(zstewar1): __getcar__ not found error.
          return null;
        }
        LispObject nextType = ListOperations.GetCar(__mro__);
        if (nextType == null) return null; // Propagate errors.
        objType = nextType as LispTypeObject;
        if (objType == null) {
          // TODO(zstewar1): set a type error: types must be type type. (This should be
          // impossible anyway, since we should prevent setting arbitrary types).
          return null;
        }
        __mro__ = ListOperations.GetCdr(__mro__);
        if (__mro__ == null) return null; // Propagate errors.      
      }
    }
    #endregion Static Helper Methods
  }
}
