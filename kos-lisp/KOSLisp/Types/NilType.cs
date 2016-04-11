using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ZStewart.KOSLisp.Interpreter;

namespace ZStewart.KOSLisp.Types {
  public sealed class NilType : SymbolType {
    private NilType () : base("nil") { }

    #region Static Type Setup
    private static LispTypeObject _nilClass;
    /// <summary>
    /// The singleton instance that represents the type "nil"
    /// </summary>
    public static LispTypeObject NilClass {
      get {
        if (_nilClass != null) return _nilClass;

        _nilClass = new LispTypeObject {
          __name__ = "NilType",
          __new__ = New,
        };
        _nilClass.__class__ = TypeType.Type;
        _nilClass.__bases__ = IConsType.ToLispTuple(Symbol);
        _nilClass.__mro__ = IConsType.ToLispTuple(_nilClass, Symbol, ObjectType.Object);
        _nilClass = LispTypeObject.ConfigureType(_nilClass);
        // TODO(zstewar1): Not sure how to handle errors in "static" setup.
        if (_nilClass == null) throw new InvalidOperationException();
        return _nilClass;
      }
    }

    private static LispObject New (LispObject subtype, LispObject args) {
      if (args != Nil) {
        LispInterpreter.SetException(ExceptionType.CreateTypeError(string.Format(
          "{0} takes no arguments.", subtype)));
      }
      return Nil;
    }
    #endregion Static Type Setup

    private static NilType _nil;
    public static NilType Nil {
      get {
        if (_nil != null) return _nil;

        _nil = new NilType();
        _nil.__class__ = NilClass;
        return _nil;
      }
    }
  }
}
