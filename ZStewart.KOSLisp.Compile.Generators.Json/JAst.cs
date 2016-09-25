using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using ZStewart.KOSLisp.Types;

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
    [JsonConverter(typeof(StringEnumConverter))]
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
    internal JBinding(string identifier) {
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
        IEnumerable<Argument> args,
        JBinding name,
        bool isMacro)
        : base(forms) {
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
    public ImmutableList<Argument> Args { get; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public JBinding Name { get; }

    /// <summary>
    /// Represents an argument in a defun/defmacro/lambda.
    /// </summary>
    public class Argument {
      [JsonConverter(typeof(StringEnumConverter))]
      public ArgumentType Type { get; }
      [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
      public JBinding Binding { get; }
      [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
      public JAst Default { get; }

      internal Argument(ArgumentType type, JBinding binding, JAst @default) {
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
        JAst function,
        IEnumerable<JAst> pargs,
        IEnumerable<KeyValuePair<int, JAst>> kwargs) {
      Function = function;
      PositionalArguments = ImmutableList.CreateRange(pargs);
      KeywordArguments = ImmutableDictionary.CreateRange(kwargs);
    }
    public override AstType Type => AstType.Call;
    public JAst Function { get; }
    public ImmutableList<JAst> PositionalArguments { get; }
    public ImmutableDictionary<int, JAst> KeywordArguments { get; }
  }

  public sealed class JGlobal : JBinding {
    internal JGlobal(string identifier, int symbol) : base(identifier) {
      Symbol = symbol;
    }
    [JsonConverter(typeof(StringEnumConverter))]
    public override AstType Type => AstType.Global;
    public int Symbol { get; }
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

  public sealed class JImport : JAst {
    internal JImport(
        string module,
        JBinding name,
        IEnumerable<FromImportItem> fromImports,
        bool allImport) {
      Module = module;
      Name = name;
      FromImport = fromImports != null ? ImmutableList.CreateRange(fromImports) : null;
      AllImport = allImport;
    }
    public override AstType Type => AstType.Import;
    public string Module { get; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public JBinding Name { get; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public ImmutableList<FromImportItem> FromImport { get; }
    public bool AllImport { get; }

    public sealed class FromImportItem {
      internal FromImportItem(int symbol, JBinding destination) {
        Symbol = symbol;
        Destination = destination;
      }
      public int Symbol { get; }
      public JBinding Destination { get; }
    }
  }

  public sealed class JLet : JProgn {
    internal JLet(IEnumerable<JAst> forms, IEnumerable<LetBinding> bindings)
        : base(forms) {
      Bindings = ImmutableList.CreateRange(bindings);
    }
    public override AstType Type => AstType.Let;
    public ImmutableList<LetBinding> Bindings { get; }

    public sealed class LetBinding {
      public JBinding Binding { get; }
      [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
      public JAst InitialValue { get; }
      internal LetBinding(JBinding binding, JAst initialValue) {
        Binding = binding;
        InitialValue = initialValue;
      }
    }
  }

  public sealed class JLocal : JBinding {
    internal JLocal(string identifier) : base(identifier) {
      UniqueId = nextUniqueId++;
    }
    [JsonConverter(typeof(StringEnumConverter))]
    public override AstType Type => AstType.Local;
    public int UniqueId { get; }

    private static int nextUniqueId = 0;
  }

  public class JProgn : JAst {
    internal JProgn(IEnumerable<JAst> forms) {
      Forms = ImmutableList.CreateRange(forms);
    }
    public override AstType Type => AstType.Progn;
    public ImmutableList<JAst> Forms { get; }
  }

  public sealed class JSet : JAst {
    internal JSet(JBinding variable, JAst value) {
      Variable = variable;
      Value = value;
    }
    public override AstType Type => AstType.Set;
    public JBinding Variable { get; }
    public JAst Value { get; }
  }

  public sealed class JTry : JAst {
    internal JTry(JAst guarded, IEnumerable<CatchExpr> catches, JAst @finally) {
      Guarded = guarded;
      Catches = ImmutableList.CreateRange(catches);
      Finally = @finally;
    }
    public override AstType Type => AstType.Try;
    public JAst Guarded { get; }
    public ImmutableList<CatchExpr> Catches { get; }
    public JAst Finally { get; }

    public sealed class CatchExpr {
      internal CatchExpr(JAst exceptionType, JBinding exceptionBinding, JAst fallback) {
        ExceptionType = exceptionType;
        ExceptionBinding = exceptionBinding;
        Fallback = fallback;
      }
      [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
      public JAst ExceptionType { get; }
      [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
      public JBinding ExceptionBinding { get; }
      public JAst Fallback { get; }
    }
  }
}
