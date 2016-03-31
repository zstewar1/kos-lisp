using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZStewart.KOSLisp.Types {
  public static class ObjectType {
    #region Static Type Setup
    /// <summary>
    /// The singleton instance that represents the type "object".
    /// </summary>
    public static readonly LispTypeObject Object = new LispTypeObject();

    static ObjectType () {
      // See the TypeType static initializer for a note on static initializers.
      Object.__name__ = "object";
      Object.__class__ = TypeType.Type;
      Object.__bases__ = NilType.Nil;
    }

    /// <summary>
    /// Instantiate an object of the given type.
    /// </summary>
    /// <param name="type">The type to instantiate.</param>
    /// <param name="args">The arguments to the __new__ method.</param>
    /// <returns>A created lisp object or null on error.</returns>
    private static LispObject New(LispObject type, LispObject args) {
      // TODO(zstewar1): Python like rules for "args" here:
      // - No args if doesn't override __init__ or __new__
      // - Ok to pass arbitrary args if overriding __init__ but not __new__
      // - No args if overriding __new__
      // See http://stackoverflow.com/a/19277824/1036501

      // TODO(zstewar1): Check that type is a "type" (Correctly, using isinstance)
      if (type is LispTypeObject) {
        var t = (type as LispTypeObject);
        if (t == Object) {
          // Check no args.
          return new LispObject {
            __class__ = Object,
          };
        } else {
          // TODO(zstewar1): Dynamic object.
          return null;
        }
      } else {
        // TODO(zstewar1): Set an error.
        return null;
      }
    }
    #endregion Static Type Setup

  }
}
