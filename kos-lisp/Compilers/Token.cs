using System;

namespace ZStewart.Compilers {

  /// <summary>
  /// Represents a chunk of tokenized input file.
  /// </summary>
  public interface Token<out TokType> {
    /// <summary>
    /// The string value that was matched into this token.
    /// </summary>
    string RawValue { get; }

    /// <summary>
    /// Index in the source where the match was found.
    /// </summary>
    int Index { get; }

    /// <summary>
    /// The line where this token was matched.
    /// </summary>
    int Line { get; }

    /// <summary>
    /// The column that this token started in.
    /// </summary>
    int Column { get; }

    /// <summary>
    /// The type of this token.
    /// </summary>
    TokType TokenType { get; }
  }

  /// <summary>
  /// Represents a chunk of tokenized input with a special value.
  /// </summary>
  public interface Token<out TokType, out T> : Token<TokType> {
    /// <summary>
    /// The parsed value of the token.
    /// </summary>
    /// <value>The parsed value.</value>
    T Value { get; }
  }

  /// <summary>
  /// A function which can be called on a chunk of text to create a token.
  /// </summary>
  public delegate Token<TokType> TokenCreator<out TokType> (string rawValue, int index, int line, int column);

  /// <summary>
  /// Helper class for using RawToken(TokType) without type aruments on static functions.
  /// </summary>
  public static class RawToken {
    /// <summary>
    /// Creates a TokenCreator function for Raw-Value tokens with the given token type.
    /// </summary>
    /// <param name="type">The token type for tokens created with this creator.</param>
    /// <returns>A function that creates tokens with the given token type.</returns>
    public static TokenCreator<TokType> CreateTokenCreator<TokType>(TokType type) {
      return RawToken<TokType>.CreateTokenCreator(type);
    }
    /// <summary>
    /// Creates a raw token with the specified type.
    /// </summary>
    /// <param name="rawValue">The matched value of the token.</param>
    /// <param name="index">The index where the token started.</param>
    /// <param name="line">The line that the token was on.</param>
    /// <param name="column">The column that the token starts at.</param>
    /// <param name="type">The type of the token.</param>
    /// <returns>A newly created raw token.</returns>
    public static Token<TokType> Create<TokType>(string rawValue, int index, int line, int column, TokType type) {
      return RawToken<TokType>.Create(rawValue, index, line, column, type);
    }
  }

  /// <summary>
  /// A token that holds only its raw value.
  /// </summary>
  public class RawToken<TokType> : Token<TokType> {
    /// <summary>
    /// Creates a TokenCreator function for Raw-Value tokens with the given token type.
    /// </summary>
    /// <param name="type">The token type for tokens created with this creator.</param>
    /// <returns>A function that creates tokens with the given token type.</returns>
    public static TokenCreator<TokType> CreateTokenCreator(TokType type) {
      return delegate (string rawValue, int index, int line, int column) {
        return Create(rawValue, index, line, column, type);
      };
    }

    /// <summary>
    /// Creates a raw token with the specified type.
    /// </summary>
    /// <param name="rawValue">The matched value of the token.</param>
    /// <param name="index">The index where the token started</param>
    /// <param name="line">The line that the token was on.</param>
    /// <param name="column">The column that the token starts at.</param>
    /// <param name="tokenType">The type of the token.</param>
    /// <returns>A newly created raw token.</returns>
    public static RawToken<TokType> Create(string rawValue, int index, int line, int column, TokType tokenType) {
      return new RawToken<TokType>(rawValue, index, line, column, tokenType);
    }

    public string RawValue { get; }
    public int Index { get; }
    public int Line { get; }
    public int Column { get; }
    public TokType TokenType { get; }

    protected RawToken (string rawValue, int index, int line, int column, TokType tokenType) {
      RawValue = rawValue;
      Index = index;
      Line = line;
      Column = column;
      TokenType = tokenType;
    }

    public override string ToString () {
      return string.Format(
        "[RawToken: RawValue={0}, Index={1}, Line={2}, Column={3}, TokenType={4}]", 
        RawValue, Index, Line, Column, TokenType);
    }
  }

  /// <summary>
  /// Helper class for using static functions of GenericToken(TokType, T) without type arguments.
  /// </summary>
  public static class GenericToken {
    /// <summary>
    /// Creates a token creator function which creates generic tokens based on the provided parser
    /// function.
    /// </summary>
    /// <param name="tokenType">The type of token that this creator function should create.</param>
    /// <param name="parseFunc">
    /// Parse func the function to use to conver the raw token into the token's value type.
    /// </param>
    /// <returns>
    /// The a new delegate which can be used to create GenericTokens based on the given parse 
    /// function.
    /// </returns>
    public static TokenCreator<TokType> CreateTokenCreator<TokType, T> (TokType tokenType, Func<string, T> parseFunc) {
      return GenericToken<TokType, T>.CreateTokenCreator(tokenType, parseFunc);
    }

    /// <summary>
    /// Creates a generic token with specified raw/parsed value and token type.
    /// </summary>
    /// <param name="rawValue">The raw, matched value of the token</param>
    /// <param name="index">The index in the source where this token starts.</param>
    /// <param name="line">The line that the token was on.</param>
    /// <param name="column">The column that the token starts at.</param>
    /// <param name="tokenType">The type of the token.</param>
    /// <param name="value">The value of the token.</param>
    /// <returns></returns>
    public static GenericToken<TokType, T> Create<TokType, T> (string rawValue, int index, int line, int column, TokType tokenType, T value) {
      return GenericToken<TokType, T>.Create(rawValue, index, line, column, tokenType, value);
    }
  }

  /// <summary>
  /// A token that holds an arbitrary parsed value, in addition to the raw string.
  /// </summary>
  public class GenericToken<TokType, T> : RawToken<TokType>, Token<TokType, T> {
    /// <summary>
    /// Creates a token creator function which creates generic tokens based on the provided parser
    /// function.
    /// </summary>
    /// <param name="tokenType">The type of token that this creator function should create.</param>
    /// <param name="parseFunc">
    /// Parse func the function to use to conver the raw token into the token's value type.
    /// </param>
    /// <returns>
    /// The a new delegate which can be used to create GenericTokens based on the given parse 
    /// function.
    /// </returns>
    public static TokenCreator<TokType> CreateTokenCreator (TokType tokenType, Func<string, T> parseFunc) {
      return delegate(string rawValue, int index, int line, int column) {
        return Create(rawValue, index, line, column, tokenType, parseFunc(rawValue));
      };
    }

    /// <summary>
    /// Creates a generic token with specified raw/parsed value and token type.
    /// </summary>
    /// <param name="rawValue">The raw, matched value of the token</param>
    /// <param name="index">The index in the source where this token starts.</param>
    /// <param name="line">The line that the token was on.</param>
    /// <param name="column">The column that the token starts at.</param>
    /// <param name="tokenType">The type of the token.</param>
    /// <param name="value">The value of the token.</param>
    /// <returns></returns>
    public static GenericToken<TokType, T> Create(string rawValue, int index, int line, int column, TokType tokenType, T value) {
      return new GenericToken<TokType, T>(rawValue, index, line, column, tokenType, value);
    }

    public T Value { get; }

    protected GenericToken (string rawValue, int index, int line, int column, TokType tokenType, T value) 
        : base(rawValue, index, line, column, tokenType) {
      Value = value;
    }

    public override string ToString () {
      return string.Format(
        "[GenericToken: Value={0}, RawValue={1}, Index={2}, Line={3}, Column={4}, TokenType={5}]", 
        Value, RawValue, Index, Line, Column, TokenType);
    }
  }
}
