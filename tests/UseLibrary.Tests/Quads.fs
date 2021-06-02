module Quads

open AlgebraicStruct

open SparseMatrix

open bMatrix

open test

open Expecto

// генерирует кортеж из int[,] и ее расширенной матрицы
let generator k i =
    let reconstruct, reconstruct1 = generateRandomBoolAndDefoltMtx k i
    let x = List.map (fun (elem: Pair) ->
        Triple (int elem.x, int elem.y, (System.Random().Next()))) reconstruct1.notEmptyData
    for j in x do reconstruct.[int j.coordinates.x, int j.coordinates.y] <- j.data
    reconstruct, SparseMatrix(reconstruct1.numOfRows, reconstruct1.numOfCols, x)

let createEM (x: int[,]) =
    let mutable counter = 0  
    for i in 0 .. Array2D.length1 x - 1 do
        for j in 0 .. Array2D.length2 x - 1 do
            if x.[i,j] = 0 then counter <- counter + 1
    let arrOfData = Array.create (Array2D.length2 x * Array2D.length1 x - counter) (Triple (1, 1, 0))
    counter <- 0 
    for i in 0 .. Array2D.length1 x - 1 do
        for j in 0 .. Array2D.length2 x - 1 do
            if x.[i,j] <> 0 then 
                arrOfData.[counter] <- Triple (i, j, x.[i,j])
                counter <- counter + 1
    SparseMatrix(Array2D.length1 x, Array2D.length2 x, List.ofArray arrOfData)

// создает ExtendedMatrix на основе матрицы, в которой если value <> 0 существуют все элементы
let generatorOneValue k i value =
    let mutable counter = 0
    let mtx = Array2D.create k i value
    let arr = Array.create (k * i) (Triple(-1, -1, 0))
    for i in 0 .. Array2D.length1 mtx - 1 do
        for j in 0 .. Array2D.length2 mtx - 1 do
            arr.[counter] <- Triple (i, j, value)
            counter <- counter + 1
    SparseMatrix(k, i, List.ofArray arr)


let sumMtx o (t: int[,]) =
    for i in 0 .. Array2D.length1 o - 1 do
        for j in 0 .. Array2D.length2 o - 1 do
            o.[i,j] <- o.[i,j] + t.[i,j]
    o

let m (o: int[,]) (t: int[,]) =
    let first = Array2D.length2 o
    let second = Array2D.length1 o 
    let third = Array2D.length1 t 
    let fourth = Array2D.length2 t
    if first = third
    then
        let mtx = Array2D.zeroCreate second fourth
        for i in 0 .. second - 1 do 
            for j in 0 .. fourth - 1 do 
                for k in 0 .. first - 1 do 
                    mtx.[i,j] <- mtx.[i,j] + (t.[k,j] * o.[i,k])
        mtx      
    else failwith "Cannot multiply this"

let multiplyByScalar alpha x =
    let y = Array2D.copy x 
    for i in 0 .. Array2D.length1 x - 1 do
        for j in 0 .. Array2D.length2 x - 1 do
            y.[i,j] <- alpha * y.[i,j]
    y

let tensor (m1: int[,]) (m2: int[,]) =
    let rows = m1.GetLength 0 * m2.GetLength 0
    let columns = m1.GetLength 1 * m2.GetLength 1
    let res = Array2D.zeroCreate rows columns
    for i in 0..rows - 1 do
        for j in 0..columns - 1 do
            res.[i,j] <- m1.[i / m2.GetLength 0, j / m2.GetLength 1 ] * m2.[i % m2.GetLength 0, j % m2.GetLength 0]
    res
// для теств подойдет обычное полукольцо с сложением и умножением интовым
let y = new Monoid<int>((+), 0)
let x = new SemiRing<int>(y, (*))
let group = SemiRing x

[<Tests>]
let treesOperations =
    testList "check all operations"
        [
            testProperty "tomatrix test"
            <| fun (k: int) ->
                if k > 0 && abs k < 6
                then
                    let x = generator (pown 2 (abs k)) (pown 2 (abs k))
                    let y = (QuadTreeMatrix.create (QuadTreeMatrix.toMatrix (QuadTreeMatrix.create (snd x))))
                    let z = QuadTreeMatrix.create (snd x)
                    Expect.equal y z "needs to be equal"

            testProperty "tensor mult on matrix and on trees id №2"
            <| fun (k: int) ->
                if k <> 0 && abs k < 4
                then
                    let x = generator (pown 2 (abs k)) (pown 2 (abs k))
                    let y = generator (pown 2 (abs k)) (pown 2 (abs k))   
                    Expect.equal (QuadTreeMatrix.create (createEM (tensor (fst x) (fst y)))) (QuadTreeMatrix.tensorMul group (QuadTreeMatrix.create (snd x)) (QuadTreeMatrix.create (snd y))) "needs to be equal"

            testProperty "standart mult matrix and on trees id"
            <| fun (k: int, t: int, p: int) ->
                if k <> 0 && abs k < 32 && t <> 0 && abs t < 32 && p <> 0 && abs p < 32
                then
                    let x = generator (abs t) (abs k)
                    let y = generator (abs k) (abs p)
                    let output = Array2D.zeroCreate (abs t) (abs p)
                    let fTree =
                        QuadTreeMatrix.toMatrix
                            (QuadTreeMatrix.multiply group (QuadTreeMatrix.create (snd x)) (QuadTreeMatrix.create (snd y)))

                    for i in fTree.notEmptyData do
                        output.[int i.coordinates.x, int i.coordinates.y] <- i.data
                    
                    let sTree = m (fst x) (fst y)
                    Expect.equal output sTree "needs to be equal"

            testProperty "standart mult matrix by scalar and on trees id"
            <| fun (k: int, scalar: int) ->
                if k <> 0 && abs k < 7
                then
                    let x = generator (pown 2 (abs k)) (pown 2 (abs k))
                    Expect.equal (QuadTreeMatrix.create (createEM (multiplyByScalar scalar (fst x)))) (QuadTreeMatrix.multiplyScalar group scalar (QuadTreeMatrix.create (snd x))) "id"

            testProperty "standart sum matrix and on trees id"
            <| fun (k: int) ->
                if k <> 0 && abs k < 7
                then
                    let x = generator (pown 2 (abs k)) (pown 2 (abs k))
                    let y = generator (pown 2 (abs k)) (pown 2 (abs k))
                    Expect.equal (QuadTreeMatrix.create (createEM (sumMtx (fst x) (fst y)))) (QuadTreeMatrix.sum group (QuadTreeMatrix.create (snd x)) (QuadTreeMatrix.create (snd y))) "needs to be equal"
        ]

