using System;

namespace ZStewart.KOSLisp.Types.Attributes {
  [AttributeUsage(AttributeTargets.Parameter)]
  public class KeywordArgumentAttribute : Attribute {
    public string ArgumentName { get; set; }
  }
}
