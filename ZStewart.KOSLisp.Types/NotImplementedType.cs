using System;
using System.Collections.Generic;
using System.Linq;

using ZStewart.KOSLisp.Types.Attributes;

namespace ZStewart.KOSLisp.Types {
  public sealed class NotImplementedType : LispObject {
    #region Static Type Setup
    private static LispType _notImplementedClass;
    /// <summary>
    /// The singleton instance that represents the type "nil"
    /// </summary>
    public static LispType NotImplementedClass {
      get {
        if (_notImplementedClass != null) return _notImplementedClass;

        _notImplementedClass = new LispType {
          __name__ = "NotImplementedType",
          _instance_type = typeof(NotImplementedType),
        };
        _notImplementedClass.__class__ = LispType.Type;
        _notImplementedClass.__bases__ = IConsType.ToLispTuple(Object);
        _notImplementedClass.__mro__ = IConsType.ToLispTuple(
          _notImplementedClass, LispObject.Object);
        LispType.ConfigureType(_notImplementedClass);

        return _notImplementedClass;
      }
    }

    [BuiltinFunction(Name = "--new--")]
    private static LispObject New ([Required] LispType subtype) {
      if (subtype != NotImplementedClass) {
        throw ExceptionType.ThrowTypeError(
          "cannot create new instances of NotImplemented");
      }
      return NotImplemented;
    }

    [BuiltinFunction(Name = "--init--")]
    private static void Init([RestIgnore] byte ri, [RestKwIgnore] byte rki) {}
    #endregion Static Type Setup

    private static NotImplementedType _notImplemented;
    public static NotImplementedType NotImplemented {
      get {
        if (_notImplemented != null) return _notImplemented;

        _notImplemented = new NotImplementedType();
        _notImplemented.__class__ = NotImplementedClass;
        return _notImplemented;
      }
    }
  }
}
