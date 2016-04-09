using System;
using System.Collections.Generic;
using System.Linq;
using ZStewart.KOSLisp.Types.Helpers;
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
    public MappingMethods _map_methods;

    #region Setup Funtionality
    public static LispTypeObject ConfigureType (LispTypeObject type) {
      if (type.__mro__ == null) {
        var mro = Linearize(type);
        // Propagate errors.
        if (mro == null) return null;
        type.__mro__ = mro;
      }
      // TODO(zstewar1): Populate dict.
      type.__dict__ = DictType.Create();
      return type;
    }

    /// <summary>
    /// Computes the linearization of the given type's inheritance heirarchy.
    /// </summary>
    /// <param name="type">The type ot linearlize.</param>
    /// <returns>The linearization of the given type.</returns>
    private static LispObject Linearize(LispTypeObject type) {
      if (type.__bases__ == NilType.Nil) {
        return IConsType.ToLispTuple(type);
      }
      var merged = MergeMroBases(type.__bases__);
      if (merged == null) return null;
      return IConsType.Create(type, merged);
    }

    private static LispObject MergeMroBases(LispObject bases) {
      var mroList = new List<List<LispTypeObject>>();
      foreach (var b in ListOperations.IterList<LispTypeObject>(bases)) {
        if (b == null) return null;
        var mro = ListOperations.IterList<LispTypeObject>(b.__mro__).ToList();
        if (mro.Any(x => x == null)) return null;
        mroList.Add(mro);
      }
      var linearization = new List<LispTypeObject>();

      while (mroList.Count > 0) {
        LispTypeObject head = null;
        foreach (var mro in mroList) {
          head = mro.First();
          // Check that head is not in the tail of any other MRO.
          if (mroList
              .Where(o => o != mro)
              .Any(o => o.Skip(1).Any(t => t == head))) {
            // Cannot add this element now.
            continue;
          }
          break;
        }
        if (head == null) {
          // Cannot Create Consistend MRO Error
          return null;
        }
        linearization.Add(head);
        for (int i = 0; i < mroList.Count; i++) {
          if (mroList[i][0] == head) mroList[i].RemoveAt(0);
          if (mroList[i].Count == 0) mroList.RemoveAt(i--);
        }
      }
      return IConsType.ToLispTuple(linearization);
    }
    #endregion Setup Functionality

    // Temporary fix to get type info quicker.
    public override string ToString () {
      return string.Format("(type \"{0}\")", __name__);
    }
  }
}
