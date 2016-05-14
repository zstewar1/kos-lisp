using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
          __getattr__ = GetAttr,
          __setattr__ = SetAttr,
          _instance_type = typeof(LispObject),
        };
        _object.__class__ = LispType.Type;
        _object.__bases__ = NilType.Nil;
        _object.__mro__ = IConsType.ToLispTuple(_object);
        LispType.ConfigureType(_object);

        return _object;
      }
    }

    /// <summary>
    /// Instantiate an object of the given type.
    /// </summary>
    /// <param name="type">The type to instantiate.</param>
    /// <param name="args">The arguments to the __new__ method.</param>
    /// <returns>A created lisp object or null on error.</returns>
    [BuiltinFunction(Name = "--new--")]
    private static LispObject New(
        [Required] LispType type,
        [RestCapture] List<LispObject> args) {
      // TODO(zstewar1): Python like rules for "args" here:
      // - No args if doesn't override __init__ or __new__
      // - Ok to pass arbitrary args if overriding __init__ but not __new__
      // - No args if overriding __new__
      // See http://stackoverflow.com/a/19277824/1036501

      if (type == Object) {
        if (args.Count > 0) {
          throw ExceptionType.ThrowTypeError(
            "--new-- expected 1 argument, got {0}",
            args.Count + 1);
        }
        return new LispObject {
          __class__ = Object,
        };
      } else if (!IsCorrectInstanceType(type, Object)) {
        throw ExceptionType.ThrowTypeError(
          "object.--new-- cannot be used to instantiate object of type {0}",
          type);
      } else {
        // check that type is a subtype, then check the arguments.
        var newInit = DefinesNewOrInit(type, Object);
        if (args.Count > 0 && newInit == NewInitDefined.Init) {
          return new LispObject {
            __class__ = type,
            __dict__ = DictType.Create(),
          };
        }
        throw ExceptionType.ThrowTypeError(
          "--new-- expected 1 argument, got {0}",
          args.Count + 1);
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


    [BuiltinFunction(Name = "--bool--")]
    private static LispObject ToBool([Required] LispObject nil) {
      return BoolType.T;
    }

    [BuiltinFunction(Name = "--str--")]
    private static LispObject ToStr([Required] LispObject obj) {
      return StringType.GetRepr(obj);
    }

    [BuiltinFunction(Name = "--repr--")]
    private static LispObject ToRepr([Required] LispObject obj) {
      return StringType.Create(string.Format("[{0} object]", obj.__class__.__name__));
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

    [Flags]
    public enum NewInitDefined {
      None = 0,
      New = 1 << 0,
      Init = 1 << 1,
      Both = New | Init,
    }

    /// <summary>
    /// Checks if the given subtype, or any of its bases before supertype, defines a new
    /// or init method, and returns a flags enum telling which are defined.
    ///
    /// Raises an error if the given subtype is not a subtype of the given supertype.
    /// </summary>
    public static NewInitDefined DefinesNewOrInit(LispType subtype, LispType supertype) {
      bool isSubtype = false;
      var def = NewInitDefined.None;
      foreach(var type in ListOperations.IterList<LispType>(subtype.__mro__)) {
        if (type == supertype) {
          isSubtype = true;
          break;
        }

        try {
          GetAttribute(type, PropConsts.New);
          def |= NewInitDefined.New;
        } catch (ExceptionWrapper ex) {
          if (!ExceptionType.Check(ex, ExceptionType.AttributeError)) throw;
        }

        try {
          GetAttribute(type, PropConsts.Init);
          def |= NewInitDefined.Init;
        } catch (ExceptionWrapper ex) {
          if (!ExceptionType.Check(ex, ExceptionType.AttributeError)) throw;
        }
      }
      if (!isSubtype) {
        throw ExceptionType.ThrowTypeError(
          "type {0} is not a subtype of {1}",
          subtype, supertype);
      }
      return def;
    }

    public static bool IsCorrectInstanceType(LispType subtype, LispType supertype) {
      foreach(var type in ListOperations.IterList<LispType>(subtype.__mro__)) {
        if (type._instance_type != null) {
          return type._instance_type == supertype._instance_type;
        }
      }
      throw ExceptionType.ThrowTypeError("type {0} has no instance type!", subtype);
    }
    #endregion Static Helper Methods

    public override string ToString() {
      return StringType.GetStrString(this);
    }
  }
}
