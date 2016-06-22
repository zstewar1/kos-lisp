namespace ZStewart.KOSLisp.Compile.Generators.Json {
  public enum AstType {
    Const,
    Defmacro,
    Defun,
    Call,
    Global,
    If,
    Import,
    Lambda,
    Let,
    Local,
    Progn,
    Set,
    Try,
  }

  /// <summary>
  /// Represents an element of the jsonified abstract syntax tree.
  /// </summary>
  public abstract class JAst {
    /// <summary>
    /// The type of this AST.
    /// </summary>
    [JsonConvert(typeof(StringEnumConverter))]
    public abstract AstType Type { get; }

    /// <summary>
    /// The class is public, but all implementations are kept internal with an
    /// internal-only constructor.
    /// </summary>
    internal JAst() {}
  }

  /// <summary>
  /// Marker base class for bindings.
  /// </summary>
  public abstract class JBinding : JAst {
    inernal JBinding(string identifier) {
      Identifier = identifier;
    }

    /// <summary>
    /// The identifier for this binding.
    /// </summary>
    public string Identifier { get; }
  }

  public class JConst : JAst {
    internal JConst(int value) {
      Value = value;
    }
    public override AstType Type => AstType.Const;
    public int Value { get; }
  }

  public class JDefunOrMacro : JProgn {
    internal JDefunOrMacro(
        IEnumerable<JAst> forms,
        IEnumerable<JArgument> args,
        JBinding name, bool isMacro) : base(forms) {
      if (isMacro && name == null) {
        throw new ArgumentException("macros must have a name");
      }
      Args = ImmutableList.CreateRange(args);
      Name = name;
      if (isMacro) {
        Type = AstType.Defmacro;
      } else if (Name != null) {
        Type = AstType.Defun;
      } else {
        Type = AstType.Lambda;
      }
    }
    public override AstType Type { get; }
    public ImmutableList<JArgument> Args { get; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public JBinding Name { get; }

    /// <summary>
    /// Represents an argument in a defun/defmacro/lambda.
    /// </summary>
    public class JArgument {
      [JsonConvert(typeof(StringEnumConverter))]
      public ArgumentType Type { get; }
      [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
      public JBinding Binding { get; }
      [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
      public JAst Default { get; }

      internal JArgument(ArgumentType type, JBinding binding, JAst @default) {
        Type = type;
        Binding = binding;
        Default = @default;
        if (Type == ArgumentType.RestBlock
            || Type == ArgumentType.RestIgnore
            || Type == ArgumentType.RestKwIgnore) {
          if (Default != null) {
            throw new ArgumentException(
                "rest[kw] block|ignore arguments cannot have default values");
          }
          if (Binding != null) {
            throw new ArgumentException(
                "rest[kw] block|ignore arguments cannot have a binding");
          }
        } else if (Binding == null) {
          throw new ArgumentException("not block|ignore arguments must have a binding");
        }
        if ((Type == ArgumentType.RestCapture || Type == ArgumentType.RestKwCapture)
            && Default != null) {
          throw new ArgumentException("capture arguments cannot have a default value");
        }
      }
    }
  }

  public class JCall : JAst {
    internal JCall(
        IEnumerable<JAst> pargs,
        IEnumerable<KeyValuePair<int, JAst>> kwargs) {
      PositionalArguments = ImmutableList.CreateRange(pargs);
      KeywordArguments = ImmutableList.CreateRange(kwargs);
    }
    public override AstType Type => AstType.Call;
    public ImmutableList<JAst> PositionalArguments { get; }
    public ImmutableDictionary<int, JAst> KeywordArguments { get; }
  }

  public class JGlobal : JBinding {
    internal JGlobal(string identifier, string module) : base(identifier) {
      Module = module;
    }
    public override AstType Type => AstType.Global;
    public string Module { get; }
  }

  public class JIf : JAst {
    internal JIf(JAst condition, JAst valueIfTrue, JAst valueIfFalse) {
      Condition = condition;
      ValueIfTrue = valueIfTrue;
      ValueIfFalse = valueIfFalse;
    }
    public override AstType Type => AstType.If;
    public JAst Condition { get; }
    public JAst ValueIfTrue { get; }
    public JAst ValueIfFalse { get; }
  }

  public class JImport : JAst {
    internal JImport(
        string module,
        JBinding name,
        IEnumerable<FromImportItem> fromImports,
        string allTo) {
      Module = module;
      Name = name;
      FromImport = fromImports != null ? ImmutableList.CreateRange(fromImports);
      AllTo = allTo;
    }
    public string Module { get; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public JBinding Name { get; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public ImmutableList<FromImportItem> FromImport { get; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string AllTo { get; }

    public class FromImportItem {
      internal FromImportItem(int symbol, JBinding destination) {
        Symbol = symbol;
        Destination = destination;
      }
      public int Symbol { get; }
      public JBinding Destination { get; }
    }
  }

  public class JProgn : JAst {
    internal JProgn(IEnumerable<JAst> forms) {
      Forms = ImmutableList.CreateRange(forms);
    }
    public override AstType Type => AstType.Progn;
    public ImmutableList<JAst> Forms { get; }
  }
}
