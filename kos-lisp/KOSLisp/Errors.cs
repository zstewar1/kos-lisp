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
    public string SourceLine { get; }

    /// <summary>
    /// The line where the error occured. Counting from 1 = first line.
    /// </summary>
    public int LineNumber { get; }

    /// <summary>
    /// The column where the error occured. Counting from 0 = first character.
    /// </summary>
    public int ColumnIndex { get; }

    public LispCompileException (string message, string sourceLine, int lineNumber, int columnIndex) : base(message) {
      SourceLine = sourceLine;
      LineNumber = lineNumber;
      ColumnIndex = columnIndex;
    }

    public override string ToString () {
      return string.Format(
        "{0}: {1}\nSourceLine={2}, LineNumber={3}, ColumnIndex={4}",
        this.GetType(), Message, SourceLine, LineNumber, ColumnIndex);
    }
  }

  class UnexpectedEOLException : LispCompileException {
    public UnexpectedEOLException (string message, string sourceLine, int lineNumber, int columnIndex) 
      : base(message, sourceLine, lineNumber, columnIndex) { }
  }

  class UnexpectedInput : LispCompileException {
    public UnexpectedInput (string message, string sourceLine, int lineNumber, int columnIndex) 
      : base(message, sourceLine, lineNumber, columnIndex) { }
  }

  class UnexpectedSigil : LispCompileException {
    public UnexpectedSigil (string message, string sourceLine, int lineNumber, int columnIndex) 
      : base(message, sourceLine, lineNumber, columnIndex) { }
  }

  class UnexpectedToken : LispCompileException {
    public UnexpectedToken (string message, string sourceLine, int lineNumber, int columnIndex) 
      : base(message, sourceLine, lineNumber, columnIndex) { }
  }
}
