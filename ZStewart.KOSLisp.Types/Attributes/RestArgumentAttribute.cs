using System;

namespace ZStewart.KOSLisp.Types.Attributes {
  /// <summary>
  /// What should be done with arguments that fit a given rest argument.
  /// </summary>
  public enum RestArgumentType {
    /// <summary>
    /// The extra arguments should be collected into a list/dictionary.
    /// </summary>
    Capture,
    /// <summary>
    /// The extra arguments should be discarded.
    /// </summary>
    Ignore,
    /// <summary>
    /// The extra arguments are not allowed and should cause an error.
    /// </summary>
    Block,
  }

  /// <summary>
  /// Argument attribute for rest (keyword and positional) arguments.
  /// </summary>
  [AttributeUsage(AttributeTargets.Parameter)]
  public abstract class RestArgumentAttribute : Attribute {
    /// <summary>
    /// Whether this rest argument collects keywords or positional arguments.
    /// </summary>
    public abstract bool IsKeyword { get; }
    /// <summary>
    /// How extra arguments found for this argument should be handled.
    /// </summary>
    public abstract RestArgumentType Type { get; }
  }

  public class RestCaptureAttribute : RestArgumentAttribute {
    public override bool IsKeyword { get { return false; } }
    public override RestArgumentType Type { get { return RestArgumentType.Capture; } }
  }

  public class RestIgnoreAttribute : RestArgumentAttribute {
    public override bool IsKeyword { get { return false; } }
    public override RestArgumentType Type { get { return RestArgumentType.Ignore; } }
  }

  public class RestBlockAttribute : RestArgumentAttribute {
    public override bool IsKeyword { get { return false; } }
    public override RestArgumentType Type { get { return RestArgumentType.Block; } }
  }

  public class RestKwCaptureAttribute : RestArgumentAttribute {
    public override bool IsKeyword { get { return true; } }
    public override RestArgumentType Type { get { return RestArgumentType.Capture; } }
  }

  public class RestKwIgnoreAttribute : RestArgumentAttribute {
    public override bool IsKeyword { get { return true; } }
    public override RestArgumentType Type { get { return RestArgumentType.Ignore; } }
  }
}
