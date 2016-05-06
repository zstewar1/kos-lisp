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
          __setattr__ = SetAttr,
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
          throw ExceptionType.ThrowNotImplemented("Cannot initialize subtypes yet.");
          // TODO(zstewar1): Dynamic object.
        }
      } else {
        throw ExceptionType.ThrowTypeError("Argument must be a type");
      }
    }

    private static LispObject GetAttr(LispObject obj, LispObject attr) {
      if (!LispType.IsInstance(attr, SymbolType.Symbol)) {
        throw ExceptionType.ThrowTypeError(
          "attribute name must be symbol, not \"{0}\"", attr.__class__);
      }

      // Check if the item is in the object's dictionary, then check the class. If the
      // class item is a data descriptor fetch the value and return it, otherwise return
      // the object item, if available, otherwise return the fetch result.
      LispObject objdictitem = null;
      if (obj.__dict__ != null) {
        try {
          objdictitem = MappingOperations.GetItem(obj.__dict__, attr);
        } catch (ExceptionWrapper ex) {
          if (!ExceptionType.Check(ex, ExceptionType.KeyError)) throw;
        }
      }
      LispObject classitem = null;
      foreach (var targetType in ListOperations.IterMro(obj)) {
        try {
          classitem = MappingOperations.GetItem(targetType.__dict__, attr);
          break;
        } catch (ExceptionWrapper ex) {
          if (!ExceptionType.Check(ex, ExceptionType.KeyError)) throw;
        }
      }

      // If neither is null, we have to preference data-descriptors.
      if (classitem != null && objdictitem != null) {
        // Always give the object item if not get-able.
        if (!DescriptorOperations.IsDescriptor(classitem)) return objdictitem;

        // If there is an __get__, preference the class item only if there is also an
        // __set__ or __del__.

        // TODO(zstewar1): Also check if deleteable (either set or delete is data).
        if (DescriptorOperations.IsDataDescriptor(classitem)) {
          return DescriptorOperations.Get(classitem, obj, obj.__class__);
        }
        return objdictitem;
      }
      if (objdictitem != null) return objdictitem;
      if (classitem != null) {
        if (DescriptorOperations.IsDescriptor(classitem))
          return DescriptorOperations.Get(classitem, obj, obj.__class__);
        return classitem;
      }

      throw ExceptionType.ThrowAttributeError(
        "\"{0}\" object has no attribute {1}", obj.__class__, attr);
    }

    private static LispObject SetAttr(LispObject obj, LispObject attr, LispObject value) {
      if (!LispType.IsInstance(attr, SymbolType.Symbol)) {
        throw ExceptionType.ThrowTypeError(
          "attribute name must be symbol, not \"{0}\"", attr.__class__);
      }

      // Check if the item is in the class dictionary, then check if the clas item is a
      // data descriptor. If it is, call the data descriptor's set with the instance.
      // Otherwise, just set it on the instance dictionary (if available).
      LispObject classitem = null;
      foreach (var targetType in ListOperations.IterMro(obj)) {
        try {
          classitem = MappingOperations.GetItem(targetType.__dict__, attr);
          break;
        } catch (ExceptionWrapper ex) {
          if (!ExceptionType.Check(ex, ExceptionType.KeyError)) throw;
        }
      }

      if (classitem != null) {
        if (DescriptorOperations.IsDataDescriptor(classitem)) {
          DescriptorOperations.Set(classitem, obj, value);
          return NilType.Nil;
        }
      }
      if (obj.__dict__ != null) {
        MappingOperations.SetItem(obj.__dict__, attr, value);
        return NilType.Nil;
      }

      throw ExceptionType.ThrowAttributeError(
        "\"{0}\" object has no attribute {1}", obj.__class__, attr);
    }


    private static LispObject ToBool([PositionalArgument] LispObject nil) {
      return BoolType.T;
    }
    #endregion Static Type Setup

    #region Static Helper Methods

    #region Method Call Convenience Methods
    /// <summary>
    /// Call a method on the given object, with no arguments.
    /// </summary>
    /// <param name="obj">The object to call a method on.</param>
    /// <param name="method">The name of the method to call.</param>
    /// <returns>The result of calling method method of object</returns>
    public static LispObject Call(LispObject obj, string method) {
      return Call(obj, SymbolType.Create(method));
    }

    /// <summary>
    /// Call a method on the given object, with no arguments.
    /// </summary>
    /// <param name="obj">The object to call a method on.</param>
    /// <param name="method">The name of the method to call.</param>
    /// <returns>The result of calling method method of object</returns>
    public static LispObject Call(LispObject obj, SymbolType method) {
      return CallableOperations.Call(GetAttribute(obj, method));
    }

    /// <summary>
    /// Call a method on the given object, with only positional arguments.
    /// </summary>
    /// <param name="obj">The object to call a method on.</param>
    /// <param name="method">The name of the method to call.</param>
    /// <param name="args">The positional arguments to pass to the method.</param>
    /// <returns>The result of calling method method of object</returns>
    public static LispObject Call(
        LispObject obj, string method, List<LispObject> args) {
      return Call(obj, SymbolType.Create(method), args);
    }

    /// <summary>
    /// Call a method on the given object, with only position arguments.
    /// </summary>
    /// <param name="obj">The object to call a method on.</param>
    /// <param name="method">The name of the method to call.</param>
    /// <param name="args">The positional arguments to pass to the method.</param>
    /// <returns>The result of calling method method of object</returns>
    public static LispObject Call(
        LispObject obj, SymbolType method, List<LispObject> args) {
      return CallableOperations.Call(GetAttribute(obj, method), args);
    }

    /// <summary>
    /// Call a method on the given object, with only positional arguments.
    /// </summary>
    /// <param name="obj">The object to call a method on.</param>
    /// <param name="method">The name of the method to call.</param>
    /// <param name="args">The positional arguments to pass to the method.</param>
    /// <returns>The result of calling method method of object</returns>
    public static LispObject Call(
        LispObject obj, string method, params LispObject[] args) {
      return Call(obj, SymbolType.Create(method), new List<LispObject>(args));
    }

    /// <summary>
    /// Call a method on the given object, with only position arguments.
    /// </summary>
    /// <param name="obj">The object to call a method on.</param>
    /// <param name="method">The name of the method to call.</param>
    /// <param name="args">The positional arguments to pass to the method.</param>
    /// <returns>The result of calling method method of object</returns>
    public static LispObject Call(
        LispObject obj, SymbolType method, params LispObject[] args) {
      return Call(obj, method, new List<LispObject>(args));
    }

    /// <summary>
    /// Call a method on the given object, with positional and keyword arguments.
    /// </summary>
    /// <param name="obj">The object to call a method on.</param>
    /// <param name="method">The name of the method to call.</param>
    /// <param name="pargs">The positional arguments to pass to the method.</param>
    /// <param name="kwargs">The keyword arguments to pass to the method.</param>
    /// <returns>The result of calling method method of object</returns>
    public static LispObject Call(
        LispObject obj, string method,
        List<LispObject> pargs,
        Dictionary<SymbolType, LispObject> kwargs) {
      return Call(obj, SymbolType.Create(method), pargs, kwargs);
    }

    /// <summary>
    /// Call a method on the given object, with position and keyword arguments.
    /// </summary>
    /// <param name="obj">The object to call a method on.</param>
    /// <param name="method">The name of the method to call.</param>
    /// <param name="pargs">The positional arguments to pass to the method.</param>
    /// <param name="kwargs">The keyword arguments to pass to the method.</param>
    /// <returns>The result of calling method method of object</returns>
    public static LispObject Call(
        LispObject obj, SymbolType method,
        List<LispObject> pargs,
        Dictionary<SymbolType, LispObject> kwargs) {
      return CallableOperations.Call(GetAttribute(obj, method), pargs, kwargs);
    }
    #endregion Method Call Convenience Methods

    /// <summary>
    /// Get an attribute of an object using its builtin __getattr__, or --getattribute--
    /// if provided.
    /// </summary>
    public static LispObject GetAttribute(LispObject obj, LispObject attribute) {
      if (!LispType.IsInstance(attribute, SymbolType.Symbol)) {
        throw ExceptionType.ThrowTypeError("attribute name must be symbol");
      }

      try {
        return LookupHelpers.Lookup(
          obj, attribute,
          t => t.__getattr__ != null,
          t => t.__getattr__,
          PropConsts.GetAttribute,
          // We should never reach this since everything inherits from object and object
          // provides the final fallback getattribute method.
          () => ExceptionType.CreateAttributeError(
            "\"{0}\" object has no attribute {1}", obj.__class__, attribute));
      } catch (ExceptionWrapper ex) {
        if (!ExceptionType.Check(ex, ExceptionType.AttributeError)) throw;
      }

      return LookupHelpers.Lookup(
        obj, attribute,
        t => false,
        // Since the first predicate is false, this should never be called.
        t => null,
        PropConsts.GetAttr,
        () => ExceptionType.CreateAttributeError(
          "\"{0}\" object has no attribute {1}", obj.__class__, attribute));
    }

    public static LispObject SetAttribute(
        LispObject obj, LispObject attribute, LispObject value) {
      if (!LispType.IsInstance(attribute, SymbolType.Symbol)) {
        throw ExceptionType.ThrowTypeError("attribute name must be symbol");
      }

      return LookupHelpers.Lookup(
        obj, attribute, value,
        t => t.__setattr__ != null,
        t => t.__setattr__,
        PropConsts.SetAttr,
        () => ExceptionType.CreateAttributeError(
          "\"{0}\" object has no attribute {1}", obj.__class__, attribute));
    }

    /// <summary>
    /// Check if the given object has the specified attribute and returen true if it does,
    /// false if it does not. Null is returned if there is an error while looking up the
    /// attribute other than AttributeError.
    /// </summary>
    public static bool HasAttribute(LispObject obj, LispObject attribute) {
      try {
        GetAttribute(obj, attribute);
        return true;
      } catch (ExceptionWrapper ex) {
        if (!ExceptionType.Check(ex, ExceptionType.AttributeError)) throw;
      }
      return false;
    }
    #endregion Static Helper Methods
  }
}
