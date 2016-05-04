(defun a (x) (if (cdr x) (a (cdr x)) (car x)))
(a '(1 2 3))
(a '(1 2 3 4))

(let ((a 3)) a)
