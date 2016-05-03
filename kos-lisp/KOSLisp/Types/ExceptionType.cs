using System;

namespace ZStewart.KOSLisp.Types {
  /// <summary>
  /// The builtin error type. All errors will derive from this.
  /// </summary>
  public class ExceptionType : LispObject {
    #region Static Type Setup
    private static LispType _exception;
    public static LispType Exception {
      get {
        if (_exception != null) return _exception;

        _exception = new LispType {
          __name__ = "Exception",
          _instance_type = typeof(ExceptionType),
        };
        _exception.__class__ = LispType.Type;
        _exception.__bases__ = IConsType.ToLispTuple(LispObject.Object);
        _exception.__mro__ = IConsType.ToLispTuple(_exception, LispObject.Object);
        _exception = LispType.ConfigureType(_exception);
        if (_exception == null) throw new InvalidOperationException();
        return _exception;
      }
    }

    private static LispType _typeError;
    public static LispType TypeError {
      get {
        if (_typeError != null) return _typeError;

        _typeError = new LispType {
          __name__ = "TypeError",
        };
        _typeError.__class__ = LispType.Type;
        _typeError.__bases__ = IConsType.ToLispTuple(Exception);
        _typeError.__mro__ = IConsType.ToLispTuple(
          _typeError, Exception, LispObject.Object);
        _typeError = LispType.ConfigureType(_typeError);
        if (_typeError == null) throw new InvalidOperationException();
        return _typeError;
      }
    }

    private static LispType _valueError;
    public static LispType ValueError {
      get {
        if (_valueError != null) return _valueError;

        _valueError = new LispType {
          __name__ = "ValueError",
        };
        _valueError.__class__ = LispType.Type;
        _valueError.__bases__ = IConsType.ToLispTuple(Exception);
        _valueError.__mro__ = IConsType.ToLispTuple(
          _valueError, Exception, LispObject.Object);
        _valueError = LispType.ConfigureType(_valueError);
        if (_valueError == null) throw new InvalidOperationException();
        return _valueError;
      }
    }

    private static LispType _attributeError;
    public static LispType AttributeError {
      get {
        if (_attributeError != null) return _attributeError;

        _attributeError = new LispType {
          __name__ = "AttributeError",
        };
        _attributeError.__class__ = LispType.Type;
        _attributeError.__bases__ = IConsType.ToLispTuple(Exception);
        _attributeError.__mro__ = IConsType.ToLispTuple(
          _attributeError, Exception, LispObject.Object);
        _attributeError = LispType.ConfigureType(_attributeError);
        if (_attributeError == null) throw new InvalidOperationException();
        return _attributeError;
      }
    }

    private static LispType _nameError;
    public static LispType NameError {
      get {
        if (_nameError != null) return _nameError;

        _nameError = new LispType {
          __name__ = "NameError",
        };
        _nameError.__class__ = LispType.Type;
        _nameError.__bases__ = IConsType.ToLispTuple(Exception);
        _nameError.__mro__ = IConsType.ToLispTuple(
          _nameError, Exception, LispObject.Object);
        _nameError = LispType.ConfigureType(_nameError);
        if (_nameError == null) throw new InvalidOperationException();
        return _nameError;
      }
    }

    private static LispType _keyError;
    public static LispType KeyError {
      get {
        if (_keyError != null) return _keyError;

        _keyError = new LispType {
          __name__ = "KeyError",
        };
        _keyError.__class__ = LispType.Type;
        _keyError.__bases__ = IConsType.ToLispTuple(Exception);
        _keyError.__mro__ = IConsType.ToLispTuple(
          _keyError, Exception, LispObject.Object);
        _keyError = LispType.ConfigureType(_keyError);
        if (_keyError == null) throw new InvalidOperationException();
        return _keyError;
      }
    }

