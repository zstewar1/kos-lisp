using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZStewart.KOSLisp.Types.TypeCategories {
  /// <summary>
  /// Methods used to define the dictionary protocol.
  /// </summary>
  public class DictMethods {
    /// <summary>
    /// Called to retrieve an item from a dictionary. Called with two arguments, the
    /// dictionary object, and the key to look up.
    /// </summary>
    public Func<LispObject, LispObject, LispObject> __getitem__;
    /// <summary>
    /// Called to put an item in a dictionary, or delete an object from a dictionary.
    /// Called with three arguments, the dictionary object, the key to set, and the value
    /// to emplace. If the value is null, this is a delete operation.
    /// </summary>
    public Func<LispObject, LispObject, LispObject, LispObject> __setitem__;
  }
}
