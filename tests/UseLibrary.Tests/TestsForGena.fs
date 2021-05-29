module TestsForGena

open Generator

open bMatrix

open Expecto

open Main

let readGeneratedMatrix file =
    let readLines = System.IO.File.ReadAllLines(file)
    let OutputMtx = Array2D.zeroCreate readLines.Length (readLines.[0].Split(" ").Length - 1)
    for i = 0 to readLines.Length - 1 do
        let this = readLines.[i].Split(" ")
        for j = 0 to this.Length - 2 do
            OutputMtx.[i, j] <- this.[j]
    OutputMtx
        
[<Tests>]
let treesOperations =
    testList "check all operations"
        [
            testProperty "test that write and read correctly for gena"
            <| fun (rows, cols, _type) ->
                if rows > 0 && cols > 0 && _type > 0 
                then
                    let aType = _type % 3 
                    let helpFunction y =
                        let f = generateSparseMatrix y  
                        printMatrix f.[0] y._path
                        let intMatrix = readGeneratedMatrix y._path
                        let strMatrix = Array2D.zeroCreate (intMatrix.GetLength(0)) (intMatrix.GetLength(1))
                        for i = 0 to intMatrix.GetLength(0) - 1 do
                            for j = 0 to intMatrix.GetLength(1) - 1 do
                                strMatrix.[i,j] <- string intMatrix.[i,j]
                        Expect.equal f.[0] strMatrix "needs to be equal"
                        
                    match aType with
                    | 0 ->
                        let y = Generator.Options(rows, rows, 1, 0.5, "test.txt", Type.Int)
                        helpFunction y 
                    | 1 ->
                        let y = Generator.Options(rows, rows, 1, 0.5, "test.txt", Type.Float)
                        helpFunction y 
                    | 2 ->
                        let y = Generator.Options(rows, rows, 1, 0.5, "test.txt", Type.Bool)
                        helpFunction y
                    | _ -> ()
        ]

