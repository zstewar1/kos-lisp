using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;

using static ZStewart.KOSLisp.Types.ExceptionType;

using ZStewart.KOSLisp.Types;

namespace ZStewart.KOSLisp.Compile.Generators.Json {
  /// <summary>
  /// Encompasses an entire kerbolisp program encoded in JSON.
  /// </summary>
  public sealed class JProgram {
    /// <summary>
    /// Storage for all constants used in the program. In the tree, these will appear as
    /// JConstantReference containing the integer id of the constant.
    /// </summary>
    [JsonProperty]
    private List<JConstant> ReferencedConstants { get; set; } = new List<JConstant>();

    /// <summary>
    /// A dictionary used when lookup up reference constants when building the program's
    /// syntax tree.
    /// </summary>
    private Dictionary<LispObject, JConstantReference> reverseConstantLookup =
      new Dictionary<LispObject, JConstantReference>();

    /// <summary>
    /// Gets a reference to the converted form of the given lisp object.
    ///
    /// If the object is already in the constant list, returns an existing reference to
    /// it.
    ///
    /// Otherwise, creates a new JConstant, dedupes it against the list of existing
    /// constants, adds it to the constants and returns a reference to it.
    /// </summary>
    public JConstantReference ReferTo(LispObject obj) {
      JConstantReference reference;
      if (reverseConstantLookup.TryGetValue(obj, out reference)) {
        return reference;
      } else {
        var newConstant = ConvertConstant(obj);
        var nextId = ReferencedConstants.Count;
        ReferencedConstants.Add(newConstant);
        var newReference = new JConstantReference(nextId, this);
        reverseConstantLookup.Add(obj, newReference);
        return newReference;
      }
    }

    /// <summary>
    /// Gets the JConstant referred to by the reference.
    /// </summary>
    public JConstant Dereference(JConstantReference reference) {
      if (reference.Arena != this) {
        throw new ArgumentException("bad reference: refers to different arena");
      }
      if (reference.Id < 0 || reference.Id >= ReferencedConstants.Count) {
        throw new ArgumentException("bad reference: id out of range");
      }
      return ReferencedConstants[reference.Id];
    }

    /// <summary>
    /// Converts a lisp object representing a constant value to the equivalent export-AST
    /// value.
    /// </summary>
    private JConstant ConvertConstant(LispObject obj) {
      var type = obj.GetType();
      if (type == typeof(BoolType)) {
        return new JBool(((BoolType)obj).Value);
      } else if (type == typeof(ConsType)) {
        var cons = (ConsType)obj;
        return new JCons(ReferTo(cons.Car), ReferTo(cons.Cdr));
      } else if (type == typeof(KeywordSymbolType)) {
        return new JKeyword(((KeywordSymbolType)obj).Identifier);
      } else if (type == typeof(NilType)) {
        return new JNil();
      } else if (type == typeof(NumberType)) {
        return new JNumber(((NumberType)obj).Value);
      } else if (type == typeof(StringType)) {
        return new JString(((StringType)obj).Value);
      } else if (type == typeof(SymbolType)) {
        return new JSymbol(((SymbolType)obj).Identifier);
      } else {
        throw ThrowTypeError(
          "emitted AST can only contain primitive values, please check your macros. " +
          "(found non-primitive object {0})", obj);
      }
    }
  }
}
