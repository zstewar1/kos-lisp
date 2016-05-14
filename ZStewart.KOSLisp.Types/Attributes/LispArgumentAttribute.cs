using System;

namespace ZStewart.KOSLisp.Types.Attributes {
  /// <summary>
  /// An attribute that marks this argument as being a lisp argment (as opposed to a rest
  /// argument).
  /// </summary>
  [AttributeUsage(AttributeTargets.Parameter)]
  public abstract class LispArgumentAttribute : Attribute {
    /// <summary>
    /// The name of this attribute (used for calling with keywords).
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// Whether this argument is optional.
    /// </summary>
    public abstract bool IsOptional { get; }
    /// <summary>
    /// The default value for this argument (if it is optional).
    /// </summary>
    public virtual object Default { get { return null; } }
  }

  public class RequiredAttribute : LispArgumentAttribute {
    public override bool IsOptional { get { return false; } }
  }

  public class OptionalAttribute : LispArgumentAttribute {
    public override object Default { get; }
    public override bool IsOptional { get { return true; } }

    public OptionalAttribute(object @default) {
      Default = @default;
    }
  }
}