    private static LispType _notImplemented;
    public static LispType NotImplemented {
      get {
        if (_notImplemented != null) return _notImplemented;

        _notImplemented = new LispType {
          __name__ = "NotImplemented",
        };
        _notImplemented.__class__ = LispType.Type;
        _notImplemented.__bases__ = IConsType.ToLispTuple(Exception);
        _notImplemented.__mro__ = IConsType.ToLispTuple(
          _notImplemented, Exception, LispObject.Object);
        _notImplemented = LispType.ConfigureType(_notImplemented);
        if (_notImplemented == null) throw new InvalidOperationException();
        return _notImplemented;
      }
    }

    private static LispType _syntaxError;
    public static LispType SyntaxError {
      get {
        if (_syntaxError != null) return _notImplemented;

        _syntaxError = new LispType {
          __name__ = "SyntaxError",
        };
        _syntaxError.__class__ = LispType.Type;
        _syntaxError.__bases__ = IConsType.ToLispTuple(Exception);
        _syntaxError.__mro__ = IConsType.ToLispTuple(
          _syntaxError, Exception, LispObject.Object);
        _syntaxError = LispType.ConfigureType(_notImplemented);
        if (_syntaxError == null) throw new InvalidOperationException();
        return _syntaxError;
      }
    }
    #endregion

    #region Static Helper Methods
    public static ExceptionType CreateException(string message, params object[] args) {
      return new ExceptionType {
        __class__ = Exception,
        message = string.Format(message, args),
      };
    }

    public static void ThrowException(string message, params object[] args) {
      throw new ExceptionWrapper(CreateException(message, args));
    }

    public static ExceptionType CreateTypeError(string message, params object[] args) {
      return new ExceptionType {
        __class__ = TypeError,
        message = string.Format(message, args),
      };
    }

    public static ExceptionWrapper ThrowTypeError(string message, params object[] args) {
      throw new ExceptionWrapper(CreateTypeError(message, args));
    }

    public static ExceptionType CreateValueError(string message, params object[] args) {
      return new ExceptionType {
        __class__ = ValueError,
        message = string.Format(message, args),
      };
    }

    public static ExceptionWrapper ThrowValueError(string message, params object[] args) {
      throw new ExceptionWrapper(CreateValueError(message, args));
    }

    public static ExceptionType CreateAttributeError(
        string message, params object[] args) {
      return new ExceptionType {
        __class__ = AttributeError,
        message = string.Format(message, args),
      };
    }

    public static ExceptionWrapper ThrowAttributeError(
        string message, params object[] args) {
      throw new ExceptionWrapper(CreateAttributeError(message, args));
    }

    public static ExceptionType CreateNameError(string message, params object[] args) {
      return new ExceptionType {
        __class__ = NameError,
        message = string.Format(message, args),
      };
    }

    public static ExceptionWrapper ThrowNameError(string message, params object[] args) {
      throw new ExceptionWrapper(CreateNameError(message, args));
    }

    public static ExceptionType CreateKeyError(string message, params object[] args) {
      return new ExceptionType {
        __class__ = KeyError,
        message = string.Format(message, args),
      };
    }

    public static ExceptionWrapper ThrowKeyError(string message, params object[] args) {
      throw new ExceptionWrapper(CreateKeyError(message, args));
    }

    public static ExceptionType CreateNotImplemented(
        string message, params object[] args) {
      return new ExceptionType {
        __class__ = NotImplemented,
        message = string.Format(message, args),
      };
    }

    public static ExceptionWrapper ThrowNotImplemented(string message, params object[] args) {
      throw new ExceptionWrapper(CreateNotImplemented(message, args));
    }

    public static ExceptionType CreateSyntaxError(
        string message, params object[] args) {
      return new ExceptionType {
        __class__ = SyntaxError,
        message = string.Format(message, args),
      };
    }

    public static ExceptionWrapper ThrowSyntaxError(string message, params object[] args) {
      throw new ExceptionWrapper(CreateSyntaxError(message, args));
    }

    public static bool Check(ExceptionType ex, LispType type) {
      return LispType.IsInstance(ex, type);
    }
    #endregion

    public LispObject __cause__ { get; set; }
    public LispObject __context__ { get; set; }

    private string message;
    public string Message { get { return message; } }

    public override string ToString() {
      return string.Format("{0}: {1}", __class__.__name__, Message);
    }
  }
}
