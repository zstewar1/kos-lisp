namespace ZStewart.KOSLisp.Types {
  /// <summary>
  /// How positional arguments should be treated.
  /// </summary>
  public enum PositionalType {
    /// <summary>
    /// Extra positional arguments will overrun into keyword arguments.
    /// </summary>
    KeywordOverrun,
    /// <summary>
    /// Extra positional arguments will be collected into a list.
    /// </summary>
    RestCapture,
    /// <summary>
    /// Extra positioanl arguments will be discarded.
    /// </summary>
    RestIgnore,
    /// <summary>
    /// Extra positional arguments are forbidden.
    /// </summary>
    RestIllegal,
  }
}
