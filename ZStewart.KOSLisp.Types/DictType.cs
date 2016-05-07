using System;
using System.Collections.Generic;

using ZStewart.KOSLisp.Types.TypeCategories;
using ZStewart.KOSLisp.Types.Attributes;

namespace ZStewart.KOSLisp.Types {
  public class DictType : LispObject {

    #region Static Type Setup
    private static LispType _dict;
    public static LispType Dict {
      get {
        if (_dict != null) return _dict;
        _dict = new LispType {
          __name__ = "dict",
          _instance_type = typeof(DictType),
          _map_methods = new MappingMethods {
            __getitem__ = GetItem,
            __setitem__ = SetItem,
          },
        };
        _dict.__class__ = LispType.Type;
        _dict.__bases__ = IConsType.ToLispTuple(LispObject.Object);
        _dict.__mro__ = IConsType.ToLispTuple(_dict, LispObject.Object);
        _dict = LispType.ConfigureType(_dict);
        // TODO(zstewar1): Not sure how to handle errors in "static" setup.
        if (_dict == null) throw new InvalidOperationException();

        LispType.AddStatic(_dict, "IterKeys", "keys");
        LispType.AddStatic(_dict, "IterValues", "values");
        LispType.AddStatic(_dict, "Iter", "iter");

        return _dict;
      }
    }

    private static LispObject GetItem(LispObject dict, LispObject key) {
      if (!(dict is DictType)) {
        throw ExceptionType.ThrowTypeError(
          "dict must be a dictionary or dictionary subtype, was {0}", dict.__class__);
      }
      return ((DictType)dict).GetItem(key);
    }

    private static LispObject SetItem(LispObject dict, LispObject key, LispObject value) {
      if (!(dict is DictType)) {
        throw ExceptionType.ThrowTypeError(
          "dict must be a dictionary or dictionary subtype, was {0}", dict.__class__);
      }
      return ((DictType)dict).SetItem(key, value);
    }

    private static LispObject IterKeys([PositionalArgument] DictType self) {
      return BuiltinIterType.Create(self.IterKeys());
    }

    private static LispObject IterValues([PositionalArgument] DictType self) {
      return BuiltinIterType.Create(self.IterValues());
    }

    private static LispObject Iter([PositionalArgument] DictType self) {
      return BuiltinIterType.Create(self.Iter());
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

    /// <summary>
    /// Converts a C# dictionary of lisp objects to a lisp dictionary.
    /// </summary>
    /// <param name="dict">The dictionary to convert.</param>
    /// <returns>A lisp dictionary with the same contents as the original dict.</returns>
    public static DictType ToLispDict(IDictionary<SymbolType, LispObject> dict) {
      return new DictType (dict) {
        __class__ = Dict,
      };
    }
    #endregion Static Helper Methods

    private Dictionary<LispObject, LispObject> storage;

    protected DictType() {
      storage = new Dictionary<LispObject, LispObject>();
    }

    protected DictType(IDictionary<LispObject, LispObject> startingContents) {
      storage = new Dictionary<LispObject, LispObject>(startingContents);
    }

    protected DictType(IDictionary<SymbolType, LispObject> startingContents) {
      storage = new Dictionary<LispObject, LispObject>();
      foreach (var kvp in startingContents) {
        storage.Add(kvp.Key, kvp.Value);
      }
    }

    protected LispObject GetItem(LispObject key) {
      LispObject value;
      if (storage.TryGetValue(key, out value))
        return value;
      throw ExceptionType.ThrowKeyError(
        "the key {0} was not found in the dictionary", key);
    }

    protected LispObject SetItem(LispObject key, LispObject value) {
      if (value == null) {
        storage.Remove(key);
      } else {
        storage[key] = value;
      }
      return NilType.Nil;
    }

    protected IEnumerable<LispObject> IterKeys () {
      return storage.Keys;
    }

    protected IEnumerable<LispObject> IterValues () {
      return storage.Values;
    }

    protected IEnumerable<LispObject> Iter() {
      foreach (var kvp in storage) {
        yield return IConsType.Create(kvp.Key, kvp.Value);
      }
    }
  }
}
