module Loader

open Config
open Message
open Balancer
open TestsForGenerator
open System.IO

let read path = 
    let fst = Array.ofSeq (Seq.map (fun seq -> Array.ofSeq (Seq.map (fun elem -> int elem) seq)) (readGeneratedMatrix path))
    let array2D = Array2D.zeroCreate fst.[0].Length fst.Length
    for i in 0 .. fst.[0].Length - 1 do 
        for j in 0 .. fst.Length - 1 do 
            array2D.[i, j] <- fst.[i].[j]
    array2D

let loader (config: Config) =
    let balancer = balancer config
    MailboxProcessor.Start(fun (inbox: MailboxProcessor<LoaderMessage>) ->
        let rec loop input =
            async {
                let! msg = inbox.Receive()
                match msg with
                | EOS ch ->
                    balancer.PostAndReply BalancerMessage.EOS
                    ch.Reply()
                | Continue ch as msg ->
                    match input with
                    | [] ->
                        inbox.Post (EOS ch)
                        return! loop input
                    | fst :: snd :: tail ->
                        balancer.Post 
                            (NamedPair 
                                ((read fst, Path.GetFileNameWithoutExtension(fst)), 
                                (read snd, Path.GetFileNameWithoutExtension(snd)))
                            )
                        inbox.Post msg
                        return! loop tail
                    | _ -> failwith "Number of files not even"
            }
        loop config.Files
    )