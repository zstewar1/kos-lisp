using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZStewart.Compilers {
  public interface Compiler {
    /// <summary>
    /// Reads input from the source files and transforms it into output in the destination file.
    /// </summary>
    /// <param name="sourceFiles">The files to read output from.</param>
    /// <param name="destinationFile">The file to write output to.</param>
    void Compile (IList<string> sourceFiles, string destinationFile);

    /// <summary>
    /// Reads input from the source files and transforms it into output in a default destination file.
    /// </summary>
    /// <param name="sourceFiles">The set of files to read input from.</param>
    void Compile (IList<string> sourceFiles);
  }
}
