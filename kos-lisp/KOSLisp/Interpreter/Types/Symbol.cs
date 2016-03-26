using System;
using System.Collections.Generic;
using System.Linq;

namespace ZStewart.KOSLisp.Interpreter.Types {
  /// <summary>
  /// Represents a symbol in lisp. A symbol is sort of a singleton string. Symbols are
  /// generally used to access variables, but they can also be used as variables to
  /// represent whatever.
  /// </summary>
  // TODO(zstewar1): Self-evaluating symbols? :keyword type. These need to be handled in 
  // the lookup system, probably.
  public abstract class LispSymbol : LispAtom {
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
        try {
          if (identifier.StartsWith(":")) {
            // Construct the keyword subtype if this symbol starts with the keyword marker.
            return new LispKeyword(identifier);
          } else if (identifier.Contains(".")) {
            return new LispSubreferenceSymbol(identifier);
          } else {
            return new LispBasicSymbol(identifier);
          }
        } catch {
          // If we got here then the symbol wasn't already in the lookup table.
          // Make sure it remains that way, since constuction failed.
          existingSymbols.Remove(identifier);
          // Rethrow the exception -- we don't actually want to catch it, but we do only
          // want to execute this cleanup on error.
          throw;
        }
      }
    }

    /// <summary>
    /// The string value of this symbol.
    /// </summary>
    public string Identifier { get; }

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

  /// <summary>
  /// Represents a : prefixed symbol. These should be setup to always refer to themselves.
  /// </summary>
  public sealed class LispKeyword : LispSymbol {
    /// <summary>
    /// Gets the equivalent non-keyword identifier.
    /// </summary>
    public LispSymbol Unprefixed { get { return Of(Identifier.Substring(1)); } }

    internal LispKeyword(string identifier) : base(identifier) {
      Preconditions.CheckArgument(identifier.StartsWith(":"));
    }
  }

  /// <summary>
  /// Represents a subreference symbol -- a symbol containing ".", e.g "A.B". This symbol
  /// represents a subreference -- a lookup within a lookup -- and can be broken down into
  /// sub symbols for each element of the path it represents.
  /// </summary>
  public sealed class LispSubreferenceSymbol : LispSymbol {

    /// <summary>
    /// Computes and returns the array of symbols that make up the subreferences of this
    /// symbol.
    /// </summary>
    public LispSymbol[] SubSymbols {
      get {
        return Identifier.Split('.').Select(symb => Of(symb)).ToArray();
      }
    }

    internal LispSubreferenceSymbol(string identifier) : base(identifier) {
      Preconditions.CheckArgument(
        identifier.Split('.').All(s => !string.IsNullOrEmpty(s)));
    }
  }

  /// <summary>
  /// A basic symbol with no special properties.
  /// </summary>
  public sealed class LispBasicSymbol : LispSymbol {
    internal LispBasicSymbol (string identifier) : base(identifier) { }
  }
}
