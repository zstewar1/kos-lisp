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
    /// <summary>
    /// String Comparer used for comparing symbols.
    /// </summary>
    public static StringComparer SymbolComparer {
      get { return StringComparer.InvariantCultureIgnoreCase; }
    }

    /// <summary>
    /// Shortcut function for doing symbol comparison of arbitrary strings. This compares
    /// the strings using the SymbolComparer.
    /// </summary>
    public static int Compare(string s1, string s2) {
      return SymbolComparer.Compare(s1, s2);
    }

    public static LispObject Create(string identifier) {
      SymbolType val;
      if (UniqueSymbolDictionary.TryGetValue(identifier, out val))
        return val;
      val = new SymbolType(identifier);
      val.__class__ = Symbol;
      return val;
    }
    #endregion Static Helpers

    private static readonly Dictionary<string, SymbolType> UniqueSymbolDictionary =
      new Dictionary<string, SymbolType>(SymbolComparer);

    protected SymbolType (string identifier) {
      // We don't need to do this for unique dictionary comparisons, but we do it so that
      // we get uniform output from read out symbols.
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
