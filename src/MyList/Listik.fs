module Listik

type MyList<'t> =
    | One of 't
    | Cons of 't * MyList<'t>

type MyString = MyList<char>

let rec fold f acc x =
    match x with
    | One t -> f acc t  
    | Cons (hd, tl) -> fold f (f acc hd) tl
    
let length x =
    fold (fun acc elem -> acc +  1) 0 x

let toMyList x = 
    if List.length x < 1
    then failwith "use correct list"
    else
        let y = List.rev x
        List.fold (fun acc elem -> if x = [] then acc else Cons (elem, acc)) (One y.[0]) (y.Tail)

let  toDefoltList x =        
    List.rev (fold (fun acc elem -> elem :: acc) [] x)

let generator t =
    if t < 1
    then failwith "MyList cannot be created because input values uncorrect"
    else (toMyList (List.init t (fun _ -> System.Random().Next())))

let rec concat x y =
    match x with
    | One t -> Cons (t, y)
    | Cons (i, o) -> Cons (i, concat o y)

let sort x = 
    let rec _go x =
        match x with
        | One t -> x
        | Cons (head, Cons (head2, tl)) when head > head2 -> (Cons (head2, _go (Cons (head, tl))))
        | Cons (head, Cons (head2, tl)) -> (Cons (head, _go (Cons (head2, tl))))
        | Cons (head, One t) when head < t -> Cons (head, One t)
        | Cons (head, One t) -> Cons (t, One head)  
    let rec _go1 x k =
        match k with
        | k when (k = length x) -> x
        | _ -> _go1 (_go x) (k + 1)
    _go1 (_go x) 0
           
let rec iter f x =
    match x with
    | One t -> f t
    | Cons (i, o) ->
        f i
        iter f o

let rec map f x =
    match x with
    | One t -> One (f t)
    | Cons (i, o) -> Cons (f i, map f o)

let concatMyString (x: MyString) (y: MyString) =
    (concat x y): MyString 

let toMyString (str: string) =
    let k = [for i in str -> i]
    toMyList k:MyString

let toString (x: MyString) =
    fold (fun acc elem -> acc + string elem) "" x
