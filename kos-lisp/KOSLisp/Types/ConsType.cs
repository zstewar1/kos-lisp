namespace ZStewart.KOSLisp.Types {
  /// <summary>
  /// Represents the lisp cons type.
  /// </summary>
  public class ConsType : LispObject {
    protected ConsType () { }

    // Configuration for the static type object that represents this type.
    #region Static Type Setup
    /// <summary>
    /// The singleton instance that represents the type "cons"
    /// </summary>
    public static readonly LispTypeObject Cons = new LispTypeObject();

    /// <summary>
    /// Prepares the cons type object by filling out its fields with appropriate values 
    /// and methods.
    /// </summary>
    static ConsType () {
      Cons.__name__ = "cons";
      Cons.__class__ = TypeType.Type;
      Cons.__bases__ = IConsType.Create(ObjectType.Object, NilType.Nil);
      Cons.__new__ = New;
    }

    private static LispObject New (LispObject subtype, LispObject args) {
      // TODO(zstewar1)
      return null;
    }
    #endregion Static Type Setup

    // Methods to help other C# code interface with the cons type. These bypass the need
    // to retrieve the type object and call its __new__ method with appropriate arguments.
    #region Static Helper Methods
    public static ConsType Create(LispObject car, LispObject cdr) {
      return new ConsType {
        __class__ = Cons,
        Car = car,
        Cdr = cdr,
      };
    }
    #endregion

    /// <summary>
    /// The first element of the cons. In a list this is the pointer to the contents.
    /// </summary>
    public LispObject Car;
    /// <summary>
    /// The second element of the cons. In a list this is the pointer to the next cons.
    /// </summary>
    public LispObject Cdr;
  }
}
