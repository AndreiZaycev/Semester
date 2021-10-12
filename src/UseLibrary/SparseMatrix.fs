module SparseMatrix
open AlgebraicStruct
open System
open System.Collections.Generic
open bMatrix

let private getPowOfTwo number =
    let mutable aproxNum = 1

    while number > aproxNum do
        aproxNum <- aproxNum * 2

    aproxNum

type Triple<'t> =
    val coordinates: Pair
    val data: 't
    new (x, y, data) = {coordinates = (if x < 0 || y < 0 then failwith "expected positive" else Pair (x * 1<Row>, y * 1<Col>)); data = data}

type SparseMatrix<'t> =
    val numOfRows: int
    val numOfCols: int
    val notEmptyData: list<Triple<'t>> 
    new (k, p, lsts) = {numOfRows = k; numOfCols = p; notEmptyData = lsts}

type QuadTree<'t when 't : equality> =
    | None 
    | Leaf of 't 
    | Node of QuadTree<'t> * QuadTree<'t> * QuadTree<'t> * QuadTree<'t>

type QuadTreeMatrix<'t when 't : equality> = 
    val numOfRows: int
    val numOfCols: int
    val tree: QuadTree<'t>
    new (rows, cols, tr) = {
            numOfRows = if rows <= 0 || cols <= 0 then failwith "expected positive num of rows" else rows

            numOfCols = if rows <= 0 || cols <= 0 then failwith "expected positive num of cols" else cols

            tree = tr
        }
        
    override this.GetHashCode() =
        hash (this.numOfRows, this.numOfCols, this.tree)

    override this.Equals(t) =
        match t with
        | :? QuadTreeMatrix<'t> as t ->
            this.tree = t.tree
            && this.numOfRows = t.numOfRows

            && this.numOfCols = t.numOfCols

            && this.tree = this.tree
        | _ -> false

let matchTrees nw ne se sw =
    match nw, ne, se, sw with
    | None, None, None, None -> None
    | _, _, _, _ -> Node(nw, ne, se, sw)

let sum group (treeMtx: QuadTreeMatrix<'t>) (treeMtx1: QuadTreeMatrix<'t>) =
    if treeMtx.numOfRows <> treeMtx1.numOfRows || treeMtx.numOfCols <> treeMtx1.numOfCols
    then failwith "cannot sum various sizes"
    else
        let x, y = treeMtx.tree, treeMtx1.tree

        let neutral, operation =
            match group with
            | Monoid x -> x.neutral, x.sum
            | SemiRing x -> x.monoid.neutral, x.monoid.sum

        let rec _go x y = 
            match x, y with
            | Leaf a, Leaf b ->
                let current = operation a b

                if neutral = current then None else Leaf current
            | None, k -> k
            | k, None -> k
            | Node (tl, tl1, tl2, tl3), Node (tail, tail1, tail2, tail3) ->
                let first = _go tl tail

                let second = _go tl1 tail1

                let third = _go tl2 tail2

                let fourth = _go tl3 tail3

                matchTrees first second third fourth
            | _, _ -> failwith "cannot sum trees with different dimensions"

        QuadTreeMatrix(treeMtx.numOfRows, treeMtx.numOfCols, _go x y)

let private restruct (point: Pair) var1 var2 var3 var4 =
    Pair((int point.x - fst var1) * 1<Row>, (int point.y - snd var1) * 1<Col>),

    Pair((int point.x + fst var2) * 1<Row>, (int point.y - snd var2) * 1<Col>),

    Pair((int point.x - fst var3) * 1<Row>, (int point.y + snd var3) * 1<Col>),

    Pair((int point.x + fst var4) * 1<Row>, (int point.y + snd var4) * 1<Col>)

