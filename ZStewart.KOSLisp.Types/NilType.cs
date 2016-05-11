using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ZStewart.KOSLisp.Types.Attributes;

namespace ZStewart.KOSLisp.Types {
  public sealed class NilType : SymbolType {
    private NilType () : base("nil") { }

    #region Static Type Setup
    private static LispType _nilClass;
    /// <summary>
    /// The singleton instance that represents the type "nil"
    /// </summary>
    public static LispType NilClass {
      get {
        if (_nilClass != null) return _nilClass;

        _nilClass = new LispType {
          __name__ = "NilType",
          _instance_type = typeof(NilType),
        };
        _nilClass.__class__ = LispType.Type;
        _nilClass.__bases__ = IConsType.ToLispTuple(Symbol);
        _nilClass.__mro__ = IConsType.ToLispTuple(_nilClass, Symbol, LispObject.Object);
        _nilClass = LispType.ConfigureType(_nilClass);
        // TODO(zstewar1): Not sure how to handle errors in "static" setup.
        if (_nilClass == null) throw new InvalidOperationException();

        return _nilClass;
      }
    }

    [BuiltinFunction(Name = "--new--")]
    private static LispObject New ([PositionalArgument] LispType subtype) {
      if (subtype != NilClass) {
        throw ExceptionType.ThrowTypeError("cannot create new instances of Nil");
      }
      return Nil;
    }

    [BuiltinFunction(Name = "--bool--")]
    private static LispObject ToBool([PositionalArgument] NilType nil) {
      return BoolType.F;
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
