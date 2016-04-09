using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZStewart.KOSLisp.Types {
  public class SymbolType : LispObject {
    #region Static Type Setup
    private static LispTypeObject _symbol;
    public static LispTypeObject Symbol {
      get {
        if (_symbol != null) return _symbol;

        _symbol = new LispTypeObject {
          __name__ = "symbol",
        };
        _symbol.__class__ = TypeType.Type;
        _symbol.__bases__ = IConsType.ToLispTuple(ObjectType.Object);
        _symbol.__mro__ = IConsType.ToLispTuple(Symbol, ObjectType.Object);
        _symbol = LispTypeObject.ConfigureType(_symbol);
        // TODO(zstewar1): Not sure how to handle errors in "static" setup.
        if (_symbol == null) throw new InvalidOperationException();
        return _symbol;
      }
    }

    // TODO(zstewar1): In language instantiation stuff.
    #endregion

    #region Static Helpers
    public static LispObject Create(string identifier) {
      SymbolType val;
      if (UniqueSymbolDictionary.TryGetValue(identifier.ToUpperInvariant(), out val))
        return val;
      val = new SymbolType(identifier);
      val.__class__ = Symbol;
      return val;
    }
    #endregion Static Helpers

    private static readonly Dictionary<string, SymbolType> UniqueSymbolDictionary =
      new Dictionary<string, SymbolType>();

    protected SymbolType (string identifier) {
      Identifier = identifier.ToUpperInvariant();
      if (UniqueSymbolDictionary.ContainsKey(Identifier)) {
        throw new InvalidOperationException(
          string.Format("Attempting to instantiate duplicate symbol {0}!", Identifier));
      }
      UniqueSymbolDictionary[Identifier] = this;
    }

    public string Identifier { get; }

    public override string ToString () {
      return Identifier;
    }
  }
}
