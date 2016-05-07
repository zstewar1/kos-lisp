;; Woot.
(defun a (x) (if (cdr x) (a (cdr x)) (car x)))
(a '(1 2 3))
(a '(1 2 3 4))

(let ((a 3)) a)

(let ((b t))
  (if b 1 2))

(let ((c f))
  (if c 1 2))

(let ((d t))
  (if d 1))

(let ((e f))
  (if e 1))
