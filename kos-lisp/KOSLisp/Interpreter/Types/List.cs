namespace ZStewart.KOSLisp.Interpreter.Types {
  /// <summary>
  /// Interface for a lisp list. Represents a list as a pair -- Car (the current element)
  /// and Cdr (a pointer to the next pair).
  /// </summary>
  public interface LispList : LispObject {
    LispObject Car { get; set; } 
    LispObject Cdr { get; set; }
  }
}
