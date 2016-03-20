using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace ZStewart.KOSLisp {

  /// <summary>
  /// A token streamer for a particular input.
  /// </summary>
  public interface Tokenizer<out TokType, in Mode> {
    /// <summary>
    /// The source text being lexed.
    /// </summary>
    string Source { get; }

    /// <summary>
    /// Get the next token from the tokenizer.
    /// </summary>
    Token<TokType> Next ();

    /// <summary>
    /// Change the tokenizer to the specified mode.
    /// </summary>
    /// <param name="mode">The mode to switch to. Must be a member of Modes.</param>
    void SwitchMode (Mode mode);
  }

  /// <summary>
  /// A lexical configuration.
  /// </summary>
  public interface Lexer<out TokType, in Mode> {
    /// <summary>
    /// Create a tokenizer for the given string using this lexicon.
    /// </summary>
    /// <param name="source">The string to tokenize.</param>
    Tokenizer<TokType, Mode> Lex (string source);
  }
}