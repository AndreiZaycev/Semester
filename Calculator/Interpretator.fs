module Interpretator

open Listik

open BigAriphmetics

open System.Collections.Generic

let toDotList = new Stack<_>() 

let numbersOfOp = new Stack<_>() // нужно нумеровать операции чтобы они не склеивались в доте  

let mutable private flag = true

let mutable private counter = 0

let helpFunc vDict x y stringNameOp preOperationString operation1 mainRec = // вынес код и переиспользую
    counter <- counter + 1
    let numOfOp = "op" + string counter
    numbersOfOp.Push(numOfOp + sprintf "[shape = ellipse, label = %A];" stringNameOp)
    if flag
    then
        flag <- false
        let y1 = mainRec vDict y numOfOp
        let x1 = mainRec vDict x numOfOp
        operation1 x1 y1
    else
        let y1 = mainRec vDict y numOfOp
        let x1 = mainRec vDict x numOfOp
        toDotList.Push(preOperationString + " -> " + numOfOp) 
        operation1 x1 y1

let helpFunc1 vDict x stringNameOp preOperationString operation1 mainRec =
    counter <- counter + 1
    let numOfOp = "op" + string counter
    numbersOfOp.Push(numOfOp + sprintf "[shape = ellipse, label = %A];" stringNameOp)
    if flag
    then
        flag <- false
        let x1 = mainRec vDict x numOfOp
        operation1 x1
    else
        toDotList.Push(preOperationString + " -> " + numOfOp) 
        let x1 = mainRec vDict x numOfOp
        operation1 x1

let processExpr (vDict: Dictionary<_,_>) expr =
    let rec _go (vDict: Dictionary<_,_>) expr toDotOp = 
        match expr with
        | Exp.Num t ->
            let text = toInt t
            toDotList.Push(toDotOp + " -> " + string text)
            t
        | Exp.NVar t ->
            let data =
                try
                    vDict.[t]
                with
                | _ -> failwithf "Variable %A is not declared" t
            let text = toInt data
            toDotList.Push(toDotOp + " -> " + string text)
            data
        | Exp.Sum (x, y) -> helpFunc vDict x y " + " toDotOp sum _go     
        | Exp.Sub (x, y) -> helpFunc vDict x y " - " toDotOp sub _go   
        | Exp.Mul (x, y) -> helpFunc vDict x y " * " toDotOp multiply _go   
        | Exp.Div (x, y) -> helpFunc vDict x y " / " toDotOp division _go
        | Exp.Abs x -> helpFunc1 vDict x " absolute " toDotOp absolute _go
        | Exp.Bin x -> helpFunc1 vDict x " binary " toDotOp toBinary _go
        | Exp.Pow (x, y) -> helpFunc vDict x y " ^ " toDotOp power _go
        | Exp.DivRem (x, y) -> helpFunc vDict x y " % " toDotOp remDiv _go        
    _go vDict expr "" 

let processStmt (vDict: Dictionary<_,_>) stmt =
    match stmt with
    | Exp.Print t ->
        let (data: BigInt) =
            try
                vDict.[t]
            with
            | _ -> failwithf "Variable %A is not declared" t      
        printfn "%A" (toInt data)
    | Exp.VDecl (x, y) ->
        if vDict.ContainsKey x
        then vDict.[x] <- processExpr vDict y
        else vDict.Add (x, processExpr vDict y)
    vDict

let run ast path =
    let vDict = new Dictionary<_,_>()
    List.fold processStmt vDict ast |> ignore
    numbersOfOp.Push ("rankdir = TB") |> ignore
    numbersOfOp.Push ("{") |> ignore
    numbersOfOp.Push ("digraph nfa") |> ignore
    let secArr = (numbersOfOp.ToArray())
    let firstArr = (toDotList.ToArray())
    let newArr = Array.append secArr firstArr 
    newArr.[newArr.Length - 1] <- newArr.[newArr.Length - 1] + "}"
    System.IO.File.WriteAllLines (path, List.ofArray newArr)
