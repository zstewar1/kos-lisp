using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ZStewart.KOSLisp.Interpreter;
using ZStewart.KOSLisp.Types.Helpers;

namespace ZStewart.KOSLisp.Types {
  public static class ObjectType {
    #region Static Type Setup
    private static LispTypeObject _object;
    /// <summary>
    /// The singleton instance that represents the type "object".
    /// </summary>
    public static LispTypeObject Object {
      get {
        if (_object != null) return _object;

        _object = new LispTypeObject {
          __name__ = "object",
          __new__ = New,
        };
        _object.__class__ = TypeType.Type;
        _object.__bases__ = NilType.Nil;
        _object.__mro__ = IConsType.ToLispTuple(_object);
        _object = LispTypeObject.ConfigureType(_object);
        // TODO(zstewar1): Not sure how to handle errors in "static" setup.
        if (_object == null) throw new InvalidOperationException();
        return _object;
      }
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
      // (Possibly not necessary, since all types should be LispTypeObject-s)
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
        LispInterpreter.SetException(ExceptionType.CreateTypeError(
          "Argument must be a type"));
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

    /// <summary>
    /// Get an attribute of an object using its builtin __getattr__, or --getattribute--
    /// if provided.
    /// </summary>
    public static LispObject GetAttribute(LispObject obj, LispObject attribute) {
      // TODO(zstewar1): maybe check that attribute is a symbol?
      foreach (var objType in ListOperations.IterMro(obj)) {
        // Propagate errors.
        if (objType == null) return null;
        if (objType.__getattr__ != null) {
          return objType.__getattr__(obj, attribute);
        } else {
          LispObject __getattr__ = MappingOperations.GetItem(
            objType.__dict__, getattrattr);
          if (__getattr__ == null) {
            if (LispInterpreter.CheckException(ExceptionType.KeyError))
              LispInterpreter.ClearException();
            else return null;
          } else {
            return CallableOperations.Call(
              __getattr__, IConsType.ToLispTuple(obj, attribute));
          }
        }
      }
      LispInterpreter.SetException(ExceptionType.CreateTypeError(
        "\"{0}\" object has no attribute {1}", obj.__class__, attribute));
      return null;
    }
    #endregion Static Helper Methods
  }
}
