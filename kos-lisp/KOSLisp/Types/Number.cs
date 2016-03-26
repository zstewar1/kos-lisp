namespace ZStewart.KOSLisp.Types {
  /// <summary>
  /// Represents a floating point number in lisp.
  /// </summary>
  public sealed class LispNumber : LispAtom {
    /// <summary>
    /// Creator fuction for floats. (Using a creator function allows us the possibility of
    /// more control over construction, e.g. caching).
    /// </summary>
    /// <param name="value">The value of this double.</param>
    /// <returns></returns>
    public static LispNumber Of(double value) { return new LispNumber(value); }

    /// <summary>
    /// The read-only value of this float.
    /// </summary>
    public double Value { get; }

    /// <summary>
    /// Constructs a float with a fixed value.
    /// </summary>
    /// <param name="value">The value this float should have.</param>
    private LispNumber(double value) { Value = value; }

    public override bool Equals (object obj) {
      if (obj is LispNumber) {
        return Value == (obj as LispNumber).Value;
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
