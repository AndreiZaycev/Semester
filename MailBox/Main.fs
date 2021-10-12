open Argu
open Config
open Message

    type CLIArguments =
        | [<Mandatory>] ReadDirectory of path: string
        | [<Mandatory>] WriteDirectory of path: string
        | [<Mandatory>] SparsityBound of sparsity: float
        | Amount of amount: int
        interface IArgParserTemplate with
            member s.Usage =
                match s with
                | ReadDirectory _ -> "Directory to read matrices"
                | WriteDirectory _ -> "Directory to write matrices"
                | Amount _ -> "The number of multiplied matrices"
                | SparsityBound _ -> "Indicates multiply strategy by this value"

    [<EntryPoint>]
    let main (argv: string array) =
        try
            let parser = ArgumentParser.Create<CLIArguments>(programName = "MailBox")
            let args = parser.ParseCommandLine argv
            let readDirectory, writeDirectory = args.GetResult(ReadDirectory), args.GetResult(WriteDirectory)
            if not (System.IO.Directory.Exists(readDirectory)) || not (System.IO.Directory.Exists(writeDirectory))
            then failwith "Invalid directories"
            if args.Contains(Amount)
            then
                let config =
                    let sparsity = args.GetResult(SparsityBound)
                    if args.Contains(Amount)
                    then Config(args.GetResult(Amount), readDirectory, writeDirectory, sparsity)
                    else Config(readDirectory, writeDirectory, sparsity)
                (Loader.loader config).PostAndReply Continue              
            0
        with
        | :? ArguParseException as ex ->
            printfn $"%s{ex.Message}"
            1