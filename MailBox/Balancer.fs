module Balancer

open Config
open Message
open SparseMatrix
open Multiplier
open AlgebraicStruct
open Quads
open Matrix

let quadTreeInitialize (matrix: int[,]) = 
    let sparseMatrix = createEM matrix
    create (toMatrix (create sparseMatrix))

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
                    let maxSize = max (size fst) (size snd)
                    match config.defineMultiplication (evaluateSparsity fst) (evaluateSparsity snd) maxSize with
                        | QuadTreeParallel -> quadTreeParallel.Post pair 
                        | QuadTreeStandart -> quadTreeStandart.Post pair
                        | ArrayParallel -> arrayParallel.Post pair
                        | ArrayStandart -> arrayStandart.Post pair
                    return! loop ()
            }
        loop ()
    )