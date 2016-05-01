using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using ZStewart.KOSLisp.Interpreter;
using ZStewart.KOSLisp.Types.Attributes;
using ZStewart.KOSLisp.Types.Helpers;

namespace ZStewart.KOSLisp.Types {
  public class BuiltinDataDescriptorType : LispObject {
    #region Static Type Setup
    private static LispType _builtinDataDescriptor;
    public static LispType BuiltinDataDescriptor {
      get {
        if (_builtinDataDescriptor != null) return _builtinDataDescriptor;

        _builtinDataDescriptor = new LispType {
          __name__ = "BuiltinDataDescriptor",
          __get__ = GetStatic,
          __set__ = SetStatic,
          _instance_type = typeof(BuiltinDataDescriptorType),
        };
        _builtinDataDescriptor.__class__ = LispType.Type;
        _builtinDataDescriptor.__bases__ = IConsType.ToLispTuple(LispObject.Object);
        _builtinDataDescriptor.__mro__ = IConsType.ToLispTuple(
          _builtinDataDescriptor, LispObject.Object);
        _builtinDataDescriptor = LispType.ConfigureType(_builtinDataDescriptor);
        if (_builtinDataDescriptor == null) throw new InvalidOperationException();

        LispType.AddDataProperty(_builtinDataDescriptor, "Name", PropConsts.Name);

        return _builtinDataDescriptor;
      }
    }

    private static LispObject GetStatic(
        LispObject self,
        LispObject instance,
        LispObject type) {
      if (!(self is BuiltinDataDescriptorType)) {
        throw ExceptionType.ThrowTypeError("self must be a BuiltinDataDescriptor");
      }
      return ((BuiltinDataDescriptorType)self).Get(instance, type);
    }

    private static LispObject SetStatic(
        LispObject self,
        LispObject instance,
        LispObject value) {
      if (!(self is BuiltinDataDescriptorType)) {
        throw ExceptionType.ThrowTypeError("self must be a BuiltinDataDescriptor");
      }
      return ((BuiltinDataDescriptorType)self).Set(instance, value);
    }
    #endregion Static Type Setup

    #region Static Helper Methods
    public static BuiltinDataDescriptorType Create<T>(
        string propName, SymbolType lispName) {
      return Create(typeof(T), propName, lispName);
    }

    public static BuiltinDataDescriptorType Create(
        Type type, string propName, SymbolType lispName) {
      return new BuiltinDataDescriptorType(
        type.GetProperty(
          propName,
          BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance |
          BindingFlags.FlattenHierarchy),
        lispName);
    }
    #endregion Static Helper Methods

    protected BuiltinDataDescriptorType(PropertyInfo property, SymbolType name) {
      this.property = property;
      Name = name;
    }

    public SymbolType Name { get; }
    private Type ObjectType { get { return property.DeclaringType; } }
    private Type PropertyType { get { return property.PropertyType; } }
    private readonly PropertyInfo property;

    private LispObject Get (LispObject instance, LispObject type) {
      if (!(type is LispType) && type != NilType.Nil) {
        throw ExceptionType.ThrowTypeError("type must be a type or nil");
      } else if (instance == NilType.Nil && type != NilType.NilClass) {
        return this;
      } else if (!ObjectType.IsAssignableFrom(instance.GetType())) {
        throw ExceptionType.ThrowTypeError(
          "builtin property \"{0}\" does not apply to \"{1}\" objects",
          Name, instance.__class__);
      } else if (!property.CanRead) {
        throw ExceptionType.ThrowTypeError(
          "can't get property \"{0}\" of \"{1}\"", Name, instance.__class__);
      } else {
        return Arguments.Unmarshal(property.GetValue(instance));
      }
    }

    private LispObject Set (LispObject instance, LispObject value) {
      if (!ObjectType.IsAssignableFrom(instance.GetType())) {
        throw ExceptionType.ThrowTypeError(
          "builtin property \"{0}\" does not apply to \"{1}\" objects",
          Name, instance.__class__);
      } else if (!property.CanWrite) {
        throw ExceptionType.ThrowTypeError(
          "can't set property \"{0}\" of \"{1}\"", Name, instance.__class__);
      } else {
        property.SetValue(instance, Arguments.Marshal(PropertyType, value));
        return NilType.Nil;
      }
    }
  }
}
