using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZStewart.KOSLisp.Types {
  public class ExceptionWrapper : Exception {
    public ExceptionType LispException { get; }
    public ExceptionWrapper(ExceptionType exception) {
      LispException = exception;
    }

    public static implicit operator ExceptionType(ExceptionWrapper ew) {
      return ew.LispException;
    }
  }
}
