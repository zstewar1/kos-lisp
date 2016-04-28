using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using ZStewart.KOSLisp.Interpreter;
using ZStewart.KOSLisp.Types.Helpers;

namespace ZStewart.KOSLisp.Types {
  /// <summary>
  /// Base class for lisp types.
  ///
  /// (this file contains the static type object and other methods).
  /// </summary>
  public partial class LispType {

    #region Static Type Setup
    private static LispType _type;
    public static LispType Type {
      get {
        // These initializers are designed to get the references correct among these
        // objects, but there's no guarantee that the properties will be correct during
        // setup.
        if (_type != null) return _type;

        // Setup the type.
        _type = new LispType {
          __name__ = "type",
          __new__ = New,
          __call__ = Call,
          _instance_type = typeof(LispType),
        };
        _type.__class__ = _type;
        _type.__bases__ = IConsType.ToLispTuple(LispObject.Object);
        _type.__mro__ = IConsType.ToLispTuple(_type, LispObject.Object);
        _type = LispType.ConfigureType(_type);
        // TODO(zstewar1): Not sure how to handle errors in "static" setup.
        if (_type == null) throw new InvalidOperationException();
        return _type;
      }
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

    #region Setup Funtionality
    public static LispType ConfigureType (LispType type) {
      if (type.__mro__ == null) {
        var mro = Linearize(type);
        // Propagate errors.
        if (mro == null) return null;
        type.__mro__ = mro;
      }
      // TODO(zstewar1): Populate dict.
      type.__dict__ = DictType.Create();
      return type;
    }

    /// <summary>
    /// Computes the linearization of the given type's inheritance heirarchy.
    /// </summary>
    /// <param name="type">The type ot linearlize.</param>
    /// <returns>The linearization of the given type.</returns>
    private static LispObject Linearize(LispType type) {
      if (type.__bases__ == NilType.Nil) {
        return IConsType.ToLispTuple(type);
      }
      var merged = MergeMroBases(type.__bases__);
      if (merged == null) return null;
      return IConsType.Create(type, merged);
    }

    private static LispObject MergeMroBases(LispObject bases) {
      var mroList = new List<List<LispType>>();
      foreach (var b in ListOperations.IterList<LispType>(bases)) {
        if (b == null) return null;
        var mro = ListOperations.IterList<LispType>(b.__mro__).ToList();
        if (mro.Any(x => x == null)) return null;
        mroList.Add(mro);
      }
      var linearization = new List<LispType>();

      while (mroList.Count > 0) {
        LispType head = null;
        foreach (var mro in mroList) {
          head = mro.First();
          // Check that head is not in the tail of any other MRO.
          if (mroList
            .Where(o => o != mro)
            .Any(o => o.Skip(1).Any(t => t == head))) {
            // Cannot add this element now.
            continue;
          }
          break;
        }
        if (head == null) {
          // Cannot Create Consistend MRO Error
          return null;
        }
        linearization.Add(head);
        for (int i = 0; i < mroList.Count; i++) {
          if (mroList[i][0] == head) mroList[i].RemoveAt(0);
          if (mroList[i].Count == 0) mroList.RemoveAt(i--);
        }
      }
      return IConsType.ToLispTuple(linearization);
    }

    public static void AddStatic(
      LispType type, string staticName, string lispName) {
      MappingOperations.SetItem(
        type.__dict__,
        SymbolType.Create(lispName),
        BuiltinFunctionType.Create(type._instance_type, staticName, lispName));
    }
    #endregion Setup Functionality

    /// <summary>
    /// Determines if the instance is of the given type.
    /// </summary>
    /// <param name="instance">The instance to look up.</param>
    /// <param name="type">The type to look for.</param>
    /// <returns>
    /// True if instance is of type type, false if it is not, and null on error.
    /// </returns>
    public static bool? IsInstance(LispObject instance, LispObject type) {
      if (!(type is LispType)) {
        LispInterpreter.SetException(ExceptionType.CreateTypeError(
          "Second argument must be a type, got {0}", type));
        return null;
      }
      foreach (var t in ListOperations.IterMro(instance)) {
        if (t == null) return null;
        if (t == type) return true;
      }
      return false;
    }

    /// <summary>
    /// Determines if one type is a subtype of another.
    /// </summary>
    /// <param name="subtype">The type to test if it is a subtype.</param>
    /// <param name="type">The parent type to test subtype against.</param>
    /// <returns>
    /// True if subtype is a subtype of parentType, false if it is not and null on error.
    /// </returns>
    public static bool? IsSubtype(LispObject subtype, LispObject parentType) {
      if (!(subtype is LispType)) {
        LispInterpreter.SetException(ExceptionType.CreateTypeError(
          "First argument must be a type, got {0}", subtype));
        return null;
      }
      if (!(parentType is LispType)) {
        LispInterpreter.SetException(ExceptionType.CreateTypeError(
          "Second argument must be a type, got {0}", parentType));
        return null;
      }
      foreach (
          var t in ListOperations.IterList<LispType>(
            ((LispType)subtype).__mro__)) {
        if (t == null) return null;
        if (t ==  parentType) return true;
      }
      return false;
    }
    #endregion Static Helper Methods
  }
}