let toMatrix (tree: QuadTreeMatrix<'t>) =
    let size = getPowOfTwo (max tree.numOfRows tree.numOfCols)

    let outputHash = new HashSet<_>()

    let rec _go x (point: Pair) size =
        match x, size with
        | Leaf t, _ -> outputHash.Add(Triple(int point.x, int point.y, t)) |> ignore
        | None, _ -> ()
        | Node (q1, q2, q3, q4), 2 ->
            let nwPnt, swPnt, nePnt, sePnt = restruct point (1, 1) (0, 1) (1, 0) (0, 0)

            _go q1 nwPnt (size / 2); _go q2 nePnt (size / 2); _go q3 swPnt (size / 2); _go q4 sePnt (size / 2)
        | Node (q1, q2, q3, q4), _ ->
            let nwPnt, swPnt, nePnt, sePnt =
                restruct
                    point
                    (size / 4, size / 4)
                    (size / 4, size / 4)
                    (size / 4, size / 4)
                    (size / 4, size / 4)
                       
            _go q1 nwPnt (size / 2); _go q2 nePnt (size / 2); _go q3 swPnt (size / 2); _go q4 sePnt (size / 2)

    _go tree.tree (Pair((size / 2) * 1<Row>, (size / 2) * 1<Col>)) size

    SparseMatrix(tree.numOfRows, tree.numOfCols, List.ofSeq outputHash)

let private additionToNeedSize x iter =
    if x = None
    then x
    else
        let rec _go x cnt =
            match cnt with
            | 0 -> x
            | _ -> _go (Node(x, None, None, None)) (cnt - 1)
        _go x iter

let private reduce x iter =
    let rec _go x size =
        match x, size with
        | None, _ -> None
        | _, 0 -> x
        | Node(q1, None, None, None), _ -> _go q1 (size - 1)
        | Node(None, q1, None, None), _ -> _go q1 (size - 1)
        | Node(None, None, q1, None), _ -> _go q1 (size - 1)
        | Node(None, None, None, q1), _ -> _go q1 (size - 1)
        | _, _ -> failwith "cannot reduce this"
    _go x iter

let multiply group (tree: QuadTreeMatrix<'t>) (tree1: QuadTreeMatrix<'t>) =
    if tree.numOfCols <> tree1.numOfRows
    then failwith "cannot multiply because of different sizes"
    else
        let mutable flag = false

        let redefTree, redefTree1 =
            if getPowOfTwo tree.numOfCols > (max (getPowOfTwo tree.numOfRows) (getPowOfTwo tree1.numOfCols))
            then
                let maxsize = getPowOfTwo tree.numOfCols

                flag <- true

                QuadTreeMatrix(maxsize, maxsize, tree.tree),

                QuadTreeMatrix(maxsize, maxsize, tree1.tree)
            elif tree.numOfRows > tree1.numOfCols
            then 
                let maxSize = getPowOfTwo tree.numOfRows

                let difference = Math.Log2 ((maxSize / max (getPowOfTwo tree1.numOfCols) (getPowOfTwo tree1.numOfRows)) |> float) |> int

                QuadTreeMatrix(maxSize, maxSize, tree.tree),

                QuadTreeMatrix(maxSize, maxSize, additionToNeedSize tree1.tree difference)
            else
                let maxSize = getPowOfTwo tree1.numOfCols

                let difference = Math.Log2 ((maxSize / max (getPowOfTwo tree.numOfRows) (getPowOfTwo tree.numOfCols)) |> float) |> int

                QuadTreeMatrix(maxSize, maxSize, additionToNeedSize tree.tree difference),

                QuadTreeMatrix(maxSize, maxSize, tree1.tree)
            
        let neutral, operation =
            match group with
            | Monoid _ -> failwith "monoid cannot be in multiply"
            | SemiRing x -> x.monoid.neutral, x.mul

        let rec _go x y currSize =
            match x, y with
            | Leaf t, Leaf k ->
                let current = operation t k

                if neutral = current then None else Leaf current
            | None, _ | _, None -> None
            | Node (q, q1, q2, q3), Node (qu, qu1, qu2, qu3) ->
                let sum firstTree secondTree =
                    sum
                        group
                        (QuadTreeMatrix<'t>(currSize, currSize, firstTree))
                        (QuadTreeMatrix<'t>(currSize, currSize, secondTree))

                let size = currSize / 2

                let first = sum (_go q qu size) (_go q1 qu2 size) 

                let second = sum (_go q qu1 size) (_go q1 qu3 size) 

                let third = sum (_go q2 qu size) (_go q3 qu2 size) 

                let fourth = sum (_go q2 qu1 size) (_go q3 qu3 size) 

                matchTrees first.tree second.tree third.tree fourth.tree
            | _, _ -> failwith "cannot be in this case"

        let semiAnswer = _go redefTree.tree redefTree1.tree redefTree.numOfCols

        if flag
        then
            let dif = Math.Log2 ((getPowOfTwo tree.numOfCols) / (getPowOfTwo (max tree.numOfRows tree1.numOfCols)) |> float) |> int

            QuadTreeMatrix(tree.numOfRows, tree1.numOfCols, reduce semiAnswer dif)
        else QuadTreeMatrix(tree.numOfRows, tree1.numOfCols, semiAnswer)

let parallelMultiply group (tree: QuadTreeMatrix<'t>) (tree1: QuadTreeMatrix<'t>) =
    // made it a constant because I don't want to pass it as a function parameter
    let maxDeepness = 2
    if tree.numOfCols <> tree1.numOfRows
    then failwith "cannot multiply because of different sizes"
    else
        let mutable flag = false

        let redefTree, redefTree1 =
            if getPowOfTwo tree.numOfCols > (max (getPowOfTwo tree.numOfRows) (getPowOfTwo tree1.numOfCols))
            then
                let maxsize = getPowOfTwo tree.numOfCols

                flag <- true

                QuadTreeMatrix(maxsize, maxsize, tree.tree),

                QuadTreeMatrix(maxsize, maxsize, tree1.tree)
            elif tree.numOfRows > tree1.numOfCols
            then 
                let maxSize = getPowOfTwo tree.numOfRows

                let difference = Math.Log2 ((maxSize / max (getPowOfTwo tree1.numOfCols) (getPowOfTwo tree1.numOfRows)) |> float) |> int

                QuadTreeMatrix(maxSize, maxSize, tree.tree),

                QuadTreeMatrix(maxSize, maxSize, additionToNeedSize tree1.tree difference)
            else
                let maxSize = getPowOfTwo tree1.numOfCols

                let difference = Math.Log2 ((maxSize / max (getPowOfTwo tree.numOfRows) (getPowOfTwo tree.numOfCols)) |> float) |> int

                QuadTreeMatrix(maxSize, maxSize, additionToNeedSize tree.tree difference),

                QuadTreeMatrix(maxSize, maxSize, tree1.tree)
            
        let neutral, operation =
            match group with
            | Monoid _ -> failwith "monoid cannot be in multiply"
            | SemiRing x -> x.monoid.neutral, x.mul

        let rec _go x y currSize counterOfDeepness =
            match x, y with
            | Leaf t, Leaf k ->
                let current = operation t k

                if neutral = current then None else Leaf current
            | None, _ | _, None -> None
            | Node (q, q1, q2, q3), Node (qu, qu1, qu2, qu3) ->
                let sum firstTree secondTree =
                    sum
                        group
                        (QuadTreeMatrix<'t>(currSize, currSize, firstTree))
                        (QuadTreeMatrix<'t>(currSize, currSize, secondTree))

                let size = currSize / 2

                if counterOfDeepness < maxDeepness
                then
                    let first = async { return  sum (_go q qu size (counterOfDeepness + 1)) (_go q1 qu2 size (counterOfDeepness + 1)) }

                    let second = async { return sum (_go q qu1 size (counterOfDeepness + 1)) (_go q1 qu3 size (counterOfDeepness + 1)) }

                    let third = async { return sum (_go q2 qu size (counterOfDeepness + 1)) (_go q3 qu2 size (counterOfDeepness + 1)) } 

                    let fourth = async { return  sum (_go q2 qu1 size (counterOfDeepness + 1)) (_go q3 qu3 size (counterOfDeepness + 1)) }

                    let res = [first; second; third; fourth] |> Async.Parallel |> Async.RunSynchronously
                        
                    matchTrees res.[0].tree res.[1].tree res.[2].tree res.[3].tree
                else
                    let first = sum (_go q qu size counterOfDeepness) (_go q1 qu2 size counterOfDeepness) 

                    let second = sum (_go q qu1 size counterOfDeepness) (_go q1 qu3 size counterOfDeepness) 

                    let third = sum (_go q2 qu size counterOfDeepness) (_go q3 qu2 size counterOfDeepness) 

                    let fourth = sum (_go q2 qu1 size counterOfDeepness) (_go q3 qu3 size counterOfDeepness) 

                    matchTrees first.tree second.tree third.tree fourth.tree
            | _, _ -> failwith "cannot be in this case"

        let semiAnswer = _go redefTree.tree redefTree1.tree redefTree.numOfCols 0 

        if flag
        then
            let dif = Math.Log2 ((getPowOfTwo tree.numOfCols) / (getPowOfTwo (max tree.numOfRows tree1.numOfCols)) |> float) |> int

            QuadTreeMatrix(tree.numOfRows, tree1.numOfCols, reduce semiAnswer dif)
        else QuadTreeMatrix(tree.numOfRows, tree1.numOfCols, semiAnswer)

let multiplyScalar group (scalar: 't) (x: QuadTreeMatrix<'t>) =
    let neutral, operation =
        match group with
        | Monoid x -> x.neutral, x.sum
        | SemiRing x -> x.monoid.neutral, x.mul

    if scalar = neutral
    then QuadTreeMatrix<'t>(x.numOfRows, x.numOfCols, None)
    else
        let rec _go x =
            match x with
            | Leaf t -> Leaf (operation scalar t)
            | None -> None
            | Node (q, q1, q2, q3) -> Node (_go q, _go q1, _go q2, _go q3)

        QuadTreeMatrix(x.numOfRows, x.numOfCols, _go x.tree)

let tensorMultiply group (m1:QuadTreeMatrix<'t>) (m2:QuadTreeMatrix<'t>) =
    let rec go m1 =
        match m1 with
        | Leaf x -> (multiplyScalar group x m2).tree
        | None -> None
        | Node(nw, ne, se, sw) ->
            let nw = go nw

            let ne = go ne

            let se = go se

            let sw = go sw

            matchTrees nw ne se sw

    let t =
        match m1.tree, m2.tree with
        | None, _ | _, None -> None
        | _, _ -> go m1.tree

    QuadTreeMatrix(getPowOfTwo m1.numOfRows * getPowOfTwo m2.numOfRows, getPowOfTwo m1.numOfCols * getPowOfTwo m2.numOfCols, t)

let create (this: SparseMatrix<'t>) =
    let x =
        let max = getPowOfTwo (max this.numOfRows this.numOfCols)

        SparseMatrix(max, max, this.notEmptyData)
// fIter определяет 4 квадратных матрицы из одной, сравнивая все элементы с compPoint(которая является "центром" матрицы)
    let divideByPnt (mtx: SparseMatrix<'t>) (compPoint: Pair) =
        let list, list1, list2, list3 =
            List.fold
                (fun (nw, ne, sw, se) (elem: Triple<'t>) ->
                    if int elem.coordinates.x > int compPoint.x - 1   
                    then
                        if int elem.coordinates.y > int compPoint.y - 1  
                        then (nw, ne, sw, List.append se [elem])
                        else (nw, ne, List.append sw [elem], se)
                    elif int elem.coordinates.y > int compPoint.y - 1 
                    then (nw, List.append ne [elem], sw, se)
                    else (List.append nw [elem], ne, sw, se))
                ([],[],[],[])
                mtx.notEmptyData

        let numOfColsRows = mtx.numOfCols / 2

        (SparseMatrix(numOfColsRows, numOfColsRows, list), SparseMatrix(numOfColsRows, numOfColsRows, list1)),

        (SparseMatrix(numOfColsRows, numOfColsRows, list2), SparseMatrix(numOfColsRows, numOfColsRows, list3))

    let rec _go (point: Pair) (matrix: SparseMatrix<'t>) =
        match matrix.numOfCols, matrix.notEmptyData with
        | 1, _ ->
            if matrix.notEmptyData.IsEmpty
            then None
            else Leaf matrix.notEmptyData.[0].data
        | _, k when k.Length <> 0 ->
            let size = matrix.numOfCols

            let nwPnt, swPnt, nePnt, sePnt =
                restruct
                    point
                    (size / 4, size / 4)
                    (size / 4, size / 4)
                    (size / 4, size / 4)
                    (size / 4, size / 4)

            Node               
                (_go nwPnt (fst (fst (divideByPnt matrix point))),
                _go nePnt (snd (fst (divideByPnt matrix point))),
                _go swPnt (fst (snd (divideByPnt matrix point))),
                _go sePnt (snd (snd (divideByPnt matrix point))))
        | _, _ -> None
            
    QuadTreeMatrix(this.numOfRows, this.numOfCols, _go (Pair((x.numOfCols / 2) * 1<Row>, (x.numOfCols / 2) * 1<Col>)) x)
