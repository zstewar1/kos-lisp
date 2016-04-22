using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ZStewart.KOSLisp.Interpreter;
using ZStewart.KOSLisp.Types.Helpers;

namespace ZStewart.KOSLisp.Types {
  public static class TypeType {

    #region Static Type Setup
    private static LispTypeObject _type;
    public static LispTypeObject Type {
      get {
        // These initializers are designed to get the references correct among these
        // objects, but there's no guarantee that the properties will be correct during
        // setup.
        if (_type != null) return _type;

        // Setup the type.
        _type = new LispTypeObject {
          __name__ = "type",
          __new__ = New,
          __call__ = Call,
        };
        _type.__class__ = _type;
        _type.__bases__ = IConsType.ToLispTuple(ObjectType.Object);
        _type.__mro__ = IConsType.ToLispTuple(_type, ObjectType.Object);
        _type = LispTypeObject.ConfigureType(_type);
        // TODO(zstewar1): Not sure how to handle errors in "static" setup.
        if (_type == null) throw new InvalidOperationException();
        return _type;
      }
    }

    private static LispObject New(LispObject subtype, LispObject args) {
      return new LispObject {
        __class__ = Type,
      };
    }

    private static LispObject Call(LispObject receiver, LispObject args) {

      // TODO(zstewar1): Calling a type calls new then maybe init.
      return null;
    }
    #endregion Static Type Setup

    #region Static Helper Methods
    /// <summary>
    /// Determines if the instance is of the given type.
    /// </summary>
    /// <param name="instance">The instance to look up.</param>
    /// <param name="type">The type to look for.</param>
    /// <returns>
    /// True if instance is of type type, false if it is not, and null on error.
    /// </returns>
    public static bool? IsInstance(LispObject instance, LispObject type) {
      if (!(type is LispTypeObject)) {
        LispInterpreter.SetException(ExceptionType.CreateTypeError(
          "Second argument must be a type, got {0}", type));
        return null;
      }
      foreach (var t in ListOperations.IterMro(instance)) {
        if (t == null) return null;
        if (t == type) return true;
      }
      return false;
    }

    /// <summary>
    /// Determines if one type is a subtype of another.
    /// </summary>
    /// <param name="subtype">The type to test if it is a subtype.</param>
    /// <param name="type">The parent type to test subtype against.</param>
    /// <returns>
    /// True if subtype is a subtype of parentType, false if it is not and null on error.
    /// </returns>
    public static bool? IsSubtype(LispObject subtype, LispObject parentType) {
      if (!(subtype is LispTypeObject)) {
        LispInterpreter.SetException(ExceptionType.CreateTypeError(
          "First argument must be a type, got {0}", subtype));
        return null;
      }
      if (!(parentType is LispTypeObject)) {
        LispInterpreter.SetException(ExceptionType.CreateTypeError(
          "Second argument must be a type, got {0}", parentType));
        return null;
      }
      foreach (
          var t in ListOperations.IterList<LispTypeObject>(
            ((LispTypeObject)subtype).__mro__)) {
        if (t == null) return null;
        if (t ==  parentType) return true;
      }
      return false;
    }
    #endregion Static Helper Methods
  }
}
