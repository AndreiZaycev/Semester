module Matrix
open SparseMatrix
open bMatrix   

let evaluateSparsity (mtx: int[,]) =
    let mutable nons = 0.
    let numOfElems = mtx.GetLength(1) * mtx.GetLength(0)
    for i in 0 .. mtx.GetLength(0) - 1 do
        for j in 0 .. mtx.GetLength(1) - 1 do
            if mtx.[i, j] = 0
            then nons <- nons + 1.
    nons / float numOfElems

let size (mtx: int[,]) = Array2D.length1 mtx + Array2D.length2 mtx

let quadTreeTo2D quad = 
    let fTree = toMatrix quad
    let output = Array2D.zeroCreate fTree.numOfCols fTree.numOfRows
    for i in fTree.notEmptyData do
        output.[int i.coordinates.x, int i.coordinates.y] <- i.data
    output

let specialWrite path quad = writeOutputMatrix (quadTreeTo2D quad) path

let writeMatrix path matrix = writeOutputMatrix matrix path

let arrayParallelMultiply (mtx1: int[,]) (mtx2: int[,]) =
    if mtx1.GetLength 1 = mtx2.GetLength 0 then
          let res = Array2D.zeroCreate (mtx1.GetLength 0) (mtx2.GetLength 1) 
          [ for i in 0 .. mtx1.GetLength 0 - 1 ->
              async {
                  do
                      for j in 0 .. mtx2.GetLength 1 - 1 do
                          for k in 0 .. mtx1.GetLength 1 - 1 do
                              res.[i, j] <- res.[i, j] + mtx1.[i, k] * mtx2.[k, j]
              }
          ]
          |> Async.Parallel
          |> Async.RunSynchronously
          |> ignore
          res
      else failwith "Wrong sizes of matrixes"