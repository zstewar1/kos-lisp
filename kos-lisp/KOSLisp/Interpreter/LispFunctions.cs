using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZStewart.KOSLisp.Interpreter.Types;

namespace ZStewart.KOSLisp.Interpreter {
  public static class LispFunctions {
    /// <summary>
    /// Builitin lisp function to append two lists together.
    /// </summary>
    /// <param name="args">lisp arguments as a list.</param>
    /// <returns>A lisp list of the appended together lists.</returns>
    public static LispObject Append(LispList args) {
      // (defun append (:rest args)
      //     (if (nullp (cdr args))
      //         (cdr args)
      //       (if (not (listp (car args))) (error "Arugments to Append must be lists")
      //         (append-helper (car args) (apply append (cdr args))))))
      Preconditions.CheckNotNull(args);
      if (args.Cdr == LispNil.Nil) {
        return args.Car;
      }
      if (!(args.Car is LispList))
        // TODO(zstewar1): This will probably be hard to debug. Add more info?
        throw new InvalidOperationException("Arguments to Append must be lists");
      return AppendHelper(args.Car as LispList, Append(args.Cdr as LispList));
    }

    /// <summary>
    /// Internal helper for the Append function.
    /// </summary>
    /// <param name="copy">
    /// The list being appended to -- it needs to have all its cars copied to the new 
    /// list.
    /// </param>
    /// <param name="append">
    /// The list being appended -- it can just be consed onto the end.
    /// </param>
    /// <returns>
    /// A list with the elements of "append" added to the end of "copy".
    /// </returns>
    private static LispObject AppendHelper(LispObject copy, LispObject append) {
      // (defun append-helper (copy append)
      //     (when (not (listp copy)) (error "Append: argument was not a proper list."))
      //     (if (nullp copy) append
      //       (cons (car copy) (append-helper (cdr copy) append)))
      if (!(copy is LispList))
        // TODO(zstewar1): Better error messages?
        throw new InvalidOperationException("Append: argument was not a proper list.");
      var copyl = copy as LispList;
      if (copyl == LispNil.Nil) return append;
      return LispCons.Of(copyl.Car, AppendHelper(copyl.Cdr, append));
    }

    /// <summary>
    /// Returns true if "item" is a single-element list.
    /// </summary>
    /// <param name="item">A lisp object to check.</param>
    /// <returns>True if item is a cons cell and the cdr of item is nil.</returns>
    public static bool List1P(LispObject item) {
      // (defun list1p (item)
      //     (and (consp item) (nilp (cdr item))))
      return item is LispCons && (item as LispCons).Cdr == LispNil.Nil;
    }
  }
}
