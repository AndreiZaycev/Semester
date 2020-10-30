namespace Semester
module Main =
    
    open Argu
    open System
    type CLIArguments =
        | Ftask 
        | Stask 
        | Thirdtask 
        | Fourthtask
        | Fifthtask
        | Sixthtask
        | Task7
        | Task8
        | Task9
        | Task10
        | Task11
        | Task12
        interface IArgParserTemplate with
            member s.Usage =
                match s with
                | Ftask _ -> "Calculates the value of the polynomial"
                | Stask _ -> "Calculates the value of the polynomial in less time"
                | Thirdtask _ -> "Calculates indexes of array elements no larger than the specified number"
                | Fourthtask _ -> "Calculates indexes of array elements that lie outside the range specified by numbers"
                | Fifthtask _ -> "Changing the array elements in some places"
                | Sixthtask _ -> "Changes element i to element j"
                | Task7 _ -> "The Fibonacci number by recursive method"
                | Task8 _ -> "The Fibonacci number by iterative method"
                | Task9 _ -> "Tail recursion"
                | Task10 _ -> "Matrix multiplication"
                | Task11 _ -> "Matrix multiplication for log "
                | Task12 _ -> "Fibonacci numbers up to the specified value"       
    [<EntryPoint>]
     let main (argv: string array) =
        try
         let parser = ArgumentParser.Create<CLIArguments>(programName = "Semester")
         let results = parser.Parse(argv)
         if results.Contains Ftask
         then
             printfn "Введите значение x :"
             let x = Console.ReadLine() |> float
             printfn ("Значение полинома равно %A") (domashka.Ftask x) 
         elif results.Contains Stask
         then
             printfn ("Введите значение x :")
             let x = Console.ReadLine() |> float
             printfn ("Значение полинома равно %A") (domashka.Stask x)
         elif results.Contains Thirdtask
         then
             printfn ("Введите размер массива:")
             let x = Console.ReadLine() |> int 
             let array = domashka.create_array x             
             printfn ("Введите число, меньше которого будут выведены индексы:")
             let bigelement = Console.ReadLine() |> int
             if bigelement < 0
             then printfn ("Введите число >0")
             else
                let secondarray = domashka.Thirdtask array bigelement
                printfn ("%A:") secondarray
         elif results.Contains Fourthtask
         then
             printfn ("Введите размерность массива ")
             let x = Console.ReadLine() |> int 
             let array = domashka.create_array x 
             printfn ("Введите левую границу :")
             let felement = Console.ReadLine() |> int
             printfn ("Введите правую границу :")
             let selement = Console.ReadLine() |> int
             let thirdarray = domashka.Fourthtask array felement selement 
             printfn ("Индексы элементов вне диапазона :")
             printfn ("%A") thirdarray
         elif results.Contains Fifthtask
         then            
             let dimensionArray = 2          
             let array = domashka.create_array dimensionArray
             let arrayizm = domashka.Fifthtask array
             printfn ("Измененные элементы массива")
             printfn ("%A") arrayizm
         elif results.Contains Sixthtask
         then
             printfn ("Введите размер массива ")
             let dimensionArray = Console.ReadLine() |> int
             if dimensionArray < 2
             then failwith ("Array needs at least 2 elements")
             else
                 let array: int array  = domashka.create_array dimensionArray
                 printfn ("Введите индексы элементов которые надо поменять")
                 let z = Console.ReadLine() |> int
                 let v = Console.ReadLine() |> int                            
                 let arrayizmeneniy = domashka.Sixthtask array z v 
                 printfn ("Измененный массив")
                 printfn ("%A") arrayizmeneniy
         elif results.Contains Task7 then
             printfn ("Введите число фибоначи, которое хотите вычислить")
             let n = Console.ReadLine() |> int
             let result = domashka2.Task7 n
             printfn ("Число фибоначи равно =")
             printfn ("%A") result            
         elif results.Contains Task8 then
            printfn ("Введите число фибоначи, которое хотите вычислить")
            let t = Console.ReadLine() |> int
            let result = domashka2.Task8 t
            printfn ("Число фибоначи равно =")
            printfn ("%A") result
         elif results.Contains Task9 then
            printfn ("Введите число фибоначи, которое хотите вычислить")
            let q = Console.ReadLine() |> int
            let result = domashka2.Task9 q
            printfn ("Число фибоначи равно =")
            printfn ("%A") result
         elif results.Contains Task10 then
            printfn ("Введите число фибоначи, которое хотите вычислить")
            let q = Console.ReadLine() |> int
            let result = domashka2.Task10 q           
            printfn ("Число фибоначи равно =")
            printfn ("%A") (result)
         elif results.Contains Task11 then
            printfn ("Введите число фибоначи, которое хотите вычислить")
            let y = Console.ReadLine() |> int
            let result = domashka2.Task11 y           
            printfn ("Число фибоначи равно =")
            printfn ("%A") (result)
         elif results.Contains Task12 then
            printfn ("Введите число, до которого выведутся числа фибоначи ")
            let t = Console.ReadLine() |> int
            let result = domashka2.Task12 t 
            printfn ("%A") result 
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
