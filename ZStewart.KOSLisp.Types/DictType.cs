using System;
using System.Collections.Generic;
using System.Text;

using ZStewart.KOSLisp.Types.Attributes;
using ZStewart.KOSLisp.Types.Helpers;
using ZStewart.KOSLisp.Types.TypeCategories;

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
        LispType.ConfigureType(_dict);

        return _dict;
      }
    }

    [BuiltinFunction(Name = "--new--")]
    private static DictType New(
        [Required] LispType subtype,
        [RestIgnore] byte ri, [RestKwIgnore] byte rki) {
      if (ReferenceEquals(subtype, Dict)) {
        return Create();
      } else {
        if (!LispType.IsSubtype(subtype, Dict)) {
          throw ExceptionType.ThrowTypeError("type must be a subtype of dict");
        }
        if (!IsCorrectInstanceType(subtype, Dict)) {
          throw ExceptionType.ThrowTypeError(
              "dict.--new-- cannot be used to instantiate object of type {0}",
              subtype);
        }
        return new DictType() {
          __class__ = Dict,
          __dict__ = Create(),
        };
      }
    }

    [BuiltinFunction(Name = "--init--")]
    [BuiltinFunction(Name = "update")]
    private static void Init(
        [Required] DictType self,
        [Optional(null)] List<LispObject> seq,
        [RestKwCapture] Dictionary<SymbolType, LispObject> kwargs) {
      if (seq != null) {
        for(int i = 0; i < seq.Count; i++) {
          LispObject key, value;
          try {
            key = ListOperations.GetCar(seq[i]);
            value = ListOperations.GetCdr(seq[i]);
          } catch (ExceptionWrapper ex) {
            if (!ExceptionType.CheckException(ex, ExceptionType.TypeError)) throw;
            throw ExceptionType.ThrowTypeError(
              "dictionary update sequence item #{0} was not a cons", i);
          }
          self.storage[key] = value;
        }
      }
      foreach (var kvp in kwargs) {
        self.storage[kvp.Key] = kvp.Value;
      }
    }

    [BuiltinFunction(Name = "--getitiem--")]
    private static LispObject GetItem(
        [Required] LispObject dict,
        [Required] LispObject key) {
      if (!(dict is DictType)) {
        throw ExceptionType.ThrowTypeError(
          "dict must be a dictionary or dictionary subtype, was {0}", dict.__class__);
      }
      return ((DictType)dict).GetItem(key);
    }

    [BuiltinFunction(Name = "--setitem--")]
    private static LispObject SetItem(
        [Required] LispObject dict,
        [Required] LispObject key,
        [Required] LispObject value) {
      if (!(dict is DictType)) {
        throw ExceptionType.ThrowTypeError(
          "dict must be a dictionary or dictionary subtype, was {0}", dict.__class__);
      }
      return ((DictType)dict).SetItem(key, value);
    }

    [BuiltinFunction(Name = "--delitem--")]
    private static void DelItem(
        [Required] DictType dict,
        [Required] LispObject key) {
      dict.SetItem(key, null);
    }

    [BuiltinFunction(Name = "--repr--")]
    private static StringType Repr(
        [Required] DictType dict) {
      var sb = new StringBuilder();
      sb.Append("{");
      bool first = true;
      foreach (var kvp in dict.storage) {
        if (!first) {
          sb.Append(' ');
        } else {
          first = false;
        }
        sb.AppendFormat(
            "({0} . {1})",
            StringType.GetReprString(kvp.Key),
            StringType.GetReprString(kvp.Value));
      }
      sb.Append("}");
      return StringType.Create(sb.ToString());
    }

    [BuiltinFunction(Name = "keys")]
    private static LispObject IterKeys([Required] DictType self) {
      return BuiltinIterType.Create(self.IterKeys());
    }

    [BuiltinFunction(Name = "values")]
    private static LispObject IterValues([Required] DictType self) {
      return BuiltinIterType.Create(self.IterValues());
    }

    [BuiltinFunction(Name = "iter")]
    private static LispObject Iter([Required] DictType self) {
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
