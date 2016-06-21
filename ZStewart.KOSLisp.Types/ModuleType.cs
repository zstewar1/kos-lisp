using System.Collections.Generic;

using ZStewart.KOSLisp.Types.Attributes;
using ZStewart.KOSLisp.Types.Helpers;
using ZStewart.KOSLisp.Types.TypeCategories;

namespace ZStewart.KOSLisp.Types {
  public class ModuleType : LispObject {
    #region Static Type Setup
    private static LispType _module;
    public static LispType Module {
      get {
        if (_module != null) return _module;
        _module = new LispType {
          __name__ = "module",
          _instance_type = typeof(ModuleType),
        };
        _module.__class__ = LispType.Type;
        _module.__bases__ = IConsType.ToLispTuple(LispObject.Object);
        _module.__mro__ = IConsType.ToLispTuple(_module, LispObject.Object);
        LispType.ConfigureType(_module);

        LispType.AddDataProperty(_module, "Name", PropConsts.Name);

        return _module;
      }
    }
    #endregion Static Type Setup

    #region Static Helper Methods
    public static ModuleType Create(string name) {
      return Create(SymbolType.Create(name));
    }

    public static ModuleType Create(SymbolType name) {
      return new ModuleType (name) {
        __class__ = Module,
        // Modules always have an instance dict.
        __dict__ = DictType.Create(),
      };
    }

    public static ModuleType Create(SymbolType name, LispObject builtins) {
      var module = Create(name);
      MappingOperations.SetItem(module.__dict__, PropConsts.Builtins, builtins);
      return module;
    }

    /// <summary>
    /// Retrieves a global from the specified module. Differes from GetAttribute in that
    /// if the symbol is missing, the module's --builtins-- attribute is also checked, and
    /// if the symbol is not found, the error is a NameError rather than an
    /// AttributeError.
    /// </summary>
    public static LispObject GetGlobal(ModuleType module, SymbolType symbol) {
      try {
        return LispObject.GetAttribute(module, symbol);
      } catch (ExceptionWrapper ex) {
        if (!ExceptionType.CheckException(ex, ExceptionType.AttributeError)) throw;
      }

      LispObject builtins;
      try {
        builtins = LispObject.GetAttribute(module, PropConsts.Builtins);
      } catch (ExceptionWrapper ex) {
        if (!ExceptionType.CheckException(ex, ExceptionType.AttributeError)) throw;
        throw ExceptionType.ThrowNameError(
            "name \"{0}\" is not defined", symbol);
      }

      if (builtins is ModuleType) {
        try {
          return LispObject.GetAttribute(builtins, symbol);
        } catch (ExceptionWrapper ex) {
          if (!ExceptionType.CheckException(ex, ExceptionType.AttributeError)) throw;
          throw ExceptionType.ThrowNameError("name \"{0}\" is not defined", symbol);
        }
      } else {
        try {
          return MappingOperations.GetItem(builtins, symbol);
        } catch (ExceptionWrapper ex) {
          if (!ExceptionType.CheckException(ex, ExceptionType.KeyError)) throw;
          throw ExceptionType.ThrowNameError("name \"{0}\" is not defined", symbol);
        }
      }
    }

    /// <summary>
    /// Sets a global symbol on the given module. This differs from SetAttribute in that
    /// AttributeErrors are converted to NameErrors.
    /// </summary>
    public static LispObject SetGlobal(
        ModuleType module, SymbolType symbol, LispObject value) {
      try {
        LispObject.SetAttribute(module, symbol, value);
        return NilType.Nil;
      } catch (ExceptionWrapper ex) {
        if (!ExceptionType.CheckException(ex, ExceptionType.AttributeError)) throw;
        throw ExceptionType.ThrowNameError("name \"{0}\" is not defined", symbol);
      }
    }
    #endregion Static Helper Methods

    public SymbolType Name { get; set; }

    protected ModuleType(string name) : this(SymbolType.Create(name)) { }

    protected ModuleType(SymbolType name) {
      Name = name;
    }

    [BuiltinFunction(Name = "--repr--")]
    private LispObject ToRepr() {
      return StringType.Format("[module {0}]", Name);
    }
  }
}
