using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using ZStewart.KOSLisp.Interpreter;
using ZStewart.KOSLisp.Types.Attributes;
using ZStewart.KOSLisp.Types.Helpers;

namespace ZStewart.KOSLisp.Types {
  public class BuiltinDataDescriptorType : LispObject {
    #region Static Type Setup
    private static LispType _builtinDataDescriptor;
    public static LispType BuiltinDataDescriptor {
      get {
        if (_builtinDataDescriptor != null) return _builtinDataDescriptor;

        _builtinDataDescriptor = new LispType {
          __name__ = "BuiltinDataDescriptor",
          _instance_type = typeof(BuiltinDataDescriptorType),
        };
        _builtinDataDescriptor.__class__ = LispType.Type;
        _builtinDataDescriptor.__bases__ = IConsType.ToLispTuple(LispObject.Object);
        _builtinDataDescriptor.__mro__ = IConsType.ToLispTuple(
          _builtinDataDescriptor, LispObject.Object);
        _builtinDataDescriptor = LispType.ConfigureType(_builtinDataDescriptor);
        if (_builtinDataDescriptor == null) throw new InvalidOperationException();

        return _builtinDataDescriptor;
      }
    }
    #endregion

    private string propName;
    private Type propType;
    private PropertyInfo property;

    private void Get (LispObject instance) {
    }
  }
}
