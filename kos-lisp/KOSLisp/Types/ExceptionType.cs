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
    #endregion

    #region Static Helper Methods
    public static ExceptionType CreateException(string message, params object[] args) {
      return new ExceptionType {
        __class__ = Exception,
        message = string.Format(message, args),
      };
    }

    public static ExceptionType CreateTypeError(string message, params object[] args) {
      return new ExceptionType {
        __class__ = TypeError,
        message = string.Format(message, args),
      };
    }

    public static ExceptionType CreateValueError(string message, params object[] args) {
      return new ExceptionType {
        __class__ = ValueError,
        message = string.Format(message, args),
      };
    }

    public static ExceptionType CreateKeyError(string message, params object[] args) {
      return new ExceptionType {
        __class__ = KeyError,
        message = string.Format(message, args),
      };
    }

    public static ExceptionType CreateNotImplemented(
        string message, params object[] args) {
      return new ExceptionType {
        __class__ = NotImplemented,
        message = string.Format(message, args),
      };
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
