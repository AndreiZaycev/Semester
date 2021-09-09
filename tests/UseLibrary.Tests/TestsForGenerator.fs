module TestsForGenerator
open Generator
open Expecto
open System.IO

let readGeneratedMatrix file =
    let readLines = System.IO.File.ReadAllLines(file)
    Seq.map (fun (str: string) -> Seq.ofArray (str.Split(" "))) readLines
        
[<Tests>]
let treesOperations =
    testList "check all operations"
        [
            testProperty "test that write and read correctly for generated matrix"
            <| fun (rows, cols, _type) ->
                if rows > 0 && cols > 0 && _type > 0 
                then
                    let aType = _type % 3 
                    let helpFunction (y: Options) =
                        let matrixPath = Path.Combine(y._path, "Matrix0.txt")
                        generateSparseMatrix y |> ignore  
                        let firstReadMatrix = readGeneratedMatrix matrixPath
                        printMatrix firstReadMatrix matrixPath
                        let secondReadMatrix = readGeneratedMatrix matrixPath
                        let fst = Array.ofSeq (Seq.map (fun seq -> Array.ofSeq seq) firstReadMatrix)
                        let snd = Array.ofSeq (Seq.map (fun seq -> Array.ofSeq seq) secondReadMatrix)
                        Expect.equal fst snd "needs to be equal"
                        
                    match aType with
                    | 0 ->
                        let y = Generator.Options(rows, rows, 1, 0.5, __SOURCE_DIRECTORY__, Type.Int, System.Environment.ProcessorCount)
                        helpFunction y 
                    | 1 ->
                        let y = Generator.Options(rows, rows, 1, 0.5, __SOURCE_DIRECTORY__, Type.Float, System.Environment.ProcessorCount)
                        helpFunction y 
                    | 2 ->
                        let y = Generator.Options(rows, rows, 1, 0.5, __SOURCE_DIRECTORY__, Type.Bool, System.Environment.ProcessorCount)
                        helpFunction y
                    | _ -> ()
        ]

