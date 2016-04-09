using System;
using System.Collections.Generic;

namespace ZStewart.KOSLisp.Types {
  public class DictType : LispObject {

    protected DictType() {}

    #region Static Type Setup
    public static readonly LispTypeObject Dict = new LispTypeObject();

    static DictType () {
      Dict.__name__ = "dict";
      Dict.__class__ = TypeType.Type;
      Dict.__bases__ = IConsType.ToLispTuple(ObjectType.Object);
    }
    #endregion Static Type Setup

    #region Static Helper Methods
    /// <summary>
    /// Converts a C# dictionary of lisp objects to a lisp dictionary.
    /// </summary>
    /// <param name="dict">The dictionary to convert.</param>
    /// <returns>A lisp dictionary with the same contents as the original dict.</returns>
    public static DictType ToLispDict(IDictionary<LispObject, LispObject> dict) {
      return new DictType () {
        __class__ = Dict,
        storage = new Dictionary<LispObject, LispObject>(dict),
      };
    }
    #endregion Static Helper Methods

    protected Dictionary<LispObject, LispObject> storage;
  }
}
