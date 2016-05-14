using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using ZStewart.KOSLisp.Types.Attributes;
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
          __call__ = Call,
          _instance_type = typeof(LispType),
        };
        _type.__class__ = _type;
        _type.__bases__ = IConsType.ToLispTuple(LispObject.Object);
        _type.__mro__ = IConsType.ToLispTuple(_type, LispObject.Object);
        LispType.ConfigureType(_type);

        return _type;
      }
    }

    [BuiltinFunction(Name = "--new--")]
    private static LispObject New(
        [Required] LispObject subtype,
        [RestIgnore] byte ri, [RestKwIgnore] byte rki) {
      throw ExceptionType.ThrowNotImplemented("can't create new types yet");
    }

    [BuiltinFunction(Name = "--init--")]
    private static LispObject Init([RestIgnore] byte ri, [RestKwIgnore] byte rki) {
      throw ExceptionType.ThrowNotImplemented("can't init new types yet");
    }


    private static LispObject Call(
        LispObject instance,
        List<LispObject> pargs,
        Dictionary<SymbolType, LispObject> kwargs) {
      if (!(instance is LispType)) {
        throw ExceptionType.ThrowTypeError(
          "type must be a lisp type, got {0} object", instance.__class__);
      }
      var type = (LispType)instance;
      // Since we're calling new as a staticmethod, we need to insert the extra arguments
      // (the type) into the argument list.
      var newPargs = new List<LispObject>(pargs.Count + 1);
      newPargs.Add(type);
      newPargs.AddRange(pargs);
      var created = LispObject.Call(type, PropConsts.New, newPargs, kwargs);
      if (IsInstance(created, type)) {
        LispObject.Call(created, PropConsts.Init, pargs, kwargs);
      }
      return created;
    }

    [BuiltinFunction(Name = "--repr--")]
    private static LispObject ToRepr([Required] LispType type) {
      return StringType.Format("[type {0}]", type.__name__);
    }
    #endregion Static Type Setup

    #region Static Helper Methods

    #region Setup Funtionality
    public static void ConfigureType (LispType type) {
      if (type.__mro__ == null) {
        type.__mro__ = Linearize(type);
      }
      // TODO(zstewar1): Populate dict.
      type.__dict__ = DictType.Create();

      // If the _instance_type is not set, then its value is inherited from the parent
      // type, and we should not try to load methods into this type, since it will also
      // inherit them from the parent.
      if (type._instance_type != null) {
        foreach (var method in type._instance_type.GetMethods(
            BindingFlags.NonPublic | BindingFlags.Static)) {
          var builtin = method.GetCustomAttribute<BuiltinFunctionAttribute>();
          if (builtin != null) {
            var name = SymbolType.Create(builtin.Name ?? method.Name);
            MappingOperations.SetItem(
              type.__dict__, name, BuiltinFunctionType.Create(method, name));
          }
        }
      }
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
      return IConsType.Create(type, MergeMroBases(type.__bases__));
    }

    private static LispObject MergeMroBases(LispObject bases) {
      var mroList = new List<List<LispType>>();
      foreach (var b in ListOperations.IterList<LispType>(bases)) {
        mroList.Add(ListOperations.IterList<LispType>(b.__mro__).ToList());
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
          throw ExceptionType.ThrowTypeError("Cannot create consistent MRO");
        }
        linearization.Add(head);
        for (int i = 0; i < mroList.Count; i++) {
          if (mroList[i][0] == head) mroList[i].RemoveAt(0);
          if (mroList[i].Count == 0) mroList.RemoveAt(i--);
        }
      }
      return IConsType.ToLispTuple(linearization);
    }

    /// <summary>
    /// Take a static function with appropriate positional/keyword argument attributes
    /// from the given type._instance_type, and add a BuiltinFunction which wraps that
    /// function to the type's dictionary with the given lisp name.
    /// </summary>
    public static void AddStatic(
        LispType type, string staticName, string lispName) {
      AddStatic(type, staticName, SymbolType.Create(lispName));
    }

    /// <summary>
    /// Take a static function with appropriate positional/keyword argument attributes
    /// from the given type._instance_type, and add a BuiltinFunction which wraps that
    /// function to the type's dictionary with the given lisp name.
    /// </summary>
    public static void AddStatic(
        LispType type, string staticName, SymbolType lispName) {
      AddStatic(type._instance_type, type, staticName, lispName);
    }

    /// <summary>
    /// Add a static function to the given type from an explicitly specified source type.
    /// </summary>
    public static void AddStatic(
        Type sourceType, LispType destTypeObject, string staticName, string lispName) {
      AddStatic(sourceType, destTypeObject, staticName, SymbolType.Create(lispName));
    }

    /// <summary>
    /// Add a static function to the given type from an explicitly specified source type.
    /// </summary>
    public static void AddStatic(
        Type sourceType,
        LispType destTypeObject,
        string staticName,
        SymbolType lispName) {
      MappingOperations.SetItem(
        destTypeObject.__dict__,
        lispName,
        BuiltinFunctionType.Create(sourceType, staticName, lispName));
    }

    public static void AddDataProperty(
        LispType type, string propName, SymbolType lispName) {
      MappingOperations.SetItem(
        type.__dict__,
        lispName,
        BuiltinDataDescriptorType.Create(type._instance_type, propName, lispName));
    }
    #endregion Setup Functionality

    /// <summary>
    /// Determines if the instance is of the given type.
    /// </summary>
    /// <param name="instance">The instance to look up.</param>
    /// <param name="type">The type to look for.</param>
    /// <returns>
    /// True if instance is of type type, false if it is not
    /// </returns>
    public static bool IsInstance(
        [Required] LispObject instance,
        [Required] LispObject type) {
      if (!(type is LispType)) {
        throw ExceptionType.ThrowTypeError(
          "Second argument must be a type, got {0}", type);
      }
      foreach (var t in ListOperations.IterMro(instance)) {
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
    /// True if subtype is a subtype of parentType, false if it is not.
    /// </returns>
    public static bool IsSubtype(
        [Required] LispType subtype,
        [Required] LispType parentType) {
      foreach (var t in ListOperations.IterList<LispType>(subtype.__mro__)) {
        if (t ==  parentType) return true;
      }
      return false;
    }
    #endregion Static Helper Methods
  }
}
