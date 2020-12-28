module Tree

type MyTree<'t> =
    | Leaf of 't
    | Node of 't * Listik.MyList<MyTree<'t>>

let rec fold f acc x =
    match x with
    | Leaf t -> f acc t
    | Node (hd, tl) -> Listik.fold (fun acc1 elem -> fold f acc1 elem) (f acc hd) tl
      
let average x =
    fold (fun acc1 elem -> acc1 + elem) 0 x / fold (fun acc1 elem -> acc1 + 1) 0 x

let max x =
    fold (fun acc1 elem -> max acc1 elem) System.Int32.MinValue x

