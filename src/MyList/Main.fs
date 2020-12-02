module Main 
open Listik  
open Argu
open System

type CLIArguments =
    | FuncToMyList
    | FuncMyStringMyTree
    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | FuncToMyList _ -> "functions to my list"
            | FuncMyStringMyTree _ -> "functions to my string and tree"           
[<EntryPoint>]
    let main (argv: string array) =
        try
            let parser = ArgumentParser.Create<CLIArguments>(programName = "MyList")
            let results = parser.Parse(argv)
            if results.Contains FuncToMyList
            then
                printfn "Итак, есть несколько функций, какую протестировать? \n 1) Length \n 2) Concat \n 3) Sort \n 4) Iter \n 5) Map \n 6) ConvertToList " 
                let x = Console.ReadLine() |> int
                let myList = generatorMyList 5
                if x = 1
                then
                    printfn "Вот такой листик сгенерировался %A" myList
                    printfn "Длина равна: %A" (length myList)
                elif x = 2
                then
                    let myList1 = generatorMyList 5 
                    printfn "Вот такой первый листик сгенерировался %A" myList1
                    printfn "Вот такой второй листик сгенерировался %A" myList
                    printfn "Сконкатенированный лист: \n %A" (concatMyList myList myList1)
                elif x = 3
                then
                    printfn "Вот такой листик сгенерировался %A" myList
                    printfn "Отсортируем лист \n %A" (sortForMyList myList)
                elif x = 4
                then
                    printfn "Вот такой листик сгенерировался %A" myList
                    printfn "выпишем все элементы"
                    (iterMyList (printfn "%A") myList)
                elif x = 5
                then
                    printfn "Вот такой листик сгенерировался %A" myList
                    printfn "Прибавим 2 к каждому элементу и выведем новый лист \n %A" (mapMyList ((+) 2) myList)
                elif x = 6
                then
                    printfn "Вот такой листик сгенерировался %A" myList
                    printfn "Сконвертим в лист \n %A" (fromMyListtoStandart myList)
            elif results.Contains FuncMyStringMyTree
            then          
                printfn "Итак, есть несколько функций, какую попробовать? \n 1) Concat \n 2) FromMyStringToString,FromStringToMyString \n 3) AverageInMyTree \n 4) MaxIntMyTree "
                let x = Console.ReadLine() |> int
                if x = 1
                then
                    printfn "Введите две строчки: "
                    let y = Cons ('п', Cons ('р', Cons ('и', Cons ('в', Cons ('е', One 'т')))))
                    let z = Cons ('т', Cons ('е', Cons ('в', Cons ('и', Cons ('р', One 'п')))))
                    printfn "%A \n %A " y z 
                    printfn "Сконкатенированная строка : %A " (concatMyString y z)
                elif x = 2
                then
                    let y = Cons ('п', Cons ('р', Cons ('и', Cons ('в', Cons ('е', One 'т')))))
                    printfn "Допустим есть такой MyList : \n %A" y
                    printfn "Переведем в строку %A" (fromMyStringToString y)
                    printfn "И обратно %A" (fromStringToMyString (fromMyStringToString y))
                elif x = 3
                then
                     let mTree = (Node ((SomeMeasures.Int 100000), Cons (Node (SomeMeasures.Int 15, One (Leaf (SomeMeasures.Int 100))),One (Node (SomeMeasures.Int 5000, One (Leaf (SomeMeasures.Int 50000)))))))
                     let sMTree = (Node ((SomeMeasures.Int 15), Cons ((Leaf (SomeMeasures.Int 13)), One (Leaf (SomeMeasures.Int 10)))))
                     printfn "Пусть у нас есть два таких дерева \n %A \n %A" mTree sMTree
                     printfn "Среднее значение у первого равно: %A" (avgMyTree mTree)
                     printfn "Среднее значение у первого равно: %A" (avgMyTree sMTree)
                elif x = 4
                then
                    let mTree = (Node ((SomeMeasures.Int 100000), Cons (Node (SomeMeasures.Int 15, One (Leaf (SomeMeasures.Int 100))),One (Node (SomeMeasures.Int 5000, One (Leaf (SomeMeasures.Int 50000)))))))
                    let sMTree = (Node ((SomeMeasures.Int 15), Cons ((Leaf (SomeMeasures.Int 13)), One (Leaf (SomeMeasures.Int 10)))))
                    printfn "Пусть у нас есть два таких дерева \n %A \n %A" mTree sMTree
                    printfn "Максимальное значение у первого равно: %A" (maxInMyTree mTree)
                    printfn "Максимальное значение у первого равно: %A" (maxInMyTree sMTree)              
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
