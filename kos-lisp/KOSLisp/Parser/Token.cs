using System;

namespace ZStewart.KOSLisp.Parser {

  /// <summary>
  /// Represents a chunk of tokenized input file.
  /// </summary>
  public interface Token<out TokType> {
    /// <summary>
    /// The string value that was matched into this token.
    /// </summary>
    string RawValue { get; }

    /// <summary>
    /// Where this token came from.
    /// </summary>
    SourceInformation SourceInformation { get; }

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
  public delegate Token<TokType> TokenCreator<out TokType> (
    string rawValue, SourceInformation sourceInformation);

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
    /// <param name="rv">The matched value of the token.</param>
    /// <param name="si">Information about where this token originated.</param>
    /// <param name="type">The type of the token.</param>
    /// <returns>A newly created raw token.</returns>
    public static Token<TokType> Create<TokType>(
        string rv, SourceInformation si, TokType type) {
      return RawToken<TokType>.Create(rv, si, type);
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
      return delegate (string rv, SourceInformation si) {
        return Create(rv, si, type);
      };
    }

    /// <summary>
    /// Creates a raw token with the specified type.
    /// </summary>
    /// <param name="rv">The matched value of the token.</param>
    /// <param name="si">Information about where this token originated.</param>
    /// <param name="type">The type of the token.</param>
    /// <returns>A newly created raw token.</returns>
    public static RawToken<TokType> Create(
        string rv, SourceInformation si, TokType type) {
      return new RawToken<TokType>(rv, si, type);
    }

    public string RawValue { get; }
    public SourceInformation SourceInformation { get; }
    public TokType TokenType { get; }

    protected RawToken (string rv, SourceInformation si, TokType type) {
      RawValue = rv;
      SourceInformation = si;
      TokenType = type;
    }

    public override string ToString () {
      return string.Format(
        "[RawToken: RawValue={0}, SourceInfo={1}, TokenType={2}]", 
        RawValue, SourceInformation, TokenType);
    }
  }

  /// <summary>
  /// Helper class for using static functions of GenericToken(TokType, T) without type 
  /// arguments.
  /// </summary>
  public static class GenericToken {
    /// <summary>
    /// Creates a token creator function which creates generic tokens based on the 
    /// provided parser function.
    /// </summary>
    /// <param name="tokenType">
    /// The type of token that this creator function should create.
    /// </param>
    /// <param name="parseFunc">
    /// Parse func the function to use to conver the raw token into the token's value 
    /// type.
    /// </param>
    /// <returns>
    /// The a new delegate which can be used to create GenericTokens based on the given 
    /// parse function.
    /// </returns>
    public static TokenCreator<TokType> CreateTokenCreator<TokType, T> (
        TokType tokenType, Func<string, T> parseFunc) {
      return GenericToken<TokType, T>.CreateTokenCreator(tokenType, parseFunc);
    }

    /// <summary>
    /// Creates a generic token with specified raw/parsed value and token type.
    /// </summary>
    /// <param name="rv">The raw, matched value of the token</param>
    /// <param name="si">Information about where this token originated.</param>
    /// <param name="type">The type of the token.</param>
    /// <param name="val">The value of the token.</param>
    /// <returns></returns>
    public static GenericToken<TokType, T> Create<TokType, T> (
        string rv, SourceInformation si, TokType type, T val) {
      return GenericToken<TokType, T>.Create(rv, si, type, val);
    }
  }

  /// <summary>
  /// A token that holds an arbitrary parsed value, in addition to the raw string.
  /// </summary>
  public class GenericToken<TokType, T> : RawToken<TokType>, Token<TokType, T> {
    /// <summary>
    /// Creates a token creator function which creates generic tokens based on the 
    /// provided parser function.
    /// </summary>
    /// <param name="type">
    /// The type of token that this creator function should create.
    /// </param>
    /// <param name="parseFunc">
    /// Parse func the function to use to conver the raw token into the token's value 
    /// type.
    /// </param>
    /// <returns>
    /// The a new delegate which can be used to create GenericTokens based on the given 
    /// parse function.
    /// </returns>
    public static TokenCreator<TokType> CreateTokenCreator (
        TokType type, Func<string, T> parseFunc) {
      return delegate(string rv, SourceInformation si) {
        return Create(rv, si, type, parseFunc(rv));
      };
    }

    /// <summary>
    /// Creates a generic token with specified raw/parsed value and token type.
    /// </summary>
    /// <param name="rv">The raw, matched value of the token</param>
    /// <param name="si">Information about where this token originated.</param>
    /// <param name="type">The type of the token.</param>
    /// <param name="val">The value of the token.</param>
    /// <returns></returns>
    public static GenericToken<TokType, T> Create(
        string rv, SourceInformation si, TokType type, T val) {
      return new GenericToken<TokType, T>(rv, si, type, val);
    }

    public T Value { get; }

    protected GenericToken (string rv, SourceInformation si, TokType type, T value) 
        : base(rv, si, type) {
      Value = value;
    }

    public override string ToString () {
      return string.Format(
        "[GenericToken: Value={0}, RawValue={1}, SourceInformation={2} TokenType={3}]", 
        Value, RawValue, SourceInformation, TokenType);
    }
  }
}
