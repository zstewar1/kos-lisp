using System;

namespace ZStewart.KOSLisp.Types.Attributes {
  [AttributeUsage(AttributeTargets.Method)]
  public class BuiltinFunctionAttribute : Attribute {
    public string Name { get; set; }
  }
}
