#r "nuget: FSharpPlus"

open FSharpPlus
open FSharpPlus.Data

type CommandLineInstruction<'A> =
    | ReadLine of (string -> 'A)
    | WriteLine of string * 'A
    
type CommandLineInstruction<'A> with
    static member Map (x: CommandLineInstruction<'A>, f: 'A -> 'B) =
        match x with
        | ReadLine g -> ReadLine (g >> f)
        | WriteLine (s, a) -> WriteLine (s, f a)
        
        
type FreeCmd<'A> = Free<CommandLineInstruction<'A>, 'A>

let readline = Free.liftF (ReadLine id)
let writeline s = Free.liftF (WriteLine (s,()))

let program = monad {
    do! writeline "napisz cos"
    let! s = readline
    do! writeline $"napisales {s}"
}

let rec interpret (p: FreeCmd<'A>) =
    match Free.run p with
    | Pure x -> x
    | Roll v -> match v with
        | ReadLine f ->
                    let l = System.Console.ReadLine()
                    interpret (f(l))
        | WriteLine (s,a) ->
                    System.Console.WriteLine(s)
                    interpret a
                    
interpret program