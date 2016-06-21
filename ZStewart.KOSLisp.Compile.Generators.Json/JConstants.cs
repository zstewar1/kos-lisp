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
  public interface JConstant {
    /// <summary>
    /// The type of this constant (for deserialization).
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    ConstantType Type { get; }
  }

  /// <summary>
  /// A reference to a constant.
  /// </summary>
  public struct JConstantReference {
    internal JConstantReference(int id, JProgram arena) {
      Id = id;
      Arena = arena;
    }

    /// <summary>
    /// The integer that identifies this constant in the program.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// The JProgram where the constant that this refers to resides.
    /// </summary>
    [JsonIgnore]
    public JProgram Arena { get; }

    [JsonIgnore]
    public JConstant Value => Arena.Dereference(this);

    public override bool Equals(object obj) {
      if (!(obj is JConstantReference)) return false;
      var jcr = (JConstantReference)obj;
      return jcr.Arena == Arena && jcr.Id == Id;
    }

    // Just use ID because references from different arenas should not be stored
    // together in the same hash table.
    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator==(
        JConstantReference left, JConstantReference right) => left.Equals(right);

    public static bool operator!=(
        JConstantReference left, JConstantReference right) => !left.Equals(right);

    public static JConstant operator~(JConstantReference reference) => reference.Value;

  }

  public sealed class JBool : JConstant {
    internal JBool(bool value) {
      Value = value;
    }
    public ConstantType Type => ConstantType.Bool;
    public bool Value { get; }
  }

  public sealed class JCons : JConstant {
    internal JCons(JConstantReference car, JConstantReference cdr) {
      Car = car;
      Cdr = cdr;
    }
    public ConstantType Type => ConstantType.Cons;
    public JConstantReference Car { get; }
    public JConstantReference Cdr { get; }
  }

  public sealed class JKeyword : JConstant {
    internal JKeyword(string identifier) {
      Identifier = identifier;
    }
    public ConstantType Type => ConstantType.Keyword;
    public string Identifier { get; }
  }

  public sealed class JNil : JConstant {
    internal JNil() {}
    public ConstantType Type => ConstantType.Nil;
  }

  public sealed class JNumber : JConstant {
    internal JNumber(double value) {
      Value = value;
    }
    public ConstantType Type => ConstantType.Number;
    public double Value { get; }
  }

  public sealed class JString : JConstant {
    internal JString(string value) {
      Value = value;
    }
    public ConstantType Type => ConstantType.String;
    public string Value { get; }
  }

  public sealed class JSymbol : JConstant {
    internal JSymbol(string identifier) {
      Identifier = identifier;
    }
    public ConstantType Type => ConstantType.Symbol;
    public string Identifier { get; }
  }
}
