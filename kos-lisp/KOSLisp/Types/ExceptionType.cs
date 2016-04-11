using System;

namespace ZStewart.KOSLisp.Types {
  /// <summary>
  /// The builtin error type. All errors will derive from this.
  /// </summary>
  public class ExceptionType : LispObject {
    #region Static Type Setup
    private static LispTypeObject _exception;
    public static LispTypeObject Exception {
      get {
        if (_exception != null) return _exception;

        _exception = new LispTypeObject {
          __name__ = "Exception",
        };
        _exception.__class__ = TypeType.Type;
        _exception.__bases__ = IConsType.ToLispTuple(ObjectType.Object);
        _exception.__mro__ = IConsType.ToLispTuple(_exception, ObjectType.Object);
        _exception = LispTypeObject.ConfigureType(_exception);
        if (_exception == null) throw new InvalidOperationException();
        return _exception;
      }
    }

    private static LispTypeObject _typeError;
    public static LispTypeObject TypeError {
      get {
        if (_typeError != null) return _typeError;

        _typeError = new LispTypeObject {
          __name__ = "TypeError",
        };
        _typeError.__class__ = TypeType.Type;
        _typeError.__bases__ = IConsType.ToLispTuple(Exception);
        _typeError.__mro__ = IConsType.ToLispTuple(
          _typeError, Exception, ObjectType.Object);
        _typeError = LispTypeObject.ConfigureType(_typeError);
        if (_typeError == null) throw new InvalidOperationException();
        return _typeError;
      }
    }

    private static LispTypeObject _valueError;
    public static LispTypeObject ValueError {
      get {
        if (_valueError != null) return _valueError;

        _valueError = new LispTypeObject {
          __name__ = "ValueError",
        };
        _valueError.__class__ = TypeType.Type;
        _valueError.__bases__ = IConsType.ToLispTuple(Exception);
        _valueError.__mro__ = IConsType.ToLispTuple(
          _valueError, Exception, ObjectType.Object);
        _valueError = LispTypeObject.ConfigureType(_valueError);
        if (_valueError == null) throw new InvalidOperationException();
        return _valueError;
      }
    }

    private static LispTypeObject _keyError;
    public static LispTypeObject KeyError {
      get {
        if (_keyError != null) return _keyError;

        _keyError = new LispTypeObject {
          __name__ = "KeyError",
        };
        _keyError.__class__ = TypeType.Type;
        _keyError.__bases__ = IConsType.ToLispTuple(Exception);
        _keyError.__mro__ = IConsType.ToLispTuple(
          _keyError, Exception, ObjectType.Object);
        _keyError = LispTypeObject.ConfigureType(_keyError);
        if (_keyError == null) throw new InvalidOperationException();
        return _keyError;
      }
    }

    private static LispTypeObject _notImplemented;
    public static LispTypeObject NotImplemented {
      get {
        if (_notImplemented != null) return _notImplemented;

        _notImplemented = new LispTypeObject {
          __name__ = "NotImplemented",
        };
        _notImplemented.__class__ = TypeType.Type;
        _notImplemented.__bases__ = IConsType.ToLispTuple(Exception);
        _notImplemented.__mro__ = IConsType.ToLispTuple(
          _notImplemented, Exception, ObjectType.Object);
        _notImplemented = LispTypeObject.ConfigureType(_notImplemented);
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
