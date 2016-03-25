using System.Collections.Generic;

namespace ZStewart.KOSLisp.Interpreter.Types {
  /// <summary>
  /// Represents a cons cell. A cons cell consists of two pointers to lisp objects, Car 
  /// and Cdr.
  /// </summary>
  public sealed class LispCons : LispList {
    // Maybe change these to assert not null.
    public LispObject Car { get; set; }
    public LispObject Cdr { get; set; }

    /// <summary>
    /// Construct a cons cell from the car and cdr.
    /// </summary>
    /// <param name="car">The element to place in the car cell.</param>
    /// <param name="cdr">The element to place in the cdr cell.</param>
    /// <returns>A cons cell containing the specified elements.</returns>
    public static LispCons Of (LispObject car, LispObject cdr) {
      return new LispCons(car, cdr);
    }

    private LispCons (LispObject car, LispObject cdr) {
      Car = car;
      Cdr = cdr;
    }

    public override bool Equals (object obj) {
      // Only equal if both objects are conses.
      if (obj is LispCons) {
        return Equals(obj as LispCons);
      }
      return false;
    }

    public bool Equals (LispCons other) {
      // Only equal if both objects contain the same elemnts.
      return Car.Equals(other.Car) && Cdr.Equals(other.Cdr);
    }

    /// <summary>
    /// Serves as a hash function for a 
    /// <see cref="ZStewart.KOSLisp.Interpreter.LispCons"/> object.
    ///
    /// Warning: This hash code is computed based on the hash of the Car and Cdr. If this 
    /// cons is part of a large data structure, this *will* traverse the entire structure.
    /// </summary>
    /// <returns>
    /// A hash code for this instance that is suitable for use in hashing algorithms and 
    /// data structures such as a hash table.
    /// </returns>
    public override int GetHashCode () {
      // TODO(zstewar1): If we ever need to actually hash Conses, we'll need to find a 
      // better way to do this.
      return Car.GetHashCode() ^ Cdr.GetHashCode();
    }
    public override string ToString () {
      if (Car.Equals(LispSymbol.Of("quote")) && LispFunctions.List1P(Cdr)) {
        return "'" + (Cdr as LispCons).Car.ToString();
      } else if (Car.Equals(LispSymbol.Of("--backquote--")) 
          && LispFunctions.List1P(Cdr)) {
        return "`" + (Cdr as LispCons).Car.ToString();
      } else if (Car.Equals(LispSymbol.Of("--unquote--")) && LispFunctions.List1P(Cdr)) {
        return "," + (Cdr as LispCons).Car.ToString();
      } else if (Car.Equals(LispSymbol.Of("--splice--")) && LispFunctions.List1P(Cdr)) {
        return ",@" + (Cdr as LispCons).Car.ToString();
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
}