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
          _instance_type = typeof(BuiltinDataDescriptorType),
        };
        _builtinDataDescriptor.__class__ = LispType.Type;
        _builtinDataDescriptor.__bases__ = IConsType.ToLispTuple(LispObject.Object);
        _builtinDataDescriptor.__mro__ = IConsType.ToLispTuple(
          _builtinDataDescriptor, LispObject.Object);
        _builtinDataDescriptor = LispType.ConfigureType(_builtinDataDescriptor);
        if (_builtinDataDescriptor == null) throw new InvalidOperationException();

        LispType.AddStatic(_builtinDataDescriptor, "GetStatic", "--get--");
        LispType.AddStatic(_builtinDataDescriptor, "SetStatic", "--set--");

        return _builtinDataDescriptor;
      }
    }

    private static LispObject GetStatic(
        [PositionalArgument] BuiltinDataDescriptorType self,
        [PositionalArgument] LispObject instance,
        [PositionalArgument] LispType type) {
      return self.Get(instance, type);
    }

    private static LispObject SetStatic(
        [PositionalArgument] BuiltinDataDescriptorType self,
        [PositionalArgument] LispObject instance,
        [PositionalArgument] LispObject value) {
      return self.Set(instance, value);
    }
    #endregion

    protected BuiltinDataDescriptorType(PropertyInfo property, string name) {
      this.property = property;
      Name = name;
    }

    public string Name { get; }
    private Type ObjectType { get { return property.DeclaringType; } }
    private Type PropertyType { get { return property.PropertyType; } }
    private readonly PropertyInfo property;

    private LispObject Get (LispObject instance, LispType type) {
      if (instance == NilType.Nil && type != NilType.NilClass) {
        return this;
      } else if (!ObjectType.IsAssignableFrom(instance.GetType())) {
        LispInterpreter.SetException(ExceptionType.CreateTypeError(
          "builtin property \"{0}\" does not apply to \"{1}\" objects",
          Name, instance.__class__));
        return null;
      } else if (!property.CanRead) {
        LispInterpreter.SetException(ExceptionType.CreateTypeError(
          "can't get property \"{0}\" of \"{1}\"", Name, instance.__class__));
        return null;
      } else {
        return Arguments.Unmarshal(property.GetValue(instance));
      }
    }

    private LispObject Set (LispObject instance, LispObject value) {
      if (!ObjectType.IsAssignableFrom(instance.GetType())) {
        LispInterpreter.SetException(ExceptionType.CreateTypeError(
          "builtin property \"{0}\" does not apply to \"{1}\" objects",
          Name, instance.__class__));
        return null;
      } else if (!property.CanWrite) {
        LispInterpreter.SetException(ExceptionType.CreateTypeError(
          "can't set property \"{0}\" of \"{1}\"", Name, instance.__class__));
        return null;
      } else {
        object marshaled;
        if (!Arguments.Marshal(PropertyType, value, out marshaled)) return null;
        property.SetValue(instance, marshaled);
        return NilType.Nil;
      }
    }
  }
}
