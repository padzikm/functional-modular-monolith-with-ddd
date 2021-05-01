open System.IO

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

type FileInstruction<'A> =
    | ReadFile of string * (string -> 'A)
    | WriteFile of string * string * 'A
    
type FileInstruction<'A> with
    static member Map (x: FileInstruction<'A>, f: 'A -> 'B) =
        match x with
        | ReadFile (s, g) -> ReadFile (s, g >> f)
        | WriteFile (s, c, a) -> WriteFile (s, c, f a)

type Program<'A> = Free<Coproduct<CommandLineInstruction<'A>, FileInstruction<'A>>, 'A>

let readline = (Free.liftF << InL) (ReadLine id) : Program<_>
let writeline s = Free.liftF (InL(WriteLine (s,()))) : Program<_>
let readfile f = Free.liftF (InR(ReadFile (f, id))) : Program<_>
let writefile f c = Free.liftF (InR(WriteFile (f,c,()))) : Program<_>

let program = monad {
    do! writeline "napisz cos"
    let! s = readline
    do! writeline $"napisales {s}"
    do! writeline "podaj nazwe pliku"
    let! f = readline
    let c = $"zapis do pliku\nnapisano {s}\n"
    do! writeline "zapisuje do pliku"
    do! writefile f c
    do! writeline $"zapisalem do pliku {f}:"
    let! fc = readfile f
    do! writeline fc
}

let rec interpret (p: Program<'A>) =
    match Free.run p with
    | Pure x -> x
    | Roll v -> match v with
        | InL cmd -> match cmd with
            | ReadLine f ->
                    let l = System.Console.ReadLine()
                    interpret (f(l))
            | WriteLine (s,a) ->
                    System.Console.WriteLine(s)
                    interpret a
        | InR file -> match file with
            | ReadFile (f, n) ->
                let s = File.ReadAllText f
                let fs = $"fake file content from file {f}"
                interpret (n s)
            | WriteFile (f,c,a) ->
                File.WriteAllText(f, c)
                interpret a
                    
interpret program