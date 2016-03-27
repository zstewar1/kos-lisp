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
    #endregion Static Type Setup
  }
}
