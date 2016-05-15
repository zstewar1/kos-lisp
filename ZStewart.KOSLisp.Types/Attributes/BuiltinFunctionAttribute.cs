using System;

namespace ZStewart.KOSLisp.Types.Attributes {
  [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
  public class BuiltinFunctionAttribute : Attribute {
    public string Name { get; set; }
  }
}
