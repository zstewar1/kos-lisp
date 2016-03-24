using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ZStewart.KOSLisp.Interpreter {
  /// <summary>
  /// Base class for stuff that exists within the lisp world.
  /// </summary>
  public interface LispObject { }

  /// <summary>
  /// Marker interface for atoms.
  /// </summary>
  public interface LispAtom : LispObject { }

  /// <summary>
  /// Base class for lisp lists.
  /// </summary>
  public interface LispList : LispObject {
    LispObject Car { get; set; }
    LispObject Cdr { get; set; }
  }

  /// <summary>
  /// Cons cells.
  /// </summary>
  public sealed class LispCons : LispList {
    // Maybe change these to assert not null.
    public LispObject Car { get; set; }
    public LispObject Cdr { get; set; }

    public static LispCons Of (LispObject car, LispObject cdr) {
      return new LispCons(car, cdr);
    }

    private LispCons (LispObject car, LispObject cdr) {
      Car = car;
      Cdr = cdr;
   } 

    public override bool Equals (object obj) {
      if (obj is LispCons) {
        return Equals(obj as LispCons);
      }
      return false;
    }

    public bool Equals (LispCons other) {
      return Car.Equals(other.Car) && Cdr.Equals(other.Cdr);
    }

    /// <summary>
    /// Serves as a hash function for a <see cref="ZStewart.KOSLisp.Interpreter.LispCons"/> object.
    ///
    /// Warning: This hash code is computed based on the hash of the Car and Cdr. If this cons is 
    /// part of a large data structure, this *will* traverse the entire structure.
    /// </summary>
    /// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a hash table.</returns>
    public override int GetHashCode () {
      // TODO(zstewar1): If we ever need to actually hash Conses, we'll need to find a better way to do this.
      return Car.GetHashCode() ^ Cdr.GetHashCode();
    }

    public override string ToString () {
      if (Car.Equals(LispSymbol.Of("quote")) && Cdr is LispCons && (Cdr as LispCons).Cdr == LispNil.Nil) {
        return "'" + (Cdr as LispCons).Car.ToString();
      } else {
        List<string> elements = new List<string>();
        LispObject current = this;
        while (current is LispCons) {
          elements.Add((current as LispCons).Car.ToString());
          current = (current as LispCons).Cdr;
        }
        if (current != LispNil.Nil) {
          elements.Add(".");
          elements.Add(current.ToString());
        }
        return string.Format("({0})", string.Join(" ", elements));
      }
    }
  }

  /// <summary>
  /// The nil type -- both an atom, a symbol, and a list.
  /// </summary>
  public sealed class LispNil : LispSymbol, LispList {
    private static readonly LispNil nil = new LispNil();
    public static LispNil Nil { get { return nil; } }
    private LispNil () : base("nil") { }
    public LispObject Car {
      get { return nil; }
      set { throw new InvalidOperationException("Cannot set Car of nil"); }
    }
    public LispObject Cdr {
      get { return nil; }
      set { throw new InvalidOperationException("Cannot set Cdr of nil"); }
    }
  }

  /// <summary>
  /// Represents a symbol in lisp.
  /// </summary>
  public class LispSymbol : LispAtom {
    private static readonly IDictionary<string, LispSymbol> existingSymbols =
      new Dictionary<string, LispSymbol>();
    private string Identifier { get; }
    protected LispSymbol(string identifier) {
      // Always check if this symbol exists, then added it if it doesn't.
      // Throw if it does to prevent copied symbols.
      identifier = Preconditions.CheckNotNullOrEmpty(identifier).ToUpperInvariant();
      if (existingSymbols.ContainsKey(identifier)) {
        throw new DuplicatedSymbolException(
          string.Format("Duplicate definition of symbol {0}", identifier));
      }
      existingSymbols.Add(identifier, this);
      Identifier = identifier;
    }
    public static LispSymbol Of(string identifier) {
      identifier = Preconditions.CheckNotNullOrEmpty(identifier).ToUpperInvariant();
      LispSymbol symb;
      if (existingSymbols.TryGetValue(identifier, out symb)) {
        return symb;
      } else {
        return new LispSymbol(identifier);
      }
    }

    public override string ToString () {
      return Identifier;
    }
  }

  /// <summary>
  /// A lisp string.
  /// </summary>
  public sealed class LispString : LispAtom {
    public string Value { get; }
    private LispString(string value) { Value = value; }
    public static LispString Of(string value) {
      return new LispString(Preconditions.CheckNotNull(value)); 
    }
    public override bool Equals (object obj) {
      var other = obj as LispString;
      if (other == null) return false;
      return string.Compare(Value, other.Value, true, CultureInfo.InvariantCulture) == 0;
    }
    public override int GetHashCode () {
      return Value.ToUpperInvariant().GetHashCode();
    }

    public override string ToString () {
      return string.Format(
        "\"{0}\"",
        Value.Replace("\n", "\\n")
          .Replace("\r", "\\r")
          .Replace("\"", "\\\""));
    }
  }

  /// <summary>
  /// A lisp float.
  /// </summary>
  public sealed class LispFloat : LispAtom {
    public double Value { get; }
    private LispFloat(double value) { Value = value; }
    public static LispFloat Of(double value) { return new LispFloat(value); }
    public override bool Equals (object obj) {
      if (obj is LispFloat) {
        return Value == (obj as LispFloat).Value;
      } else if (obj is LispInt) {
        return Value == (obj as LispInt).Value;
      } else {
        return false;
      }
    }
    public override int GetHashCode () {
      return Value.GetHashCode();
    }
    public override string ToString () {
      return Value.ToString();
    }
  }

  /// <summary>
  /// A lisp integer.
  /// </summary>
  public sealed class LispInt : LispAtom {
    public long Value { get; }
    private LispInt(long value) { Value = value; }
    public static LispInt Of(long value) { return new LispInt(value); }
    public override bool Equals (object obj) {
      if (obj is LispInt) {
        return Value == (obj as LispInt).Value;
      } else if (obj is LispFloat) {
        return Value == (obj as LispFloat).Value;
      } else {
        return false;
      }
    }
    public override int GetHashCode () {
      return Value.GetHashCode();
    }
    public override string ToString () {
      return Value.ToString();
    }
  }

  /// <summary>
  /// The value True.
  /// </summary>
  public sealed class LispT : LispSymbol {
    private static readonly LispT t = new LispT();
    public static LispT T { get { return t; } }
    private LispT () : base("t") { }
  }
}