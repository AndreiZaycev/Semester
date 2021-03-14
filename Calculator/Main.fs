module Main
open Argu
open System
open System.Globalization
open System.Collections.Generic

type CLIArguments =
    | Input of file:string
    | Calculate of expr:string
    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Input _ -> "File to interpretate"
            | Calculate _ -> "Arithmetic expression to calculate and return the result"

open FSharp.Text.Lexing

let parse (text: string) =
    let lexbuf = LexBuffer<char>.FromString text
    let parsed = CalcParser.start Lexer.tokenStream lexbuf
    parsed

[<EntryPoint>]
let main (argv: string array) =
    let parser = ArgumentParser.Create<CLIArguments>(programName = "Arithmetics interpreter")
    let results = parser.Parse(argv)
    match parser.ParseCommandLine argv with
    | p when p.Contains(Input) ->
        let file = results.GetResult Input
        let ast = parse (System.IO.File.ReadAllText file)
        Interpretator.run ast "C:\Users\Zver\Desktop\dot\\this.dot"
    | p when p.Contains(Calculate) ->
        let expr = results.GetResult Calculate
        let full_expr = parse ("let [x] = " + expr + " print [x]")
        Interpretator.run full_expr "C:\Users\Zver\Desktop\dot\\this.dot"
    | _ -> parser.PrintUsage() |> printfn "%s"
    0   
    
