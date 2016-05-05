using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ZStewart.KOSLisp.Interpreter;

namespace ZStewart.KOSLisp.Types.Helpers {
  /// <summary>
  /// Helpers for working with descriptors.
  /// </summary>
  public static class DescriptorOperations {
    /// <summary>
    /// Get/Bind the data descriptor object.
    /// </summary>
    /// <param name="descriptor">The descriptor object.</param>
    /// <param name="obj">The object to bind to.</param>
    /// <param name="type">The type of the object being bound to.</param>
    /// <returns></returns>
    public static LispObject Get(LispObject descriptor, LispObject obj, LispObject type) {
      return LookupHelpers.Lookup(
        descriptor, obj, type,
        t => t.__get__ != null,
        t => t.__get__,
        PropConsts.Get,
        () => ExceptionType.CreateAttributeError(
          "\"{0}\" object has no attribute __get__", descriptor.__class__));
    }

    /// <summary>
    /// Set the data descriptor object.
    /// </summary>
    /// <param name="descriptor">The descriptor object.</param>
    /// <param name="obj">The object to bind to.</param>
    /// <param name="value">The value to set the descriptor to.</param>
    /// <returns></returns>
    public static LispObject Set(
        LispObject descriptor, LispObject obj, LispObject value) {
      LookupHelpers.Lookup(
        descriptor, obj, value,
        t => t.__set__ != null,
        t => t.__set__,
        PropConsts.Set,
        () => ExceptionType.CreateAttributeError(
          "\"{0}\" object has no attribute __set__", descriptor.__class__));
      return value;
    }

    public static bool IsDescriptor(LispObject descriptor) {
      return LookupHelpers.Query(descriptor, t => t.__get__ != null, PropConsts.Get);
    }

    public static bool IsDataDescriptor(LispObject descriptor) {
      return LookupHelpers.Query(descriptor, t => t.__set__ != null, PropConsts.Set);
    }
  }
}
