using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZStewart.KOSLisp.Types.Attributes {
  [AttributeUsage(AttributeTargets.Parameter)]
  public class PositionalArgument : Attribute { }
}
