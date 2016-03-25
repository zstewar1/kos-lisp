using System.Collections.Generic;

namespace ZStewart.KOSLisp.Interpreter.Types {
  /// <summary>
  /// Represents a symbol in lisp. A symbol is sort of a singleton string. Symbols are
  /// generally used to access variables, but they can also be used as variables to
  /// represent whatever.
  /// </summary>
  public class LispSymbol : LispAtom {
    /// <summary>
    /// Global dictionary of extant symbols. This is used to ensure that the same symbol
    /// name always references the same value.
    /// </summary>
    // TODO(zstewart): Maybe use WeakReference<LispSymbol> and add a destructor that 
    // removes the entry? Probably not worthwhile now, and might never be.
    private static readonly IDictionary<string, LispSymbol> existingSymbols =
      new Dictionary<string, LispSymbol>();

    /// <summary>
    /// Gets the symbol with the given string identifier.
    /// </summary>
    /// <param name="identifier">The identifier of the symbol.</param>
    /// <returns>
    /// The symbol with the given string identifier. This will be a new instance if the 
    /// symbol did not exist yet, or a reference to the existing symbol if it did.
    /// </returns>
    public static LispSymbol Of(string identifier) {
      identifier = Preconditions.CheckNotNullOrEmpty(identifier).ToUpperInvariant();
      LispSymbol symb;
      if (existingSymbols.TryGetValue(identifier, out symb)) {
        return symb;
      } else {
        return new LispSymbol(identifier);
      }
    }

    /// <summary>
    /// The string value of this symbol.
    /// </summary>
    private string Identifier { get; }

    /// <summary>
    /// Constructs a symbol. It is an error to construct a symbol that already exists in 
    /// the global symbol table. You should not call this method directly, instead use 
    /// LispSymbol.Of("identifier")
    /// 
    /// This method is internal so that classes in other namespaces cannot subclass this 
    /// type. It cannot be private because both Nil and T are subclasses of Symbol.
    /// </summary>
    /// <param name="identifier">The identifier to use for this symbol.</param>
    internal LispSymbol(string identifier) {
      // The symbol is always the fully uppercased name of the identifier.
      identifier = Preconditions.CheckNotNullOrEmpty(identifier).ToUpperInvariant();
      // Always check if this symbol exists, then added it if it doesn't.
      // Throw if it does to prevent copied symbols.
      if (existingSymbols.ContainsKey(identifier)) {
        throw new DuplicatedSymbolException(
          string.Format("Duplicate definition of symbol {0}", identifier));
      }
      existingSymbols.Add(identifier, this);
      Identifier = identifier;
    }
    
    public override string ToString () {
      return Identifier;
    }
  }
}
