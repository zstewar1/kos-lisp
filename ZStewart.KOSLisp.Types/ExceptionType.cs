using System;

using ZStewart.KOSLisp.Types.Attributes;

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
        LispType.ConfigureType(_exception);

        LispType.AddStatic(_exception, "ToStr", PropConsts.Str);
        LispType.AddStatic(_exception, "ToRepr", PropConsts.Repr);

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
        LispType.ConfigureType(_typeError);
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
        LispType.ConfigureType(_valueError);
        return _valueError;
      }
    }

    private static LispType _runtimeError;
    public static LispType RuntimeError {
      get {
        if (_runtimeError != null) return _runtimeError;

        _runtimeError = new LispType {
          __name__ = "RuntimeError",
        };
        _runtimeError.__class__ = LispType.Type;
        _runtimeError.__bases__ = IConsType.ToLispTuple(Exception);
        _runtimeError.__mro__ = IConsType.ToLispTuple(
          _runtimeError, Exception, LispObject.Object);
        LispType.ConfigureType(_runtimeError);
        return _runtimeError;
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
        LispType.ConfigureType(_attributeError);
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
        LispType.ConfigureType(_nameError);
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
        LispType.ConfigureType(_keyError);
        return _keyError;
      }
    }

    private static LispType _notImplementedException;
    public static LispType NotImplementedException {
      get {
        if (_notImplementedException != null) return _notImplementedException;

        _notImplementedException = new LispType {
          __name__ = "NotImplementedException",
        };
        _notImplementedException.__class__ = LispType.Type;
        _notImplementedException.__bases__ = IConsType.ToLispTuple(Exception);
        _notImplementedException.__mro__ = IConsType.ToLispTuple(
          _notImplementedException, Exception, LispObject.Object);
        LispType.ConfigureType(_notImplementedException);
        return _notImplementedException;
      }
    }

    private static LispType _syntaxError;
    public static LispType SyntaxError {
      get {
        if (_syntaxError != null) return _syntaxError;

        _syntaxError = new LispType {
          __name__ = "SyntaxError",
        };
        _syntaxError.__class__ = LispType.Type;
        _syntaxError.__bases__ = IConsType.ToLispTuple(Exception);
        _syntaxError.__mro__ = IConsType.ToLispTuple(
          _syntaxError, Exception, LispObject.Object);
        LispType.ConfigureType(_syntaxError);
        return _syntaxError;
      }
    }

    private static LispObject ToRepr([Required] ExceptionType self) {
      return StringType.Format("({0} {1})", self.__class__.__name__, self.Message);
    }

    private static LispObject ToStr([Required] ExceptionType self) {
      return StringType.Create(self.Message);
    }
    #endregion

    #region Static Helper Methods
    #region Exception
    public static ExceptionType CreateException(
        ExceptionType cause, string message, params object[] args) {
      return new ExceptionType {
        __class__ = Exception,
        messageFormat = message,
        formatArgs = args,
        __cause__ = cause,
      };
    }

    public static ExceptionType CreateException(string message, params object[] args) {
      return CreateException(null, message, args);
    }

    public static ExceptionWrapper ThrowException(
        ExceptionType cause, string message, params object[] args) {
      throw new ExceptionWrapper(CreateException(cause, message, args));
    }

    public static ExceptionWrapper ThrowException(string message, params object[] args) {
      throw new ExceptionWrapper(CreateException(message, args));
    }
    #endregion Exception

    #region TypeError
    public static ExceptionType CreateTypeError(
        ExceptionType cause, string message, params object[] args) {
      return new ExceptionType {
        __class__ = TypeError,
        messageFormat = message,
        formatArgs = args,
        __cause__ = cause,
      };
    }

    public static ExceptionType CreateTypeError(string message, params object[] args) {
      return CreateTypeError(null, message, args);
    }

    public static ExceptionWrapper ThrowTypeError(
        ExceptionType cause, string message, params object[] args) {
      throw new ExceptionWrapper(CreateTypeError(cause, message, args));
    }

    public static ExceptionWrapper ThrowTypeError(string message, params object[] args) {
      throw new ExceptionWrapper(CreateTypeError(message, args));
    }
    #endregion TypeError

    #region ValueError
    public static ExceptionType CreateValueError(
        ExceptionType cause, string message, params object[] args) {
      return new ExceptionType {
        __class__ = ValueError,
        messageFormat = message,
        formatArgs = args,
        __cause__ = cause,
      };
    }

    public static ExceptionType CreateValueError(string message, params object[] args) {
      return CreateValueError(null, message, args);
    }

    public static ExceptionWrapper ThrowValueError(
        ExceptionType cause, string message, params object[] args) {
      throw new ExceptionWrapper(CreateValueError(cause, message, args));
    }

    public static ExceptionWrapper ThrowValueError(string message, params object[] args) {
      throw new ExceptionWrapper(CreateValueError(message, args));
    }
    #endregion ValueError

    #region RuntimeError
    public static ExceptionType CreateRuntimeError(
        ExceptionType cause, string message, params object[] args) {
      return new ExceptionType {
        __class__ = RuntimeError,
        messageFormat = message,
        formatArgs = args,
        __cause__ = cause,
      };
    }

    public static ExceptionType CreateRuntimeError(string message, params object[] args) {
      return CreateRuntimeError(null, message, args);
    }

    public static ExceptionWrapper ThrowRuntimeError(
        ExceptionType cause, string message, params object[] args) {
      throw new ExceptionWrapper(CreateRuntimeError(cause, message, args));
    }

    public static ExceptionWrapper ThrowRuntimeError(string message, params object[] args) {
      throw new ExceptionWrapper(CreateRuntimeError(message, args));
    }
    #endregion RuntimeError

    #region AttributeError
    public static ExceptionType CreateAttributeError(
        ExceptionType cause, string message, params object[] args) {
      return new ExceptionType {
        __class__ = AttributeError,
        messageFormat = message,
        formatArgs = args,
        __cause__ = cause,
      };
    }

    public static ExceptionType CreateAttributeError(string message, params object[] args) {
      return CreateAttributeError(null, message, args);
    }

    public static ExceptionWrapper ThrowAttributeError(
        ExceptionType cause, string message, params object[] args) {
      throw new ExceptionWrapper(CreateAttributeError(cause, message, args));
    }

    public static ExceptionWrapper ThrowAttributeError(string message, params object[] args) {
      throw new ExceptionWrapper(CreateAttributeError(message, args));
    }
    #endregion AttributeError

    #region NameError
    public static ExceptionType CreateNameError(
        ExceptionType cause, string message, params object[] args) {
      return new ExceptionType {
        __class__ = NameError,
        messageFormat = message,
        formatArgs = args,
        __cause__ = cause,
      };
    }

    public static ExceptionType CreateNameError(string message, params object[] args) {
      return CreateNameError(null, message, args);
    }

    public static ExceptionWrapper ThrowNameError(
        ExceptionType cause, string message, params object[] args) {
      throw new ExceptionWrapper(CreateNameError(cause, message, args));
    }

    public static ExceptionWrapper ThrowNameError(string message, params object[] args) {
      throw new ExceptionWrapper(CreateNameError(message, args));
    }
    #endregion NameError

    #region KeyError
    public static ExceptionType CreateKeyError(
        ExceptionType cause, string message, params object[] args) {
      return new ExceptionType {
        __class__ = KeyError,
        messageFormat = message,
        formatArgs = args,
        __cause__ = cause,
      };
    }

    public static ExceptionType CreateKeyError(string message, params object[] args) {
      return CreateKeyError(null, message, args);
    }

    public static ExceptionWrapper ThrowKeyError(
        ExceptionType cause, string message, params object[] args) {
      throw new ExceptionWrapper(CreateKeyError(cause, message, args));
    }

    public static ExceptionWrapper ThrowKeyError(string message, params object[] args) {
      throw new ExceptionWrapper(CreateKeyError(message, args));
    }
    #endregion KeyError

    #region NotImplementedException
    public static ExceptionType CreateNotImplementedException(
        ExceptionType cause, string message, params object[] args) {
      return new ExceptionType {
        __class__ = NotImplementedException,
        messageFormat = message,
        formatArgs = args,
        __cause__ = cause,
      };
    }

    public static ExceptionType CreateNotImplementedException(
        string message, params object[] args) {
      return CreateNotImplementedException(null, message, args);
    }

    public static ExceptionWrapper ThrowNotImplementedException(
        ExceptionType cause, string message, params object[] args) {
      throw new ExceptionWrapper(CreateNotImplementedException(cause, message, args));
    }

    public static ExceptionWrapper ThrowNotImplementedException(
        string message, params object[] args) {
      throw new ExceptionWrapper(CreateNotImplementedException(message, args));
    }
    #endregion NotImplementedException

    #region SyntaxError
    public static ExceptionType CreateSyntaxError(
        ExceptionType cause, string message, params object[] args) {
      return new ExceptionType {
        __class__ = SyntaxError,
        messageFormat = message,
        formatArgs = args,
        __cause__ = cause,
      };
    }

    public static ExceptionType CreateSyntaxError(string message, params object[] args) {
      return CreateSyntaxError(null, message, args);
    }

    public static ExceptionWrapper ThrowSyntaxError(
        ExceptionType cause, string message, params object[] args) {
      throw new ExceptionWrapper(CreateSyntaxError(cause, message, args));
    }

    public static ExceptionWrapper ThrowSyntaxError(string message, params object[] args) {
      throw new ExceptionWrapper(CreateSyntaxError(message, args));
    }
    #endregion SyntaxError

    public static bool Check(ExceptionType ex, LispType type) {
      return LispType.IsInstance(ex, type);
    }
    #endregion

    public LispObject __cause__ { get; set; }
    public LispObject __context__ { get; set; }

    private string messageFormat;
    private object[] formatArgs;
    public string Message { get { return string.Format(messageFormat, formatArgs); } }
  }
}
