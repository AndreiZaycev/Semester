namespace Sorting
module Main =   
    open Argu
    open System
    type CLIArguments =
        | SortBubble
        | SortBubbleList
        | QuickSortList
        | QuickArraySort1
        | QuickArraySort2
        | Unpacking64to32
        | Unpacking64to16    
        interface IArgParserTemplate with
            member s.Usage =
                match s with
                | SortBubble _ -> "Sorting by bubble"
                | SortBubbleList _ -> "sortlist"
                | QuickSortList _ -> "Sorts quicker"
                | QuickArraySort2 _ -> "Sort Array quicker"
                | QuickArraySort1 _ -> "Sort Array quicker 2"
                | Unpacking64to32 _ -> "do what writes in func"
                | Unpacking64to16 _ -> "do what writes in func v.2.0"
    [<EntryPoint>]
    let main (argv: string array) =
        let parser = ArgumentParser.Create<CLIArguments>(programName = "Sorting")
        try 
        let results = parser.Parse argv
        if results.Contains SortBubble
        then
            printfn ("Укажите путь к файлу")
            let x = Homework4.ReadfileArray (Console.ReadLine()) 
            let t = Homework4.SortBubble x 
            printfn ("Отсортированный массив  %A") t
            printfn ("Введите путь, по которому файл будет переписан")
            let o = Console.ReadLine() |> string
            let i = t |> Homework4.ArrayToString
            Homework4.Write o i
        elif results.Contains SortBubbleList
        then
            printfn ("Укажите путь к файлу")
            let x = Homework4.ReadfileArray (Console.ReadLine()) |> List.ofArray
            let t = Homework4.SortBubbleList x
            printfn ("Отсортированный лист  %A") t
            printfn ("Введите путь, по которому файл будет переписан")
            let o = Console.ReadLine() |> string
            let i = t |> Array.ofList |> Homework4.ArrayToString
            Homework4.Write o i
        elif results.Contains QuickSortList
        then
            printfn ("Укажите путь к файлу")
            let x = Homework4.ReadfileArray (Console.ReadLine()) |> List.ofArray
            let t = Homework4.QuickSortList x
            printfn ("Отсортированный массив  %A") t
            printfn ("Введите путь, по которому файл будет переписан")
            let i = t |> Array.ofList |> Homework4.ArrayToString
            let o = Console.ReadLine() |> string
            Homework4.Write o i
        elif results.Contains QuickArraySort2
        then
            printfn ("Укажите путь к файлу")
            let x = Homework4.ReadfileArray (Console.ReadLine()) 
            let t = Homework4.QuickArraySort2 x
            printfn ("Отсортированный массив %A") t
            printfn ("Введите путь, по которому файл будет переписан")           
            let o = Console.ReadLine() |> string
            let i = t |> Homework4.ArrayToString
            Homework4.Write o i
        elif results.Contains QuickArraySort1
        then
            printfn ("Укажите путь к файлу")
            let x = Homework4.ReadfileArray (Console.ReadLine()) 
            let t = Homework4.QuickArraySort1 x
            printfn ("Отсортированный массив %A") t
            printfn ("Введите путь, по которому файл будет переписан")
            let o = Console.ReadLine() |> string
            let i = t |> Homework4.ArrayToString
            Homework4.Write o i
        elif results.Contains Unpacking64to32
        then
            printfn ("Введите два числа")
            let x = Console.ReadLine() |> int32
            let y = Console.ReadLine() |> int32
            printfn ("%A") (Homework4.Packing32to64 (x, y))
            printfn ("Распакованные числа %A") (Homework4.Unpacking64to32 ((Homework4.Packing32to64 (x, y))))
        elif results.Contains Unpacking64to16
        then 
            printfn ("введите 4 числа")
            let x = Console.ReadLine() |> int16
            let y = Console.ReadLine() |> int16
            let z = Console.ReadLine() |> int16
            let v = Console.ReadLine() |> int16
            let k = Homework4.Packing16to64 (x, y, z, v)
            printfn ("%A") (Homework4.Packing16to64 (x, y, z, v))
            printfn ("Распакованные числа %A") (Homework4.Unpacking64to16 k)
        else
            parser.PrintUsage() |> printfn "%s"
        0
        with
        | :? ArguParseException as ex ->
            printfn "%s" ex.Message
            1
        | ex ->
            printfn "Internal Error:"
            printfn "%s" ex.Message
            2
