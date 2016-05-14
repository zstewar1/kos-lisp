using System;
using System.Collections.Generic;
using System.Linq;

using ZStewart.KOSLisp.Types;
using ZStewart.KOSLisp.Types.Attributes;

namespace ZStewart.KOSLisp.Modules.Builtins {
  public sealed class GenSymType : SymbolType {
    #region Static Type Setup
    private static LispType _gensym;
    public static LispType GenSym {
      get {
        if (_gensym != null) return _gensym;

        _gensym = new LispType {
          __name__ = "gensym",
          _instance_type = typeof(GenSymType),
        };
        _gensym.__class__ = LispType.Type;
        _gensym.__bases__ = IConsType.ToLispTuple(SymbolType.Symbol);
        _gensym = LispType.ConfigureType(_gensym);
        if (_gensym == null) throw new InvalidOperationException();

        return _gensym;
      }
    }

    [BuiltinFunction(Name = "--new--")]
    private static GenSymType New(
        [PositionalArgument] LispType subtype,
        [RestArgument] List<LispObject> values) {
      if (subtype != GenSym) {
        throw ExceptionType.ThrowTypeError(
          "gensym can only be used to instantiate new gensyms");
      }
      if (values.Count > 1) {
        throw ExceptionType.ThrowTypeError(
          "gensym.--new-- takes at most 2 arguments, {0} given", values.Count + 1);
      }
      if (values.Count == 1) {
        var value = values[0];
        if (value is StringType || value is NumberType) {
          return Create(StringType.GetStrString(value));
        } else {
          throw ExceptionType.ThrowTypeError(
            "value must be a string or number, not {0}", value.__class__);
        }
      } else {
        return Create();
      }
    }
    #endregion Static Type Setup

    #region Static Helper Methods
    public static GenSymType Create() {
      return Create(string.Format("gensym-{0}", nextAutoIdent++));
    }

    public static new GenSymType Create(string identifier) {
      return new GenSymType(identifier) {
        __class__ = GenSym,
      };
    }
    #endregion Static Helper Methods

    private static int nextAutoIdent = 0;

    /// <summary>
    /// Gensym constructs with gensym always set to true.
    /// </summary>
    private GenSymType(string identifier) : base(identifier, true) {}
  }
}
