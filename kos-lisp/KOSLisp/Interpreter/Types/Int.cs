using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZStewart.KOSLisp.Interpreter.Types {
  /// <summary>
  /// Represents an integer in lisp.
  /// </summary>
  public sealed class LispInt : LispAtom {
    /// <summary>
    /// Static creation function for lisp integers.
    /// </summary>
    /// <param name="value">The value of the created int.</param>
    /// <returns>A created int with the specified value.</returns>
    public static LispInt Of(long value) { return new LispInt(value); }

    /// <summary>
    /// The numeric value of this integer.
    /// </summary>
    public long Value { get; }

    /// <summary>
    /// Constructs a new integer with the specified value.
    /// </summary>
    /// <param name="value">The value to store in this integer.</param>
    private LispInt(long value) { Value = value; }

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
}
