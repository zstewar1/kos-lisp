using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZStewart.KOSLisp.Types {
  /// <summary>
  /// Base class for stuff that exists within the lisp world.
  /// </summary>
  public abstract class LispObject { }

  /// <summary>
  /// Marker interface for atoms.
  /// </summary>
  public interface LispAtom { }

  /// <summary>
  /// Base class for lisp lists.
  /// </summary>
  public abstract class LispList : LispObject {
    public abstract LispObject Car { get; }
    public abstract LispObject Cdr { get; }
  }

  /// <summary>
  /// Cons cells.
  /// </summary>
  public sealed class LispCons : LispList {
    private readonly LispObject car;
    private readonly LispObject cdr;

    public static LispCons Of (LispObject car, LispObject cdr) {
      return new LispCons(car, cdr);
    }

    private LispCons (LispObject car, LispObject cdr) {
      this.car = car;
      this.cdr = cdr;
    }

    public override LispObject Car { get { return car; } }
    public override LispObject Cdr { get { return cdr; } }

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
    /// Serves as a hash function for a <see cref="ZStewart.KOSLisp.Types.LispCons"/> object.
    ///
    /// Warning: This hash code is computed based on the hash of the Car and Cdr. If this cons is 
    /// part of a large data structure, this *will* traverse the entire structure.
    /// </summary>
    /// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a hash table.</returns>
    public override int GetHashCode () {
      // TODO(zstewar1): If we ever need to actually hash Conses, we'll need to find a better way to do this.
      return Car.GetHashCode() ^ Cdr.GetHashCode();
    }
  }

  /// <summary>
  /// The nil type -- both an atom and a list.
  /// </summary>
  public sealed class LispNil : LispList, LispAtom {
    private static readonly LispNil nil = new LispNil();
    public static LispNil Nil { get { return nil; } }
    private LispNil () { }
    public override LispObject Car { get { return nil; } }
    public override LispObject Cdr { get { return nil; } }
  }

  /// <summary>
  /// Represents a symbol in lisp.
  /// </summary>
  public sealed class LispSymbol : LispObject, LispAtom {
    private string Identifier { get; }
    private LispSymbol(string identifier) { Identifier = identifier; }
    public static LispSymbol Of(string identifier) { 
      return new LispSymbol(Preconditions.CheckNotNullOrEmpty(identifier)); 
    }
  }

  /// <summary>
  /// A lisp string.
  /// </summary>
  public sealed class LispString : LispObject, LispAtom {
    private string Value { get; }
    private LispString(string value) { Value = value; }
    public static LispString Of(string value) {
      return new LispString(Preconditions.CheckNotNull(value)); 
    }
  }

  /// <summary>
  /// A lisp float.
  /// </summary>
  public sealed class LispFloat : LispObject, LispAtom {
    private double Value { get; }
    private LispFloat(double value) { Value = value; }
    public static LispFloat Of(double value) { return new LispFloat(value); }
  }

  /// <summary>
  /// A lisp integer.
  /// </summary>
  public sealed class LispInt : LispObject, LispAtom {
    private long Value { get; }
    private LispInt(long value) { Value = value; }
    public static LispInt Of(long value) { return new LispInt(value); }
  }

  /// <summary>
  /// The value True.
  /// </summary>
  public sealed class LispT : LispObject, LispAtom {
    private static readonly LispT t = new LispT();
    public static LispT T { get { return t; } }
    private LispT () { }
  }
}