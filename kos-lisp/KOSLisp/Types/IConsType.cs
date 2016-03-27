using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZStewart.KOSLisp.Types {
  /// <summary>
  /// Immutable cons type.
  /// </summary>
  public sealed class IConsType : LispObject {
    /// <summary>
    /// Private constructor -- use a creation method.
    /// </summary>
    private IConsType () { }

    // Configuration for the static type object that represents this type.
    #region Static Type Setup
    /// <summary>
    /// The singleton instance that represents the type "cons"
    /// </summary>
    public static readonly LispTypeObject ICons = new LispTypeObject();

    /// <summary>
    /// Prepares the cons type object by filling out its fields with appropriate values 
    /// and methods.
    /// </summary>
    static IConsType () {
      // See the TypeType static initializer for a note on static initializers.
      ICons.__name__ = "icons";
      ICons.__class__ = TypeType.Type;
      ICons.__bases__ = Create(ObjectType.Object, NilType.Nil);
      ICons.__new__ = New;
    }

    private static LispObject New (LispObject subtype, LispObject args) {
      // TODO(zstewar1)
      return null;
    }
    #endregion Static Type Setup 

    #region Static Helper Methods
    public static IConsType Create(LispObject car, LispObject cdr) {
      return new IConsType {
        __class__ = ICons,
        car = car,
        cdr = cdr,
      };
    }
    #endregion Static Helper Methods

    private LispObject car;
    private LispObject cdr;

    /// <summary>
    /// This is the car of the icons.
    /// </summary>
    public LispObject Car { get; }
    /// <summary>
    /// This is the cdr of the icons.
    /// </summary>
    public LispObject Cdr { get; }
  }
}
