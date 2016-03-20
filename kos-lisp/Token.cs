using System;

namespace ZStewart.KOSLisp {

  /// <summary>
  /// Represents a chunk of tokenized input file.
  /// </summary>
  public interface Token {
    /// <summary>
    /// The string value that was matched into this token.
    /// </summary>
    /// <value>The token's raw value.</value>
    string RawValue { get; }
  }

  /// <summary>
  /// Represents a chunk of tokenized input with a special value.
  /// </summary>
  public interface Token<out T> : Token {
    /// <summary>
    /// The parsed value of the token.
    /// </summary>
    /// <value>The parsed value.</value>
    T Value { get; }
  }

  /// <summary>
  /// A function which can be called on a chunk of text to create a token.
  /// </summary>
  public delegate Token TokenCreator (string rawValue);

  /// <summary>
  /// A token that holds only its raw value.
  /// </summary>
  public class RawToken : Token {
    public static Token Create (string rawValue) {
      return new RawToken(rawValue);
    }

    public string RawValue { get; private set; }

    protected RawToken (string rawValue) {
      RawValue = rawValue;
    }
  }

  /// <summary>
  /// A token that holds an arbitrary parsed value, in addition to the raw string.
  /// </summary>
  public class GenericToken<T> : RawToken, Token<T> {
    /// <summary>
    /// Creates a token creator function which creates generic tokens based on the provided parser
    /// function.
    /// </summary>
    /// <returns>
    /// The a new delegate which can be used to create GenericTokens based on the given parse 
    /// function.
    /// </returns>
    /// <param name="parseFunc">
    /// Parse func the function to use to conver the raw token into the token's value type.
    /// </param>
    public static TokenCreator CreateTokenCreator (Func<string, T> parseFunc) {
      return delegate(string rawValue) {
        return new GenericToken<T>(rawValue, parseFunc(rawValue));
      };
    }

    public T Value { get; private set; }

    private GenericToken (string rawValue, T value) : base(rawValue) {
      Value = value;
    }
  }
}
