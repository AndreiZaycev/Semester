module Homework4 
    open System.IO
    let ReadfileArray file =
        try
            let a = System.IO.File.ReadAllLines file
            let Array = Array.zeroCreate (a.Length)
            let mutable k = 0 
            for i in a do            
                Array.[k] <- int (i.Trim()) 
                k <- k + 1
            Array
        with
            | :? System.IO.FileNotFoundException ->
                failwith "Given file has not been found"
            | :? System.IO.IOException ->
                failwith "Invalid file name"
    let ReadfileList file =
        try
            let a = System.IO.File.ReadAllLines file
            if a.Length = 0
            then []
            else
                let mutable l = []
                for i in 0 .. a.Length - 1 do
                    l <- l @ [int (a.[i].Trim())]
                l            
        with
            | :? System.IO.FileNotFoundException ->
                failwith "Given file has not been found"
            | :? System.IO.IOException ->
                failwith "Invalid file name"
    let SortBubble (x: array<int>) =       
        for i = 0 to x.Length - 1 do
            for j = i + 1 to x.Length - 1 do
                if x.[j] < x.[i]
                then
                    x.[j] <- x.[j] + x.[i]
                    x.[i] <- x.[j] - x.[i]
                    x.[j] <- x.[j] - x.[i]
        x
    let Write file where =
        let a = System.IO.File.WriteAllText (file, where)
        a
    let WriteArray file (x: array<int>) =
        let mutable s = ""
        for i in 0 .. x.Length - 1 do
            s <- s + string x.[i] + "\n"
        Write file s
    let WriteList file (x: list<int>) =
        let mutable s = "" 
        for i in 0 .. x.Length - 1 do
            s <- s + string x.[i] + "\n"
        Write file s
    let SortBubbleList (x: list<int>) =
        let mutable k = x
        let rec _go x =
            match x with
            | [] -> []
            | x :: y :: tl when x > y -> y :: (_go (x :: tl))
            | y :: x :: tl -> y :: (_go (x :: tl))
            | x :: tl -> [x] 
        for i in x do
            k <- _go k
        k          
    let QuickSortList (file: list<int>) =
        let rec QuickList = function
        | [] -> []
        | pivot :: tl ->
           let left,right = List.partition (fun x -> x < pivot) tl
           QuickList left @ [pivot] @ QuickList right
        QuickList file 
    let QuickArraySort1 x =
        let rec Divide (t: int array) =
            if t.Length <= 1
            then t
            else
                let pivot = t.Length / 2 - t.Length % 2
                let mutable k = 0
                let mutable k1 = 0
                for i in 0 .. t.Length - 1 do
                    if t.[i] < t.[pivot]  
                    then k <- k + 1                  
                let array1 = Array.zeroCreate k
                let array2 = Array.zeroCreate (t.Length - 1 - k)               
                k <- 0
                for i in 0 .. t.Length - 1 do
                    if i <> pivot
                    then
                        if t.[i] < t.[pivot]
                        then
                            array1.[k] <- t.[i]
                            k <- k + 1
                        else 
                            array2.[k1] <- t.[i]
                            k1 <- k1 + 1               
                Array.append (Array.append (Divide array1) [|t.[pivot]|] ) (Divide array2)
        Divide x       // В общем я не особо понимаю какой из них является квик сортом поэтому оставлю оба))      
    let QuickArraySort2 (x: array<int>) =
        let rec _go = function
        | [||] -> [||]
        | x when x.Length < 2 -> x
        | x -> 
            let left, (right, pivot) = Array.partition (fun i -> i < x.[0]) x |> (fun (right, pivot) -> right, pivot |> Array.partition (fun n -> n <> x.[0]))
            Array.append (Array.append (left |> _go) pivot) (right |> _go)
        _go x     
    let Packing32to64 (x: int32, y: int32) =
        if y >= 0
        then (int64 x <<< 32) + int64 y 
        else ((int64 x <<< 32) + 4294967296L) + int64 y 
    let Unpacking64to32 (x: int64) =
        let o = x >>> 32 |> int32
        let t = (x <<< 32) >>> 32 |> int32             
        (o,t)
    let Packing16to64 (x: int16, y: int16, z: int16, v: int16) =
        let mutable a32 = 0
        let mutable b32 = 0
        if y >= 0s
        then a32 <- (int32 x <<< 16) + int32 y 
        else a32 <- ((int32 x <<< 16) + 65536) + int32 y
        if v >= 0s
        then b32 <- (int32 z <<< 16) + int32 v 
        else b32 <- ((int32 z <<< 16) + 65536) + int32 v
        Packing32to64 (a32, b32)
    let Unpacking64to16 (x: int64) =
        let a = x |> int16
        let b = x >>> 16 |> int16
        let c = x >>> 32 |> int16
        let d = x >>> 48 |> int16
        (d,c,b,a)

     

