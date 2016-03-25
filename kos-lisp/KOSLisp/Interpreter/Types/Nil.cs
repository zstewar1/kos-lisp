namespace ZStewart.KOSLisp.Interpreter.Types {
  /// <summary>
  /// The nil type. Represents false, the empty list, and nothing. Is both a Symbol and a 
  /// List. It is also a singleton so only one instance ever exists.
  /// </summary>
  public sealed class LispNil : LispSymbol, LispList {
    /// <summary>
    /// The one and only instance.
    /// </summary>
    private static readonly LispNil nil = new LispNil();
    /// <summary>
    /// Gets the value Nil.
    /// </summary>
    public static LispNil Nil { get { return nil; } }
    
    /// <summary>
    /// Construct nil, causing it to be entered into the symbol table as well.
    ///
    /// Because every symbol is a singleton looked up from a global symbol table after
    /// creation, this means that as long as the global static nil is constructed before
    /// any calls to LispSymbol.Of("nil"), calls to LispSymbol.Of("nil") will always 
    /// return the One True Instance.
    /// </summary>
    private LispNil () : base("nil") { }

    /// <summary>
    /// The Car of nil is always nil, and it cannot be set.
    /// </summary>
    public LispObject Car {
      get { return nil; }
      set { throw new InvalidOperationException("Cannot set Car of nil"); }
    }
    /// <summary>
    /// The Cdr of nil is always nil, and it cannot be set.
    /// </summary>
    public LispObject Cdr {
      get { return nil; }
      set { throw new InvalidOperationException("Cannot set Cdr of nil"); }
    }
  }
}
