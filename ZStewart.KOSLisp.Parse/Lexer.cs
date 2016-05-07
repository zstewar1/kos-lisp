using System;

namespace ZStewart.KOSLisp.Parse {
  /// <summary>
  /// Type which can be used to get tokens from a text stream.
  /// </summary>
  public interface Lexer<TokType, ModeType> {
    /// <summary>
    /// An event which is triggered before the lexer attempts to read a line of input.
    /// This can be used to trigger, e.g. writing a prompt or prompt continuation before
    /// user input during interactive mode.
    /// </summary>
    event Action BeforeReadLine;

    /// <summary>
    /// Read the next token from the input stream, using "mode" to decide what token types
    /// are available.
    ///
    /// Returns null when the end-of-input has been reached.
    /// </summary>
    Token<TokType> NextToken (ModeType mode);

    /// <summary>
    /// For interactive mode only: when an error occurs in a non-interactive file, the
    /// error is simply propagated. In an interactive terminal sesion, however, it is
    /// necessary to continue to read input even after an error has occurred while
    /// reading, but the contents of the current line should be discarded in that case.
    ///
    /// This method should discard the rest of the currently read line.
    /// </summary>
    void ClearLine();
  }
}
