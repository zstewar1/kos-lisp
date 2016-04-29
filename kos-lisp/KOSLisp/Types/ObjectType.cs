using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ZStewart.KOSLisp.Interpreter;
using ZStewart.KOSLisp.Types.Attributes;
using ZStewart.KOSLisp.Types.Helpers;

namespace ZStewart.KOSLisp.Types {
  /// <summary>
  /// Base class for things that exist in lisp.
  ///
  /// (this file contains the static type object and other methods).
  /// </summary>
  public partial class LispObject {
    #region Static Type Setup
    private static LispType _object;
    /// <summary>
    /// The singleton instance that represents the type "object".
    /// </summary>
    public static LispType Object {
      get {
        if (_object != null) return _object;

        _object = new LispType {
          __name__ = "object",
          __new__ = New,
          __getattr__ = GetAttr,
          _instance_type = typeof(LispObject),
        };
        _object.__class__ = LispType.Type;
        _object.__bases__ = NilType.Nil;
        _object.__mro__ = IConsType.ToLispTuple(_object);
        _object = LispType.ConfigureType(_object);
        // TODO(zstewar1): Not sure how to handle errors in "static" setup.
        if (_object == null) throw new InvalidOperationException();

        LispType.AddStatic(_object, "ToBool", PropConsts.Bool);

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
      if (type is LispType) {
        var t = (type as LispType);
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

    private static LispObject GetAttr(LispObject obj, LispObject attr) {
      var instance = LispType.IsInstance(attr, SymbolType.Symbol);
      if (!instance.HasValue) return null;
      if (!instance.Value) {
        LispInterpreter.SetException(ExceptionType.CreateTypeError(
          "attribute name must be symbol, not \"{0}\"", attr.__class__));
        return null;
      }

      // Check if the item is in the object's dictionary, then check the class. If the
      // class item is a data descriptor (not yet implemented) fetch the value and return
      // it, otherwise return the object item, if available, otherwise return the fetch
      // result.
      LispObject objdictitem = null;
      if (obj.__dict__ != null) {
        objdictitem = MappingOperations.GetItem(obj.__dict__, attr);
        if (objdictitem == null) {
          if (LispInterpreter.CheckException(ExceptionType.KeyError))
            LispInterpreter.ClearException();
          else return null;
        }
      }
      LispObject classitem = null;
      foreach (var targetType in ListOperations.IterMro(obj)) {
        if (targetType == null) return null;
        classitem = MappingOperations.GetItem(targetType.__dict__, attr);
        if (classitem == null) {
          if (LispInterpreter.CheckException(ExceptionType.KeyError))
            LispInterpreter.ClearException();
          else return null;
        } else {
          break;
        }
      }
      // TODO(zstewar1): Preference data descriptors
      if (objdictitem != null) return objdictitem;
      if (classitem != null) return classitem;

      LispInterpreter.SetException(ExceptionType.CreateAttributeError(
        "\"{0}\" object has no attribute {1}", obj.__class__, attr));
      return null;
    }

    private static LispObject ToBool([PositionalArgument] LispObject nil) {
      return BoolType.T;
    }
    #endregion Static Type Setup

    #region Static Helper Methods
    // in-lang --getattr-- is a different method. --getattribute-- is the real
    // unconditional lookup function in the language, as in Python.
    private static LispObject getattributeattr = StringType.Create("--getattribute--");
    private static LispObject getattrattr = StringType.Create("--getattr--");

    /// <summary>
    /// Call a method on the given object.
    /// </summary>
    /// <param name="obj">The object to call a method on.</param>
    /// <param name="method">The name of the method to call.</param>
    /// <param name="args">The arguments to the method.</param>
    /// <returns>The result of calling method method of object</returns>
    public static LispObject Call(LispObject obj, string method, LispObject args) {
      var m = GetAttribute(obj, SymbolType.Create(method));
      if (m == null) return null;
      return CallableOperations.Call(m, IConsType.Create(obj, args));
    }

    /// <summary>
    /// Get an attribute of an object using its builtin __getattr__, or --getattribute--
    /// if provided.
    /// </summary>
    public static LispObject GetAttribute(LispObject obj, LispObject attribute) {
      var instance = LispType.IsInstance(attribute, SymbolType.Symbol);
      if (!instance.HasValue) return null;
      if (!instance.Value) {
        LispInterpreter.SetException(ExceptionType.CreateTypeError(
          "attribute name must be symbol"));
        return null;
      }

      var value = LookupHelpers.Lookup(
        obj, attribute,
        t => t.__getattr__ != null,
        t => t.__getattr__,
        getattributeattr,
        // We should never reach this since everything inherits from object and object
        // provides the final fallback getattribute method.
        () => ExceptionType.CreateAttributeError(
          "\"{0}\" object has no attribute {1}", obj.__class__, attribute));

      if (value != null) return value;
      if (!LispInterpreter.CheckException(ExceptionType.AttributeError)) return null;
      LispInterpreter.ClearException();

      return LookupHelpers.Lookup(
        obj, attribute,
        t => false,
        // Since the first predicate is false, this should never be called.
        t => null,
        getattrattr,
        () => ExceptionType.CreateAttributeError(
          "\"{0}\" object has no attribute {1}", obj.__class__, attribute));
    }

    /// <summary>
    /// Check if the given object has the specified attribute and returen true if it does,
    /// false if it does not. Null is returned if there is an error while looking up the
    /// attribute other than AttributeError.
    /// </summary>
    public static bool? HasAttribute(LispObject obj, LispObject attribute) {
      var result = GetAttribute(obj, attribute);
      if (result == null) {
        if (LispInterpreter.CheckException(ExceptionType.AttributeError)) {
          LispInterpreter.ClearException();
          return false;
        } else {
          return null;
        }
      } else {
        return true;
      }
    }
    #endregion Static Helper Methods
  }
}
