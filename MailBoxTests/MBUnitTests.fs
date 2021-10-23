module MBUnitTests
open System.IO
open Expecto
open Generator
open Config
open Loader
open Message

[<Literal>]
let generatedMatrices = __SOURCE_DIRECTORY__ + "/generatedMatrices"
[<Literal>]
let multipliedMatrices = __SOURCE_DIRECTORY__ + "/multipliedMatrices"

Directory.CreateDirectory(generatedMatrices) |> ignore
Directory.CreateDirectory(multipliedMatrices) |> ignore 

let generateAndMultiply size amt =
    let generatorConfig = Options(size, size, amt, 0.5, generatedMatrices, Int, 8)
    generateSparseMatrix(generatorConfig)
    let config = Config(amt, generatedMatrices, multipliedMatrices, 0.6)
    (Loader.loader config).PostAndReply Continue 
    Array.iter (fun file -> File.Delete(file)) (Directory.GetFiles(generatedMatrices))
    config.Files.Length

[<Tests>]
let tests =
    testSequenced (testList "Mailbox tests"
        [
            testProperty "Dimensions should be uqual"
            <| fun ((size, amt): int * int) ->
                if (amt % 2 = 0) && (amt > 0) && (size > 0)
                then
                    generateAndMultiply size amt |> ignore
                    Array.iter 
                        (fun elem -> 
                            let mtx = read elem
                            Expect.equal (mtx.GetLength(0)) size ""
                            Expect.equal (mtx.GetLength(1)) size ""
                            File.Delete(elem))
                        (Directory.GetFiles(multipliedMatrices))  
            testProperty "Amount of multiplied matrices should be equal amount * 2 generated"
            <| fun ((size, amt): int * int) ->
                if (amt % 2 = 0) && (amt > 0) && (size > 0)
                then
                    let len = generateAndMultiply size amt
                    Array.iter (fun elem -> File.Delete(elem)) (Directory.GetFiles(multipliedMatrices))
                    Expect.equal len amt ""
        ])
