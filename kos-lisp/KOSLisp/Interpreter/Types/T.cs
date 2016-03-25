namespace ZStewart.KOSLisp.Interpreter.Types {
  /// <summary>
  /// A symbol which represents the boolean value "True". As a symbol, this is a 
  /// singleton.
  /// </summary>
  public sealed class LispT : LispSymbol {
    /// <summary>
    /// The singleton instance of T. Constructing in static setup ensures we get into the
    /// Symbol table with this instance rather than a regular LispSymbol.Of("t");
    /// </summary>
    private static readonly LispT t = new LispT();

    /// <summary>
    /// Gets the instance of T.
    /// </summary>
    public static LispT T { get { return t; } }

    /// <summary>
    /// Constructs "T" passing "t" to the symbol constructor and ensuring that this type
    /// gets inserted into the symbol table.
    /// </summary>
    private LispT () : base("t") { }
  }
}
