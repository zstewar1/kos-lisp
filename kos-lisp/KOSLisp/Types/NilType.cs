using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZStewart.KOSLisp.Types {
  public sealed class NilType : SymbolType {
    private NilType () : base("nil") { }

    #region Static Type Setup
    /// <summary>
    /// The singleton instance that represents the type "nil"
    /// </summary>
    public static readonly LispTypeObject NilClass = new LispTypeObject();

    static NilType () {
      // See the TypeType static initializer for a note on static initializers.
      NilClass.__name__ = "NilType";
      NilClass.__class__ = TypeType.Type;
      NilClass.__bases__ = IConsType.ToLispTuple(Symbol);
      NilClass.__mro__ = IConsType.ToLispTuple(NilClass, Symbol, ObjectType.Object);
      NilClass.__new__ = New;
      LispTypeObject.ConfigureType(NilClass);

      Nil.__class__ = NilClass;
    }

    private static LispObject New (LispObject subtype, LispObject args) {
      // TODO(zstewar1): Set error when creating a new Nil.
      return null;
    }
    #endregion Static Type Setup

    public static readonly NilType Nil = new NilType();
  }
}
