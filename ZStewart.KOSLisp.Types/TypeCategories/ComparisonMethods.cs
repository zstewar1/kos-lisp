using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZStewart.KOSLisp.Types.TypeCategories {
  /// <summary>
  /// Methods used for comparison (and hashing)
  /// </summary>
  public class ComparisonMethods {
    /// <summary>
    /// Called to implement equality comparison.
    /// </summary>
    public Func<LispObject, LispObject, LispObject> __eq__;
    /// <summary>
    /// Called to implement less-than-or-equal comparison.
    /// </summary>
    public Func<LispObject, LispObject, LispObject> __le__;
    /// <summary>
    /// Called to implement less-than comparison.
    /// </summary>
    public Func<LispObject, LispObject, LispObject> __lt__;
    /// <summary>
    /// Called to implement greater-than comparison.
    /// </summary>
    public Func<LispObject, LispObject, LispObject> __gt__;
    /// <summary>
    /// Called to implement greater-than-or-equal comparison.
    /// </summary>
    public Func<LispObject, LispObject, LispObject> __ge__;
    /// <summary>
    /// Called to get a hash value for this object.
    /// </summary>
    public Func<LispObject, LispObject> __hash__;
  }
}
