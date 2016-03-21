using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZStewart.KOSLisp {
  class LispCompileException : Exception {
    /// <summary>
    /// Index in the string where the error occured.
    /// </summary>
    public int Index { get; private set; }

    /// <summary>
    /// The line where the error occured.
    /// </summary>
    public int Line { get; private set; }

    /// <summary>
    /// The column where the error occured.
    /// </summary>
    public int Column { get; private set; }

    public LispCompileException(string message, int index, int line, int column) : base(message) {
      Index = index;
      Line = line;
      Column = column;
    }
  }

  class UnexpectedEOLException : LispCompileException {
    public UnexpectedEOLException (string message, int index, int line, int column) 
      : base(message, index, line, column) { }
  }

  class UnexpectedInput : LispCompileException {
    public UnexpectedInput (string message, int index, int line, int column) 
      : base(message, index, line, column) { }
  }

  class UnexpectedSigil : LispCompileException {
    public UnexpectedSigil (string message, int index, int line, int column) 
      : base(message, index, line, column) { }
  }
}
