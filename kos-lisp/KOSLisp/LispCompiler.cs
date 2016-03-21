using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using ZStewart.Compilers;

namespace ZStewart.KOSLisp {

  public class LispCompilerException : Exception {
    public LispCompilerException (string message) : base(message) { }
    public LispCompilerException (string message, Exception inner) : base(message, inner) { }
  }

  class LispCompiler : Compiler {
    public void Compile (IList<string> sourceFiles) {
      Preconditions.CheckArgument(sourceFiles.Count > 0);
      Compile(sourceFiles, Path.ChangeExtension(sourceFiles[0], ".ks"));
    }

    public void Compile (IList<string> sourceFiles, string destinationFile) {
      Preconditions.CheckArgument(sourceFiles.Count > 0);
      Preconditions.CheckNotNull(destinationFile);

      Lexer<LispTokType, LispLexMode> lexer = new LispLexer();

      foreach (var f in sourceFiles) {
        string content;
        try {
          content = File.ReadAllText(f);
        } catch (Exception e) {
          if (
            e is ArgumentException ||
            e is ArgumentNullException ||
            e is PathTooLongException ||
            e is DirectoryNotFoundException ||
            e is IOException ||
            e is UnauthorizedAccessException ||
            e is FileNotFoundException ||
            e is NotSupportedException ||
            e is SecurityException
          ) {
            throw new LispCompilerException(string.Format("Unable to read file \"{0}\":\n{1}", f, e.Message), e);
          }
          throw;
        }
        Tokenizer<LispTokType, LispLexMode> tokenizer = lexer.Lex(content);
      }
    }
  }
}
