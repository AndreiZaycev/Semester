module TestsSortingsAndPacking
open Expecto
let Testings fun1 fun2 (msg: string) =   
    testProperty (sprintf "Equality %A with standart Sort " msg) 
    <| fun n -> Expect.sequenceEqual (fun1 n) (fun2 n)
[<Tests>]
let SortArrayByBubbleAndQuickSort =
    testList "Sort array by bubble and quick sort"
        [
            Testings Homework4.QuickArraySort1 Array.sort "QuickArraySort1"
            Testings Homework4.QuickArraySort2 Array.sort "QuickArraySort2"
            Testings Homework4.SortBubble Array.sort "SortBubble"
        ]
[<Tests>]        
let SortingLists =
    testList "Sort list by bubble and quick sort"
        [
            Testings Homework4.QuickSortList List.sort "QuickSortList"
            Testings Homework4.SortBubbleList List.sort "SortBubbleList"
        ]
[<Tests>]
let ChecksFunctions =
    testList "Check ReadFile"
        [
            testProperty "writeArray and readArray test" <| fun (x: array<int>) ->
                Homework4.WriteArray "Arraytask.txt" x
                Expect.sequenceEqual x (Homework4.ReadfileArray "Arraytask.txt") "reversivity of read and write"
            testProperty "writeList and readList test1" <| fun (x: list<int>) ->
                Homework4.WriteList "list2task.txt" x
                Expect.sequenceEqual x (Homework4.ReadfileList "list2task.txt") "reversivity of read and write"
        ]
[<Tests>]
let ChecksPackingUnpacking =
    testList "Checking"
        [
        testProperty "32 to 64"
        <| fun (n,i) -> Expect.equal (n,i) (Homework4.Unpacking64to32 (Homework4.Packing32to64 (n,i)))
        testProperty "16 to 64"
        <| fun (a,b,c,d) -> Expect.equal (a,b,c,d) (Homework4.Unpacking64to16 (Homework4.Packing16to64 (a,b,c,d)))
        ]
                    
            

