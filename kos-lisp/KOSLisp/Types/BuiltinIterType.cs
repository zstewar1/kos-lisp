using System.Collections.Generic

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

    }

    private static LispObject GetCdr (LispObject instance) {
    }
    #endregion Static Type Setup
  }
}
