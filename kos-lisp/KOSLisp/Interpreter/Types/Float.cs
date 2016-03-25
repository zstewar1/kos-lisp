using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZStewart.KOSLisp.Interpreter.Types {
  /// <summary>
  /// Represents a floating point number in lisp.
  /// </summary>
  public sealed class LispFloat : LispAtom {
    /// <summary>
    /// Creator fuction for floats. (Using a creator function allows us the possibility of
    /// more control over construction, e.g. caching).
    /// </summary>
    /// <param name="value">The value of this double.</param>
    /// <returns></returns>
    public static LispFloat Of(double value) { return new LispFloat(value); }

    /// <summary>
    /// The read-only value of this float.
    /// </summary>
    public double Value { get; }

    /// <summary>
    /// Constructs a float with a fixed value.
    /// </summary>
    /// <param name="value">The value this float should have.</param>
    private LispFloat(double value) { Value = value; }

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
}
