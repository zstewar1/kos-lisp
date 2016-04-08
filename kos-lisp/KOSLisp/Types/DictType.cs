using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZStewart.KOSLisp.Types {
  public class DictType {
    #region Static Type Setup
    public static readonly LispTypeObject Dict = new LispTypeObject();

    static DictType () {
      Dict.__name__ = "dict";
      Dict.__class__ = TypeType.Type;
      Dict.__bases__ = IConsType.ToLispTuple(ObjectType.Object);
    }
    #endregion Static Type Setup
  }
}
