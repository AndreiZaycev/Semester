module Loader

open Config
open Message
open Balancer
open TestsForGenerator
open System.IO

let read path = (readGeneratedMatrix path) |> Seq.map (fun elem -> Seq.map (fun elem1 -> int elem1) elem) |> array2D<_,_> 
    
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