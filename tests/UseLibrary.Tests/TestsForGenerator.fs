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
            <| fun (rows, cols, _type: Type, sparsity: float) ->
                if rows > 0 && cols > 0 
                then
                    let configuration = Generator.Options(rows, rows, 1, abs(sparsity) % 101., __SOURCE_DIRECTORY__, _type, System.Environment.ProcessorCount)
                    let matrixPath = Path.Combine(configuration._path, "Matrix0.txt")
                    generateSparseMatrix configuration  
                    let firstReadMatrix = readGeneratedMatrix matrixPath
                    printMatrix firstReadMatrix matrixPath
                    let secondReadMatrix = readGeneratedMatrix matrixPath
                    let fst = Array.ofSeq (Seq.map (fun seq -> Array.ofSeq seq) firstReadMatrix)
                    let snd = Array.ofSeq (Seq.map (fun seq -> Array.ofSeq seq) secondReadMatrix)
                    Expect.equal fst snd "needs to be equal"                                    
        ]

