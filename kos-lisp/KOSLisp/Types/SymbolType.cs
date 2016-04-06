using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZStewart.KOSLisp.Types {
  public class SymbolType : LispObject {
    #region Static Type Setup
    public static readonly LispTypeObject Symbol = new LispTypeObject();

    static SymbolType () {
      Symbol.__name__ = "symbol";
      Symbol.__class__ = TypeType.Type;
      Symbol.__bases__ = IConsType.Create(ObjectType.Object, NilType.Nil);
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
