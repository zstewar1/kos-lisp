using System;
using System.Collections.Generic;

using ZStewart.KOSLisp.Types.Helpers;
using ZStewart.KOSLisp.Types.TypeCategories;

namespace ZStewart.KOSLisp.Types {
  /// <summary>
  /// A class which serves as an iterator over a C# IEnumerable(LispObject)
  /// </summary>
  public class BuiltinIterType : LispObject {
    #region Static Type Setup
    private static LispType _builtinIter;
    public static LispType BuiltinIter {
      get {
        if (_builtinIter != null) return _builtinIter;

        _builtinIter = new LispType {
          __name__ = "BuiltinIter",
          _instance_type = typeof(BuiltinIterType),
          _list_methods = new ListMethods {
            __getcar__ = GetCar,
            __getcdr__ = GetCdr,
          },
        };
        _builtinIter.__class__ = LispType.Type;
        _builtinIter.__bases__ = IConsType.ToLispTuple(LispObject.Object);
        _builtinIter.__mro__ = IConsType.ToLispTuple(_builtinIter, LispObject.Object);
        _builtinIter = LispType.ConfigureType(_builtinIter);
        if (_builtinIter == null) throw new InvalidOperationException();

        return _builtinIter;
      }
    }

    private static LispObject GetCar (LispObject instance) {
      if (instance is BuiltinIterType) {
        return (instance as BuiltinIterType).Car;
      } else {
        throw ExceptionType.ThrowTypeError(
          "instance must be of type {0}, was {1}", BuiltinIter, instance.__class__);
      }
    }

    private static LispObject GetCdr (LispObject instance) {
      if (instance is BuiltinIterType) {
        return (instance as BuiltinIterType).Cdr;
      } else {
        throw ExceptionType.ThrowTypeError(
          "instance must be of type {0}, was {1}", BuiltinIter, instance.__class__);
      }
    }
    #endregion Static Type Setup

    #region Static Helper Methods
    public static LispObject Create(IEnumerable<LispObject> enumerable) {
      return Create(enumerable.GetEnumerator());
    }

    public static LispObject Create(IEnumerator<LispObject> enumerator) {
      bool ok;
      try {
        ok = enumerator.MoveNext();
      } catch (InvalidOperationException ex) {
        throw ExceptionType.ThrowRuntimeError(ex.Message);
      }
      if (!ok) return NilType.Nil;
      return new BuiltinIterType(enumerator.Current, enumerator) {
        __class__ = BuiltinIter,
      };
    }
    #endregion Static Helper Methods

    private BuiltinIterType (LispObject car, IEnumerator<LispObject> enumerator) {
      Car = car;
      this.enumerator = enumerator;
    }

    private IEnumerator<LispObject> enumerator;
    private LispObject _cdr;

    public LispObject Car { get; }
    public LispObject Cdr {
      get {
        if (_cdr != null) return _cdr;

        _cdr = Create(enumerator);
        return _cdr;
      }
    }
  }
}
