namespace ZStewart.KOSLisp.Types {
  /// <summary>
  /// Base class for things that exist in lisp.
  /// </summary>
  public class LispObject {
    /// <summary>
    /// Pointer to the type object for this class.
    /// </summary>
    public LispTypeObject __class__;

    /// <summary>
    /// Lisp-accessable dictionary of attributes of this type. Dict or Dict-like.
    /// (we would only create a slot for this if this was a dynamic type, like Python
    /// probably maybe does, but we can't dynamically choose the size of 
    /// objects/structures like C can, and this needs to be avialable in all types if they
    /// can be subclassed from the dynamic language).
    /// 
    /// May not be initialized in all types.
    /// </summary>
    public LispObject __dict__;
  }
}
