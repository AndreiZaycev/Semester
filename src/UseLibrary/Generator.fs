module Generator
open System
open System.Threading.Tasks

type Type =
    | Int
    | Float
    | Bool

type Options =
    val _rows: int 
    val _cols: int
    val _amt: int
    val _sparsity: float
    val _path: string
    val _type: Type
    new (rows, cols, amt, sparsity, path, typeOf) =
        {
            _rows = rows;
            _cols = cols;
            _amt = amt;
            _sparsity = sparsity;
            _path = path;
            _type = typeOf
        }

let printMatrix (x: string [,]) path =
    let mutable text = ""
    for i = 0 to x.GetLength(0) - 1 do
        for j = 0 to x.GetLength(1) - 1 do
            text <- text + x.[i, j] + " "
        text <- text + "\n"
    IO.File.WriteAllText (path, text)

let generateSparseMatrix (x: Options) =
    let rand = Random()
    let cashOfMatrix = Array.zeroCreate x._amt
    for i = 0 to x._amt - 1 do
        let output = Array2D.zeroCreate x._rows x._cols
        for j = 0 to x._rows - 1 do
            for k = 0 to x._cols - 1 do             
                let y = rand.NextDouble()
                if y > x._sparsity
                then
                    output.[j, k] <- (match x._type with
                                      | Int -> string (rand.Next())
                                      | Float -> string (rand.NextDouble() * float Int32.MaxValue)
                                      | Bool -> "1")
                else output.[j, k] <- "0"
        cashOfMatrix.[i] <- output
    cashOfMatrix

