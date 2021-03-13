module calculations

open Main

open Interpretator

open Expecto

open System

open System.Collections.Generic

open BigAriphmetics

let sRun ast =
    let vDict = new Dictionary<_,_>()
    List.fold processStmt vDict ast |> ignore
    toInt vDict.[Exp.VName.Var("x")]
    

[<Tests>]
let parsingString =
    testList "check all operations"
        [
            testCase "test (+) parsing"
            <| fun _ ->                
                let subject =
                    parse
                        ("let [x] = " + "12 + 3 - 5 + 12 - 25 * (25 + 3) / 14 - 14 * 6 + 7 / 12 - 12 / 2 / 2")
                let expectedAns = -115
                let ans = sRun subject                
                Expect.equal ans expectedAns "needs to be equal"

            testCase "test1"
            <| fun _ ->                
                let subject = parse ("let [x] = " + "2 + 3 + 4 - 2 - 2 - 2 - 3 * (2 - 3 - 3 + 4) + 12 / 2 / 2")
                let expectedAns = 6
                let ans = sRun subject 
                Expect.equal ans expectedAns "needs to be equal"

            testCase "test2"
            <| fun _ ->                
                let subject = parse ("let [x] = " + "228 - 1337 * 666 + 69")
                let expectedAns = -890145
                let ans = sRun subject              
                Expect.equal ans expectedAns "needs to be equal"

            testCase "unar - test"
            <| fun _ ->                
                let subject = parse ("let [x] = " + "-2")
                let expectedAns = -2
                let ans = sRun subject               
                Expect.equal ans expectedAns "needs to be equal"

            testCase "test3"
            <| fun _ ->                
                let subject = parse ("let [x] = " + "(2 + 3) - (2 + 3)")
                let expectedAns = 0
                let ans = sRun subject              
                Expect.equal ans expectedAns "needs to be equal"

            testCase "test4"
            <| fun _ ->                
                let subject = parse ("let [x] = " + "2 - 3 - 3")
                let expectedAns = -4
                let ans = sRun subject             
                Expect.equal ans expectedAns "needs to be equal"

            testCase "priorities"
            <| fun _ ->                
                let subject = parse ("let [x] = " + "1 + 2 * 3 - 7 / (2 + 5)")
                let expectedAns = 6
                let ans = sRun subject             
                Expect.equal ans expectedAns "needs to be equal"

            testCase "prioritiesModule"
            <| fun _ ->                
                let subject = parse ("let [x] = " + "{-2 - 2 * 3} + 7")
                let expectedAns = 15
                let ans = sRun subject             
                Expect.equal ans expectedAns "needs to be equal"

            testCase "binary"
            <| fun _ ->                
                let subject = parse ("let [x] = " + "# 12 + 4 - 12 / 12 + 1")
                let expectedAns = 10000
                let ans = sRun subject             
                Expect.equal ans expectedAns "needs to be equal"

            testCase "pow priority"
            <| fun _ ->                
                let subject = parse ("let [x] = " + "2 / 2 ^ 3")
                let expectedAns = 0
                let ans = sRun subject             
                Expect.equal ans expectedAns "needs to be equal"

            testCase "div rem"
            <| fun _ ->                
                let subject = parse ("let [x] = " + "143 % 10 + 2")
                let expectedAns = 5
                let ans = sRun subject             
                Expect.equal ans expectedAns "needs to be equal"

        ]
