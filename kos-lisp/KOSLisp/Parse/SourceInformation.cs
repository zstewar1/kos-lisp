using System.Text;

namespace ZStewart.KOSLisp.Parse {
  /// <summary>
  /// Provides information about a location within a lisp source file.
  /// </summary>
  public struct SourceInformation {

    /// <summary>
    /// The name of the source location that this input originates from. (i.e. the
    /// filename).
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The line in the source file that this source information refers to.
    /// </summary>
    public string Line { get; set; }

    /// <summary>
    /// The number of the line in the source file that this source information points to.
    /// </summary>
    public int LineNumber { get; set; }

    /// <summary>
    /// The zero-based index of the column within the line that this source information
    /// points to.
    /// </summary>
    public int ColumnIndex { get; set; }

    /// <summary>
    /// Create a source information object with the specified fields.
    /// </summary>
    public SourceInformation(
      string name, string line, int lineNumber, int columnIndex) {
      Name = name;
      Line = line;
      LineNumber = lineNumber;
      ColumnIndex = columnIndex;
    }

    public override string ToString() {
      return string.Format(
        "[SourceInformation Name={0} Line={1} Column={2}", Name, LineNumber, ColumnIndex);
    }

    /// <summary>
    /// Gets an output representation of this source information as a multiline string.
    ///
    /// The output has no preceeding or ending newline.
    /// </summary>
    public string GetRepresentation() {
      var sb = new StringBuilder();
      sb.AppendFormat("{0} Line {1}, Column {2}\n", Name, LineNumber, ColumnIndex + 1);
      sb.AppendLine(Line);
      sb.Append(' ', ColumnIndex);
      sb.Append('^');
      return sb.ToString();
    }
  }
}
