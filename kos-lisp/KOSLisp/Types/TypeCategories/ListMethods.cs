using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZStewart.KOSLisp.Types.TypeCategories {
  /// <summary>
  /// Methods used to define the Cons-List protocol.
  /// </summary>
  public class ListMethods {
    /// <summary>
    /// Called to get the car of the object.
    /// </summary>
    public Func<LispObject, LispObject> __getcar__;
    /// <summary>
    /// Called to set the car of the object.
    /// </summary>
    public Func<LispObject, LispObject, LispObject> __setcar__;
    /// <summary>
    /// Called to get the cdr of the object.
    /// </summary>
    public Func<LispObject, LispObject> __getcdr__;
    /// <summary>
    /// Called to set the cdr of the object.
    /// </summary>
    public Func<LispObject, LispObject, LispObject> __setcdr__;
  }
}
