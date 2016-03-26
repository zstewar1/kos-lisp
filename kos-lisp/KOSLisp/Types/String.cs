using System.Globalization;
using System.Text;

namespace ZStewart.KOSLisp.Types {
  /// <summary>
  /// A lisp string. These are always case insensitive because Kerboscript is 
  /// case-insensitive.
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
      // Convert to a display string by converting any escaped values back to the escape 
      // sequence, then wrap the result in quotes.
      StringBuilder sb = new StringBuilder(Value);
      sb.Replace("\n", "\\n");
      sb.Replace("\r", "\\r");
      sb.Replace("\"", "\\\"");
      sb.Insert(0, "\"");
      sb.Append("\"");
      return sb.ToString();
    }
  }
}
