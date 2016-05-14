namespace ZStewart.KOSLisp.Types {
  public static class PropConsts {
    // TODO(zstewar1): maybe save these values?
    #region Attribute Access
    /// <summary>
    /// The function used to get a fallback attribute if --getattribute-- raises
    /// AttributeError.
    /// </summary>
    public static SymbolType GetAttr { get { return SymbolType.Create("--getattr--"); } }
    /// <summary>
    /// The function used for the initial lookup of a symbol on an object.
    /// </summary>
    public static SymbolType GetAttribute {
      get {
        return SymbolType.Create("--getattribute--");
      }
    }
    /// <summary>
    /// The function used to set an attribute on an object.
    /// </summary>
    public static SymbolType SetAttr { get { return SymbolType.Create("--setattr--"); } }
    #endregion Attribute Access

    #region Mapping Protocol
    /// <summary>
    /// The function used to retrieve items from a dictionary.
    /// </summary>
    public static SymbolType GetItem { get { return SymbolType.Create("--getitem--"); } }
    /// <summary>
    /// The function used to set items in a dictionary.
    /// </summary>
    public static SymbolType SetItem { get { return SymbolType.Create("--setitem--"); } }
    #endregion

    /// <summary>
    /// The member which contains the builtins object.
    /// </summary>
    public static SymbolType Builtins {
      get {
        return SymbolType.Create("--builtins--");
      }
    }

    #region Descriptor Protocol
    /// <summary>
    /// The function used to get an underlying value in the [data] descriptor protocol.
    /// </summary>
    public static SymbolType Get { get { return SymbolType.Create("--get--"); } }
    /// <summary>
    /// The function used to set an underlying value in the data descriptor protocol.
    /// </summary>
    public static SymbolType Set { get { return SymbolType.Create("--set--"); } }
    #endregion

    /// <summary>
    /// The function used to call the given object as a function.
    /// </summary>
    public static SymbolType Call { get { return SymbolType.Create("--call--"); } }
    /// <summary>
    /// The function used to call the given object as a macro.
    /// </summary>
    public static SymbolType MacroExpand {
      get { return SymbolType.Create("--macroexpand--"); }
    }

    /// <summary>
    /// The function used to get a new instance of the given type.
    /// </summary>
    public static SymbolType New { get { return SymbolType.Create("--new--"); } }
    /// <summary>
    /// The function used to initialize an instance of the given type after it has been
    /// created.
    /// </summary>
    public static SymbolType Init { get { return SymbolType.Create("--init--"); } }

    /// <summary>
    /// The function used to convert the object to a boolean.
    /// </summary>
    public static SymbolType Bool { get { return SymbolType.Create("--bool--"); } }
    /// <summary>
    /// The function used to convert the object to a string.
    /// </summary>
    public static SymbolType Str { get { return SymbolType.Create("--str--"); } }
    /// <summary>
    /// The function used to convert the object to a general representation.
    /// </summary>
    public static SymbolType Repr { get { return SymbolType.Create("--repr--"); } }

    /// <summary>
    /// The name of an object, method, module, etc.
    /// </summary>
    public static SymbolType Name { get { return SymbolType.Create("--name--"); } }
  }
}
