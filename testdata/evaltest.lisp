;; Woot.
(defun last (x)
  (if (cdr x)
    (progn
      (print "continue" x)
      (last (cdr x)))
    (progn
      (print "found last" x)
      (car x))))

(print '(last '(1 2 3)) (last '(1 2 3)))
(print '(last '(1 2 3 4)) (last '(1 2 3 4)))

(print '(let ((a 3)) a) (let ((a 3)) a))

(let ((b t))
  (if b 1 2))

(let ((c f))
  (if c 1 2))

(let ((d t))
  (if d 1))

(let ((e f))
  (if e 1))

(let ((x 1)
      (y 2)
      (z (last '(1 2 3))))
  (+ x y z))

(defun test-1 (x)
  (if x (print "x") (print "not x")))
