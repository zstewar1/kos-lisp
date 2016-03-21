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
    public static LispCons Cons(LispObject car, LispObject cdr) {
      return new LispCons(car, cdr);
    }
    private LispCons(LispObject car, LispObject cdr) {
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
    public bool Equals(LispCons other) {
      return Car.Equals(other.Car) && Cdr.Equals(other.Cdr);
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

}