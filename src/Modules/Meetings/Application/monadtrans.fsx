#r "nuget: FSharpPlus"

open FSharpPlus
open FSharpPlus.Data

let o = Some "value"
let t: Result<string option, exn> = Ok o
let ot = OptionT t

let m = monad {
    let! v = ot
    printfn "%A" v
    return $"nowy {v}"
}