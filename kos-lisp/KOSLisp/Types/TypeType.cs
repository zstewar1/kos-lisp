using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZStewart.KOSLisp.Types {
  public static class TypeType {

    #region Static Type Setup
    public static readonly LispTypeObject Type = new LispTypeObject();

    static TypeType () {
      // The static initializers for the builtin types Type, Object, ICons, and Nil are
      // all interdependent and contain references which will cause the others to be 
      // triggered to make sure these work correctly, these types are required to set all
      // static references before assigning any properties.
      //
      // This ensures that when they reference each other's global statics they alway get
      // the correct reference. The references will be updated later with actual values.
      //
      // If the type references are not done this way, problems can occur. Example:
      // Client code references NilType.Nil. This triggers the static initializer of
      // NilType. However the construction of NilType.NilClass and NilType.Nil depend on
      // ObjectType.Object and TypeType.Type, so they trigger those static initializers.
      // Those initializers in turn depend on ICons, which depends on NilType and Nil. If 
      // the references in global static variables were not assigned initially, this could
      // result in one or more of these initializer picking up a null reference instead of
      // the object. 
      //
      // To avoid this, these core types must assign their global static references to an
      // empty object, then assign the properties of that reference in the static
      // initializer. This could also be considered best practice for all types' static
      // initializers.
      Type.__name__ = "type";
      Type.__class__ = Type;
      Type.__bases__ = IConsType.Create(ObjectType.Object, NilType.Nil);
      Type.__new__ = New;
      Type.__call__ = Call;
    }

    private static LispObject New(LispObject subtype, LispObject args) {
      return new LispObject {
        __class__ = Type,
      };
    }

    private static LispObject Call(LispObject receiver, LispObject args) {
      // TODO(zstewar1): Calling a type calls new then maybe init.
      return null;
    }
    #endregion Static Type Setup

    #region Static Helper Methods
    /// <summary>
    /// Determines if the instance is of the given type.
    /// </summary>
    /// <param name="instance">The instance to look up.</param>
    /// <param name="type">The type to look for.</param>
    /// <returns>
    /// True if instance is of type type, false if it is not, and null on error.
    /// </returns>
    public static bool? IsInstance(LispObject instance, LispObject type) {
      return null;
    }
    #endregion Static Helper Methods
  }
}
