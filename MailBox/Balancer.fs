module Balancer

open Config
open Message
open SparseMatrix
open Multiplier
open AlgebraicStruct
open Quads

let evaluateSparsity (mtx: int[,]) =
    let mutable nons = 0.
    let numOfElems = mtx.GetLength(1) * mtx.GetLength(0)
    for i in 0 .. mtx.GetLength(0) - 1 do
        for j in 0 .. mtx.GetLength(1) - 1 do
            if mtx.[i, j] = 0
            then nons <- nons + 1.
    nons / float numOfElems

let balancer (config: Config) =
    let y = new Monoid<int>((+), 0)
    let x = new SemiRing<int>(y, (*))
    let group = SemiRing x
    let quadTreeParallel =
        multiplier (parallelMultiply group) quadTreeInitialize config specialWrite
    let quadTreeStandart = 
        multiplier (SparseMatrix.multiply group) quadTreeInitialize config specialWrite
    let arrayParallel =
        multiplier arrayParallelMultiply id config writeMatrix
    // m is old function that multiplies matrices
    let arrayStandart = multiplier m id config writeMatrix

    MailboxProcessor.Start(fun (inbox: MailboxProcessor<BalancerMessage>) ->
        let rec loop () =
            async {
                let! msg = inbox.Receive()
                match msg with
                | BalancerMessage.EOS ch ->                   
                    quadTreeParallel.PostAndReply BalancerMessage.EOS 
                    quadTreeStandart.PostAndReply BalancerMessage.EOS 
                    arrayParallel.PostAndReply BalancerMessage.EOS
                    arrayStandart.PostAndReply BalancerMessage.EOS
                    ch.Reply()
                | NamedPair ((fst, _), (snd, _)) as pair ->
                    match config.defineMultiplication (evaluateSparsity fst) (evaluateSparsity snd) with
                        | QuadTreeParallel -> quadTreeParallel.Post pair 
                        | QuadTreeStandart -> quadTreeStandart.Post pair
                        | ArrayParallel -> arrayParallel.Post pair
                        | ArrayStandart -> arrayParallel.Post pair
                    return! loop ()
            }
        loop ()
    )