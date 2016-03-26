using System.Collections.Generic;

namespace ZStewart.KOSLisp.Interpreter.Types {
  /// <summary>
  /// Represents a function which can be called with an argument list. This will typically
  /// be the cons of all of the arguments passed to it, though it may also be nil for zero
  /// argument functions.
  /// </summary>
  public interface LispFunction : LispObject {
    /// <summary>
    /// Call this function with the given arguments.
    /// </summary>
    /// <param name="Args">The arguments to the function.</param>
    /// <returns></returns>
    LispObject Call (LispObject Args);
  }
}
