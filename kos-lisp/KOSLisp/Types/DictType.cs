using System;
using System.Collections.Generic;

using ZStewart.KOSLisp.Interpreter;
using ZStewart.KOSLisp.Types.TypeCategories;

namespace ZStewart.KOSLisp.Types {
  public class DictType : LispObject {

    protected DictType() {
      storage = new Dictionary<LispObject, LispObject>();
    }

    protected DictType(IDictionary<LispObject, LispObject> startingContents) {
      storage = new Dictionary<LispObject, LispObject>(startingContents);
    }

    #region Static Type Setup
    private static LispTypeObject _dict;
    public static LispTypeObject Dict {
      get {
        if (_dict != null) return _dict;

        _dict = new LispTypeObject {
          __name__ = "dict",
          _map_methods = new MappingMethods {
            __getitem__ = GetItem,
            __setitem__ = SetItem,
          },
        };
        _dict.__class__ = TypeType.Type;
        _dict.__bases__ = IConsType.ToLispTuple(ObjectType.Object);
        _dict.__mro__ = IConsType.ToLispTuple(_dict, ObjectType.Object);
        _dict = LispTypeObject.ConfigureType(_dict);
        // TODO(zstewar1): Not sure how to handle errors in "static" setup.
        if (_dict == null) throw new InvalidOperationException();
        return _dict;
      }
    }

    private static LispObject GetItem(LispObject dict, LispObject key) {
      if (!(dict is DictType)) {
        LispInterpreter.SetException(ExceptionType.CreateTypeError(
          "dict must be a dictionary or dictionary subtype, was {0}", dict.__class__));
        return null;
      }
      return ((DictType)dict).GetItem(key);
    }

    private static LispObject SetItem(LispObject dict, LispObject key, LispObject value) {
      if (!(dict is DictType)) {
        LispInterpreter.SetException(ExceptionType.CreateTypeError(
          "dict must be a dictionary or dictionary subtype, was {0}", dict.__class__));
        return null;
      }
      return ((DictType)dict).SetItem(key, value);
    }
    #endregion Static Type Setup

    #region Static Helper Methods
    public static DictType Create() {
      return new DictType() {
        __class__ = Dict,
      };
    }

    /// <summary>
    /// Converts a C# dictionary of lisp objects to a lisp dictionary.
    /// </summary>
    /// <param name="dict">The dictionary to convert.</param>
    /// <returns>A lisp dictionary with the same contents as the original dict.</returns>
    public static DictType ToLispDict(IDictionary<LispObject, LispObject> dict) {
      return new DictType (dict) {
        __class__ = Dict,
      };
    }
    #endregion Static Helper Methods

    private Dictionary<LispObject, LispObject> storage;

    protected LispObject GetItem(LispObject key) {
      LispObject value;
      if (storage.TryGetValue(key, out value))
        return value;
      LispInterpreter.SetException(ExceptionType.CreateKeyError(
        "the key {0} was not found in the dictionary", key));
      return null;
    }

    protected LispObject SetItem(LispObject key, LispObject value) {
      if (value == null) {
        storage.Remove(key);
      } else {
        storage[key] = value;
      }
      return NilType.Nil;
    }
  }
}
