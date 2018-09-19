(quote "problem 1")
#;1
(define (count x xs)
  (define (iter cnt xs)
       (if (null? xs)
           cnt
           (if (equal? x (car xs))
               (iter (+ cnt 1)(cdr xs))  
               (iter cnt (cdr xs)) 
               )
        )
    )
  (iter 0 xs)
  )

(count 'a '(a b c a))
(count 'b '(a c d))

(quote "problem 2")
#;2


(define (replace pred? proc xs)
  (define (iter xs nw)
    (if (null? xs)
        nw
        (if (pred? (car xs))
            (iter (cdr xs) (append nw (list(proc (car xs)))))
            (iter (cdr xs) (append nw (list(car xs))))
            )
        )
    )
  (iter xs '())
  )

(replace zero? (lambda (x) (+ x 1)) '(0 1 2 3 0))
(replace odd? (lambda (x) (* 2 x)) '(1 2 3 4 5 6))
(replace even? (lambda (x) (/ x 2)) '(1 3 5 7))
(replace (lambda (x) (> x 0)) exp '(1 2 3 -4))

(quote "problem 3")
#;3
(define (replicate xs num)
  (define (iter e nw)
    (if (= e num)
        nw
        (iter (+ e 1) (append nw (list xs)))
    )
  )
  (iter 0 '())
 )
(replicate 'a 0)
(replicate 'a 5)
(replicate '(a b) 3)

(quote "problem 4")
#;4

(define (cycle xs num)
   (define (iter e nw)
     (if (= e num)
         nw
         (iter (+ e 1) (append nw xs))
      )
   )
   (iter 0 '())
)

(cycle '(a) 3)
(cycle '(a b) 4)
(quote "problem 5")
#;5
(define (and-fold a b c)
  (and a (and b c))
  )

(define (or-fold a b c)
  (or a (or b c))
  )
(and-fold #t #t #f)
(or-fold #f #f #f)
(quote "problem 6")
#;6
(define (f x) (* x 2))
(define (g x) (* x 3))
(define (h x) (- x))

(define (o f1 . rest-f)
   
  (if (null? rest-f)

      (if (null? f1) (lambda(x) (x)) (lambda (x) (f1 x)))
      (lambda (x) (f1 ((apply o rest-f) x) ))
      )
  )

((o f g h) 1)
((o f g) 1)
((o h) 1)
;((o) 1)
(quote "problem 7")
#;7

(define (find-number a b c)

  (define ans (+ (- c (modulo a c)) a))
  (cond ((= (modulo a c) 0) a)
          ((> ans b) #f)
          ((<= ans b) ans)
         )
  )

(find-number 0 5 2)
(find-number 7 9 3)
(find-number 4 9 4)
(find-number 3 7 9)
