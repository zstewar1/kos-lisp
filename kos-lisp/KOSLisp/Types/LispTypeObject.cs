using System;
using ZStewart.KOSLisp.Types.TypeCategories;

namespace ZStewart.KOSLisp.Types {
  public class LispTypeObject : LispObject {
    /// <summary>
    /// The name of this type.
    /// </summary>
    public string __name__;
    /// <summary>
    /// The base classes of this type. (An ICons).
    /// </summary>
    public LispObject __bases__;
    /// <summary>
    /// Function for creating a new instance of this type. Inherited.
    /// </summary>
    public Func<LispObject, LispObject, LispObject> __new__;
    /// <summary>
    /// Function for initializing data in this type. Inherited.
    /// </summary>
    public Func<LispObject, LispObject, LispObject> __init__;
    /// <summary>
    /// Function for calling this type. Inherited.
    /// </summary>
    public Func<LispObject, LispObject, LispObject> __call__;
    /// <summary>
    /// Function to look up a property of this type. Inherited.
    /// </summary>
    public Func<LispObject, LispObject, LispObject> __getattr__;
    /// <summary>
    /// Function to set a property of this type. Inherited.
    /// </summary>
    public Func<LispObject, LispObject, LispObject> __setattr__;
    /// <summary>
    /// Ordering to use when doing lookups on this type. ICons.
    /// </summary>
    public LispObject __mro__;

    /// <summary>
    /// Methods used to implent the Cons-List protocol.
    /// </summary>
    public ListMethods _list_methods;
    /// <summary>
    /// Methods used to implement the Dict protocol.
    /// </summary>
    public DictMethods _dict_methods;
  }
}
