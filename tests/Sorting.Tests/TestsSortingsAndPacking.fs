module TestsSortingsAndPacking
open Expecto

[<Tests>]
let SortArrayByBubbleAndQuickSort =
    testList "Sort array by bubble and quick sort"
        [
            testProperty "Equality with standart Sort 1 "
            <| fun (n: array<int>) -> Expect.sequenceEqual (Homework4.QuickArraySort1 n) (Array.sort n)
            testProperty "Equality with standart Sort 2 "
            <| fun (n: array<int>) -> Expect.sequenceEqual (Homework4.SortBubble n) (Array.sort n)
            testProperty "Equality with standart Sort 3 "
            <| fun (n: array<int>) -> Expect.sequenceEqual (Homework4.QuickArraySort2 n) (Array.sort n)
        ]
[<Tests>]        
let SortingLists =
    testList "Sort list by bubble and quick sort"
        [
            testProperty "Equality with standar Sort list 1 "
            <| fun (n: list<int>) -> Expect.sequenceEqual (Homework4.QuickSortList n) (List.sort n)
            testProperty "Equality with standar Sort list 2"
            <| fun (n: list<int>) -> Expect.sequenceEqual (Homework4.SortBubbleList n) (List.sort n)
        ]
[<Tests>]
let ChecksFunctions =
    testList "Check ReadFile"
        [
            testProperty "writeList and readList test" <| fun (x: array<int>) ->
                Homework4.WriteArray "Arraytask.txt" x
                Expect.sequenceEqual x (Homework4.ReadfileArray "Arraytask.txt") "reversivity of read and write"
        ]
[<Tests>]
let ChecksPackingUnpacking =
    testList "Checking"
        [
            testCase "Dont know what to write 1" <| fun _ -> 
                let subject = Homework4.Packing16to64 (13s, -12s, 15s, -203s)
                let x = Homework4.Unpacking64to16 subject
                Expect.equal x (13s, -12s, 15s, -203s) "Pack and unpack is reversive "
            testCase "Dont know what to write 2" <| fun _ -> 
                let subject = Homework4.Packing32to64 (-123, -145)
                let x = Homework4.Unpacking64to32 subject
                Expect.equal x (-123, -145) "Pack and unpack is reversive "
        ]
                
            

