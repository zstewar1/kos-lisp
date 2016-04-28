namespace ZStewart.KOSLisp.Types.Helpers {
  public static class PropConsts {
    /// <summary>
    /// The function used to get a fallback attribute if --getattribute-- raises
    /// AttributeError.
    /// </summary>
    public static readonly SymbolType GetAttr = SymbolType.Create("--getattr--");
    /// <summary>
    /// The function used for the initial lookup of a symbol on an object.
    /// </summary>
    public static readonly SymbolType GetAttribute =
      SymbolType.Create("--getattribute--");

    /// <summary>
    /// The function used to get an underlying value in the descriptor protocol.
    /// </summary>
    public static readonly SymbolType Get = SymbolType.Create("--get--");
  }
}
