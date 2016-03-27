using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZStewart.KOSLisp.Types {
  public static class SymbolType {
    #region Static Type Setup
    public static readonly LispTypeObject Symbol = new LispTypeObject();

    static SymbolType () {
      Symbol.__name__ = "symbol";
      Symbol.__class__ = TypeType.Type;
      Symbol.__bases__ = IConsType.Create(ObjectType.Object, NilType.Nil);
    }
    #endregion

    public static LispObject Create(string identifier) {
      // TODO(zstewar1)
      return null;
    }
  }
}
