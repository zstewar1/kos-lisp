using System.Collections.Generic;
using System.IO;

namespace ZStewart.KOSLisp.Parse {
  /// <summary>
  /// A lexer data source. Iterates over input lines.
  /// </summary>
  public interface Source : IEnumerable<string> {
    /// <summary>
    /// The name of this data source.
    /// </summary>
    string Name { get; }
  }

  /// <summary>
  /// A simple source that gets data from a text reader.
  /// </summary>
  public class TextReaderSource : Source {
    public string Name { get; }

    private readonly TextReader textSource;

    public TextReaderSource(string name, TextReader textSource) {
      Name = name;
      this.textSource = textSource;
    }

    public IEnumerator<string> GetEnumerator() {
      string line;
      while ((line = textSource.ReadLine()) != null) {
        yield return line;
      }
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
      return GetEnumerator();
    }
  }
}
