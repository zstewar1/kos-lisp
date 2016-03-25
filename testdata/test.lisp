(defun b (asdf bsd)
  (cons (car asdf) bsd))

(b '(sdf
     sadf
     bldsf
     lsdk)
   'c)

34

"test string"

("test string 34" 765)

"Test \n escape \r \" 'sdf '"

'x
'(y z)
(quote a)
(quote (a b))
(quote a b)
(quote (a b) c)
(quote (a . b))
(quote (a b) . c)
(quote a . b)

`(a b ,c d)
`(a b ,(c d) e)
`(a b ,@(c d) e)
`(a b ,@c d)
`(a ,(`,b))

(let ((j 3.14)
      (k 50)
      (l -10)
      (m -4.4)
      (n -.5)
      (osdf 1.5e34)
      (ad -1.4e35)
      (ae -1.6e-34)
      (af 9.7e50)
      (jd 'lsd)
      (jdk '(sdaf sl 34 15.5)))
  (+ k l))

(a . b)
(a.b . c)
