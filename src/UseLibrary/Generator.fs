
module Generator
open System
open System.Threading.Tasks
open System.IO

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
    val _streams: int
    new (rows, cols, amt, sparsity, path, typeOf, streams) =
        {
            _rows = rows;
            _cols = cols;
            _amt = amt;
            _sparsity = sparsity;
            _path = path;
            _type = typeOf
            _streams = streams
        }

let printMatrix (x: seq<seq<string>>) path = File.WriteAllLines(path, Seq.map (fun seq -> String.concat " " seq) x)

let generateSparseMatrix (x: Options) =
    let rand = Random()
    let chunkSize = x._amt / x._streams
    let streams = x._streams
    [ for t in 0 .. streams - 1 ->
        async { do
                let lastNum = 
                    if t < streams - 1
                    then
                        chunkSize * (t + 1) - 1
                    else
                        x._amt - 1      
                for i in t * chunkSize .. lastNum do
                    let output = 
                        seq {
                            for j = 0 to x._rows - 1 do
                                seq {
                                    for k = 0 to x._cols - 1 do             
                                        let y = rand.NextDouble()
                                        if y > x._sparsity
                                        then 
                                            match x._type with
                                            | Int -> string (rand.Next())
                                            | Float -> string (rand.NextDouble() * float Int32.MaxValue)
                                            | Bool -> "1"
                                        else "0"                               
                                }
                        }
                    printMatrix output (Path.Combine (x._path, "Matrix" + string i + ".txt"))
        }
    ]
    |> Async.Parallel
    |> Async.RunSynchronously
    |> ignore
