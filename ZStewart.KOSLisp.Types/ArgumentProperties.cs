using System;

namespace ZStewart.KOSLisp.Types {
  /// <summary>
  /// Represents the possible kinds of arguments to a function.
  /// </summary>
  public enum ArgumentType {
    /// <summary>
    /// An argument that can be assigned by its position or name.
    /// </summary>
    PositionalOrKeyword,
    /// <summary>
    /// An argument that collects all extra positional arguments.
    /// </summary>
    RestCapture,
    /// <summary>
    /// An argument that captures and discards all extra positional arguments.
    /// </summary>
    RestIgnore,
    /// <summary>
    /// An argument that causes extra positional arguments to result in an error.
    /// </summary>
    RestBlock,
    /// <summary>
    /// An argument that can be assigned by name only.
    /// </summary>
    Keyword,
    /// <summary>
    /// An argument that collects all extra keyword arguments.
    /// </summary>
    RestKwCapture,
    /// <summary>
    /// An argument the collects and discards all other keyword arguments.
    /// </summary>
    RestKwIgnore,
  }

  /// <summary>
  /// Tracks information about an argument which can be used to buld an argument list.
  /// </summary>
  public class ArgumentProperties {
    /// <summary>
    /// The ArgumentType of this argument.
    /// </summary>
    public ArgumentType Type { get; }
    /// <summary>
    /// The symbol representing the name of the argument being passed into.
    /// Only meaningful for PositionalOrKeyword and Keyword arguments.
    /// </summary>
    public SymbolType Name { get; }
    /// <summary>
    /// Whether it is ok to leave out the value for this argument.
    /// Only meaningful for PositionalOrKeyword and Keyword arguments.
    /// </summary>
    public bool IsOptional { get; }

    /// <summary>
    /// Creates an argument property with the given type, optionality, and name
    /// </summary>
    public ArgumentProperties(
        ArgumentType type, SymbolType name = null, bool isOptional = false) {
      if (type == ArgumentType.PositionalOrKeyword || type == ArgumentType.Keyword) {
        if (name == null) {
          throw new ArgumentException(string.Format(
            "a name must be provided for {0} arguments, got null", type));
        }
      } else {
        if (name != null) {
          throw new ArgumentException(string.Format(
            "Rest* arguments cannot have names, got {0}", name));
        }
        if (isOptional) {
          throw new ArgumentException("Rest* arguments cannot be optional");
        }
      }

      Type = type;
      Name = name;
      IsOptional = isOptional;
    }
  }
}
