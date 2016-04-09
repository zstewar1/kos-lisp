using System;
using System.Collections.Generic;
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
    public static readonly LispTypeObject Dict = new LispTypeObject();

    static DictType () {
      Dict.__name__ = "dict";
      Dict.__class__ = TypeType.Type;
      Dict.__bases__ = IConsType.ToLispTuple(ObjectType.Object);
      Dict.__mro__ = IConsType.ToLispTuple(Dict, ObjectType.Object);
      Dict._map_methods = new MappingMethods() {
        __getitem__ = GetItem,
        __setitem__ = SetItem,
      };

      LispTypeObject.ConfigureType(Dict);
    }

    private static LispObject GetItem(LispObject dict, LispObject key) {
      return null;
    }

    private static LispObject SetItem(LispObject dict, LispObject key, LispObject value) {
      return null;
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
  }
}
