namespace ZStewart.KOSLisp.Interpreter.Types {
  /// <summary>
  /// Marker interface to indicate that a type is a lisp Atom. Atoms are anything that is
  /// not a list.
  /// </summary>
  public interface LispAtom : LispObject { }
}
