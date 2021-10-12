module Multiplier

open Message
open Quads
open SparseMatrix
open bMatrix
open Config

type MultiplyingMethod =
    | QuadTreeParallel
    | QuadTreeStandart
    | ArrayParallel
    | ArrayStandart

let quadTreeInitialize (matrix: int[,]) = 
    let sparseMatrix = createEM matrix
    create (toMatrix (create sparseMatrix))   

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

let multiplier strategyMultiply toDefinedType (config: Config) print =
    MailboxProcessor.Start(fun inbox ->
        let rec loop () = 
            async {
                let! msg = inbox.Receive()
                match msg with
                | BalancerMessage.EOS ch ->
                    ch.Reply()
                    return! loop ()
                | NamedPair ((fst, firstName), (snd, secondName)) ->
                    let res = strategyMultiply (toDefinedType fst) (toDefinedType snd)  
                    print (System.IO.Path.Join (config.PrintPath, $"{firstName}_X_{secondName}.txt")) res 
                    return! loop ()             
            }
        loop ()
    )