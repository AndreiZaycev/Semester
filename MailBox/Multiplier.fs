module Multiplier

open Message
open Config

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