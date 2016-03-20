using System;
using System.Diagnostics;

namespace ZStewart.KOSLisp {
  /// <summary>
  /// Simple precondition checker.
  ///
  /// Modeled after the Precondition checker in Google's Guava (Java) library. (See: 
  /// http://docs.guava-libraries.googlecode.com/git/javadoc/com/google/common/base/Preconditions.html)
  /// </summary>
  public static class Preconditions {
    /// <summary>
    /// Raises an argument exception if the given string is null or empty.
    /// </summary>
    /// <returns>The string given.</returns>
    /// <param name="s">The string to check.</param>
    public static string CheckNotNullOrEmpty (string s) {
      if (string.IsNullOrEmpty(s))
        throw new ArgumentException();
      return s;
    }

    /// <summary>
    /// Raises an argument exception with the given message if the given string is null or empty.
    /// </summary>
    /// <returns>The string given.</returns>
    /// <param name="s">The string to check.</param>
    /// <param name="message">Message to set in the exception.</param>
    public static string CheckNotNullOrEmpty (string s, string message) {
      if (string.IsNullOrEmpty(s))
        throw new ArgumentException(message);
      return s;
    }
    
    /// <summary>
    /// Raises an argument exception if the given value is null.
    /// </summary>
    /// <typeparam name="T">The type of the value to check.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <returns>The given value.</returns>
    public static T CheckNotNull<T> (T value) {
      if (value == null)
        throw new ArgumentException();
      return value;
    }

    /// <summary>
    /// Raises an argument exception with the given message if the given value is null.
    /// </summary>
    /// <typeparam name="T">The type of the value to check.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="message">The message to set on the thrown error.</param>
    /// <returns>The given value.</returns>
    public static T CheckNotNull<T> (T value, string message) {
      if (value == null)
        throw new ArgumentException(message);
      return value;
    }

    /// <summary>
    /// Raises an argument exception if the condition is false.
    /// </summary>
    /// <param name="expression">The expression to check.</param>
    public static void CheckArgument (bool expression) {
      if (!expression)
        throw new ArgumentException();
    }

    /// <summary>
    /// Raises an argument exception with the given message if the condition is false.
    /// </summary>
    /// <param name="expression">The expression to check.</param>
    /// <param name="message">The message to set on the error.</param>
    public static void CheckArgument (bool expression, string message) {
      if (!expression)
        throw new ArgumentException(message);
    }
  }
}

