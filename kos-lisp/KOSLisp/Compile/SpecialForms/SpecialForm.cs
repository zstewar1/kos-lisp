using ZStewart.KOSLisp.Compile.AST;
using ZStewart.KOSLisp.Compile.Contexts;
using ZStewart.KOSLisp.Types;

namespace ZStewart.KOSLisp.Compile.SpecialForms {
  /// <summary>
  /// An interface for converting S-Expressions in Lisp Cons-lists into AST expressions.
  ///
  /// Special forms are the basis of the language. They represent operations which cannot
  /// be expressed as a combination of more basic structures. Functions transform data,
  /// and Macros transform code, but SpecialForms are the base language that all code must
  /// eventually conform to after macro expansion.
  ///
  /// Most special forms should be absolute primitives: function declaration, conditional
  /// operation, etc. Some are slightly non-primitive in that they can be expressed as a
  /// combination of other special forms. Such non-primitive forms are allowed to exist
  /// when it would be much more difficult and much less efficient to express the same
  /// thing as a macro which expands to the more primitive special form.
  ///
  /// Examples of non-primitive special forms include defun, let, and progn. All three of
  /// these forms can be (mostly) expressed in terms of lambda:
  ///
  /// progn is a lambda containing the forms which is immediately called with no
  /// arguments.
  /// (defmacro progn (&rest forms) `((lambda () ,@forms)))
  ///
  /// Let is a more complicated expression which binds the variables from the bindings
  /// expression and calls the lambda with the expressions that they are bound to.
  /// (defmacro let (bindings &rest forms)
  ///   `((lambda (,@(map
  ///                  (lambda (binding) (if (symbol? binding) binding (car binding)))
  ///                  bindings))
  ///             ,@forms)
  ///     ,@(map (lambda (binding) (if (symbol? binding) nil (cadr binding))) bindings)))
  ///
  /// Defun is comparatively simple:
  /// (defmacro defun (name &rest fdef) `(set ,name (lambda ,@fdef)))
  ///
  /// The progn and let get their own special forms for efficiency reasons: we don't want
  /// the extra stack-frame and function call just to evaluate a series of expressions in
  /// sequence.
  ///
  /// Defun gets its own special form due to a minor semantic difference: defun defines a
  /// function with a name matching the name of the variable it is assigned to.
  /// </summary>
  public interface SpecialForm {
    /// <summary>
    /// Convert the given expression to an AST.
    /// </summary>
    /// <param name="expression">
    /// The lisp object representing the expression to convert.
    /// </param>
    /// <param name="context">
    /// Variable context of the outer expression. Can be used to look up variables which
    /// this expression does not itself bind.
    /// </param>
    /// <param name="compiler">
    /// The Lisp compiler, which this special form can use to parse sub-expressions.
    /// </param>
    AstOp ToAst(LispObject expression, Context context, Compiler compiler);
  }
}
