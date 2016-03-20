using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace ZStewart.KOSLisp {

  /// <summary>
  /// A token streamer for a particular input.
  /// </summary>
  public interface Tokenizer {
    /// <summary>
    /// Get the next token from the tokenizer.
    /// </summary>
    Token Next ();

    /// <summary>
    /// The list of available modes.
    /// </summary>
    /// <value>
    /// The available modes for this tokenizer.
    /// </value>
    ImmutableList<Mode> Modes { get; }

    /// <summary>
    /// Change the tokenizer to the specified mode.
    /// </summary>
    /// <param name="mode">The mode to switch to. Must be a member of Modes.</param>
    void SwitchMode (Mode mode);
  }

  /// <summary>
  /// A mode that the tokenizer can be in.
  /// </summary>
  public interface Mode {
    /// <summary>
    /// The name of this mode.
    /// </summary>
    /// <value>The name.</value>
    string Name { get; }
  }

  /// <summary>
  /// A lexical configuration.
  /// </summary>
  public interface Lexer {
    /// <summary>
    /// Create a tokenizer for the given string using this lexicon.
    /// </summary>
    /// <param name="source">The string to tokenize.</param>
    Tokenizer Lex (string source);
  }

  /// <summary>
  /// Basic implementation of a mode. Just wraps a string.
  /// </summary>
  public sealed class StringMode : Mode {
    public string Name { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ZStewart.KOSLisp.StringMode"/> class.
    /// </summary>
    /// <param name="name">The name of this mode.</param>
    public StringMode (string name) {
      Name = Preconditions.CheckNotNullOrEmpty(name, "Name required");
    }

    public override bool Equals (object obj) {
      if (obj == null) {
        return false;
      } else if (obj is StringMode) {
        return Equals(obj as StringMode);
      } else {
        return false;
      }
    }

    public bool Equals (StringMode other) {
      if (other == null) {
        return false;
      } else {
        return Name == other.Name;
      }
    }

    public override int GetHashCode () {
      return Name.GetHashCode();
    }
  }
}

