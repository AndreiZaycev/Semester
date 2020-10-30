module Tests3
open Expecto
open System
[<Tests>]
let task7 =
    testList "task7"
        [
           testCase "First Fibonacci number, first task" <| fun _ ->
               let subject = domashka2.Task7 1
               Expect.equal subject 1 "rфывфывesult must be equal 1"
           testCase "Second Fibonacci number, first task" <| fun _ ->
               let subject = domashka2.Task7 2
               Expect.equal subject 1 "result must be equal 1"
           testCase "Third Fibonacci number, first task" <| fun _ ->
               let subject = domashka2.Task7 3
               Expect.equal subject 2 "result must be equal 2"
           testCase "Twelveth Fibonacci number, first task" <| fun _ ->
               let subject = domashka2.Task7 12
               Expect.equal subject 144 "result must be equal 144"
        ]
[<Tests>]
let task8 =
    testList "task8"
        [
           testCase "First Fibonacci number, second task" <| fun _ ->
               let subject = domashka2.Task8 1
               Expect.equal subject 1 "result must be equal 1"
           testCase "Second Fibonacci number, second task" <| fun _ ->
               let subject = domashka2.Task8 2
               Expect.equal subject 1 "result must be equal 1"
           testCase "Third Fibonacci number, second task" <| fun _ ->
               let subject = domashka2.Task8 3
               Expect.equal subject 2 "result must be equal 2"
           testCase "Twelveth Fibonacci number, second task" <| fun _ ->
               let subject = domashka2.Task8 12
               Expect.equal subject 144 "result must be equal 144"
        ]
[<Tests>]
let task9 =
    testList "task9"
        [
           testCase "First Fibonacci number, third task" <| fun _ ->
               let subject = domashka2.Task9 1
               Expect.equal subject 1 "result must be equal 1"
           testCase "Second Fibonacci number, third task" <| fun _ ->
               let subject = domashka2.Task9 2
               Expect.equal subject 1 "result must be equal 1"
           testCase "Third Fibonacci number, third task" <| fun _ ->
               let subject = domashka2.Task9 3
               Expect.equal subject 2 "result must be equal 2"
           testCase "Twelveth Fibonacci number, third task" <| fun _ ->
               let subject = domashka2.Task9 12
               Expect.equal subject 144 "result must be equal "
        ]
[<Tests>]
let task10 =
    testList "task10"
        [
           testCase "First Fibonacci number, fourth task" <| fun _ ->
               let subject = domashka2.Task10 1
               Expect.equal subject  1 "result must be equal 1"
           testCase "Third Fibonacci number, fourth task" <| fun _ ->
               let subject = domashka2.Task10 3
               Expect.equal subject 2 "result must be equal 2"
           testCase "Twelveth Fibonacci number, fourth task" <| fun _ ->
               let subject = domashka2.Task10 12
               Expect.equal subject 144 "result must be equal 144"
        ]
[<Tests>]
let task11 =
    testList "task11"
        [
           testCase "First Fibonacci number, fifth task" <| fun _ ->
               let subject = domashka2.Task11 1
               Expect.equal subject 1 "result must be equal 1"          
           testCase "Third Fibonacci number, fifth task" <| fun _ ->
               let subject = domashka2.Task11 3
               Expect.equal subject 2 "result must be equal 2"
           testCase "Twelveth Fibonacci number, fifth task" <| fun _ ->
               let subject = domashka2.Task11 12
               Expect.equal subject 144 "result must be equal 144"
        ]
[<Tests>]
let task12 =
    testList "task12"
        [
           testCase "fibonnacis numbers 1" <| fun _ ->
               let subject = domashka2.Task12 1
               Expect.sequenceEqual subject [|0; 1|] "result must be equal [|0; 1|]"
           testCase "fibonnacis numbers 2" <| fun _ ->
               let subject = domashka2.Task12 2
               Expect.sequenceEqual subject [|0; 1; 1|] "result must be equal [|0; 1; 1|]"
           testCase "fibonnacis numbers 3" <| fun _ ->
               let subject = domashka2.Task12 3
               Expect.sequenceEqual subject [|0; 1; 1; 2|] "result must be equal [|0; 1; 1; 2|]"
           testCase "Twelveth Fibonacci number, six" <| fun _ ->
               let subject = domashka2.Task12 4
               Expect.sequenceEqual subject [|0; 1; 1; 2; 3|] "result must be equal [|0; 1; 1; 2; 3|]"
        ]
[<Tests>]
let testproperties =
    testList "tests some tasks"
        [
           testProperty "task9 task10"
               <| fun (n: int) -> Expect.equal (domashka2.Task9 (abs n)) (domashka2.Task10 (abs n)) "Compare task10 task9"
           testProperty "task8 task9"
               <| fun (n: int) -> Expect.equal (domashka2.Task8  (abs n)) (domashka2.Task9 (abs n)) "Compare task8 task9"
           testProperty "task10 task11"
           <| fun (n: int) -> Expect.equal (domashka2.Task10 (abs n)) (domashka2.Task11 (abs n)) "Compare task10 task11"
        ]    
