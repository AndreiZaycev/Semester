module LongArithmetic

open Expecto

open BigAriphmetics

open Listik

let template valuesTest operator (operator1: BigInt -> BigInt -> BigInt) name =
    testProperty name
    <| fun (k: int, t: int) ->
           if k <> 0 && abs k < valuesTest && t <> 0 && abs t < valuesTest // чтоб инты эксшепшн не выдавали
           then
               let first = createBigInt (abs k)
               let second = createBigInt (abs t)
               let firstInt = fold (fun acc elem -> string elem + acc) "" (rev first.digits) |> int
               let secondInt = fold (fun acc elem -> string elem + acc) "" (rev second.digits) |> int
               if secondInt <> 0 then
                   Expect.equal
                       (operator (firstInt * first.sign) (secondInt * second.sign))
                       ((operator1 first second).sign * (fold (fun acc elem -> string elem + acc) "" (rev (operator1 first second).digits) |> int))
                       "equals"

[<Tests>]
let testingsOperations =
    testList "sum div multi subtr"
        [
            template 8 (+) sum "sum"

            template 8 (-) sub "sub"

            template 5 (*) multiply  "multiply"

            template 8 (/) division "division"
        ]

