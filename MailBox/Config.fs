module Config

type Config =
    val Files: string list
    val PrintPath: string
    val SparsityBound: float
    new (amount, loadPath, printPath, lowerBound) = {
            Files = 
                let fileNames = List.ofArray (System.IO.Directory.GetFiles(loadPath))
                let files =
                    match amount with
                    | x when x % 2 = 1 -> failwith "Number of files is not even"
                    | x when x <= 0 -> failwith "Amount should be positive integer"
                    | x when x * 2 > fileNames.Length -> failwith "Not enough files to load"
                    | _ -> fileNames.[ .. amount * 2 - 1]
                files
            PrintPath = printPath;
            SparsityBound = 
                if lowerBound > 1. || lowerBound <= 0.
                then failwith "Sparsity needs to be in range 0..1"
                else lowerBound;
    }
    new (loadPath, printPath, lowerBound) = {
            Files = List.ofArray (System.IO.Directory.GetFiles(loadPath))
            PrintPath = printPath;
            SparsityBound = 
                if lowerBound > 1. || lowerBound <= 0.
                then failwith "Sparsity needs to be in range 0..1"
                else lowerBound;
    }

    member this.defineMultiplication sparsityFirst sparsitySecond =
        match (sparsityFirst, sparsitySecond) with
            | i, j when i < this.SparsityBound && j < this.SparsityBound -> QuadTreeStandart
            | i, j when i < this.SparsityBound && j > this.SparsityBound -> QuadTreeParallel
            | i, j when i > this.SparsityBound && j > this.SparsityBound  -> ArrayParallel
            | i, j when i > this.SparsityBound && j < this.SparsityBound -> ArrayStandart
            | _, _ -> ArrayParallel
