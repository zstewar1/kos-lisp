using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ZStewart.KOSLisp.Compile.Generators.Json {
  public enum ConstantType {
    Bool,
    Cons,
    Keyword,
    Nil,
    Number,
    String,
    Symbol,
  }

  /// <summary>
  /// Interface represents a constant that can be referenced from the program's ast code
  /// and which should have the same reference anywhere it is referenced.
  /// </summary>
  public abstract class JConstant {
    /// <summary>
    /// The type of this constant.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public abstract ConstantType Type { get; }

    internal JConstant() {}
  }

  public sealed class JBool : JConstant {
    internal JBool(bool value) {
      Value = value;
    }
    public override ConstantType Type => ConstantType.Bool;
    public bool Value { get; }
  }

  public sealed class JCons : JConstant {
    internal JCons(int car, int cdr) {
      Car = car;
      Cdr = cdr;
    }
    public override ConstantType Type => ConstantType.Cons;
    public int Car { get; }
    public int Cdr { get; }
  }

  public sealed class JKeyword : JConstant {
    internal JKeyword(string identifier) {
      Identifier = identifier;
    }
    public override ConstantType Type => ConstantType.Keyword;
    public string Identifier { get; }
  }

  public sealed class JNil : JConstant {
    internal JNil() {}
    public override ConstantType Type => ConstantType.Nil;
  }

  public sealed class JNumber : JConstant {
    internal JNumber(double value) {
      Value = value;
    }
    public override ConstantType Type => ConstantType.Number;
    public double Value { get; }
  }

  public sealed class JString : JConstant {
    internal JString(string value) {
      Value = value;
    }
    public override ConstantType Type => ConstantType.String;
    public string Value { get; }
  }

  public sealed class JSymbol : JConstant {
    internal JSymbol(string identifier) {
      Identifier = identifier;
    }
    public override ConstantType Type => ConstantType.Symbol;
    public string Identifier { get; }
  }
}
