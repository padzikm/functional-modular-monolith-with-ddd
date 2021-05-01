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

type DbRecord = {
    name: string
    age: int
}

type DatabaseInstruction<'A> =
    | ReadEntity of string * (DbRecord -> 'A)
    | WriteEntity of string * DbRecord * 'A
    
type DatabaseInstruction<'A> with
    static member Map (x: DatabaseInstruction<'A>, f: 'A -> 'B) =
        match x with
        | ReadEntity (s, g) -> ReadEntity (s, g >> f)
        | WriteEntity (s, e, a) -> WriteEntity (s, e, f a)


type Program<'A> = Free<Coproduct<DatabaseInstruction<'A>, Coproduct<CommandLineInstruction<'A>, FileInstruction<'A>>>, 'A>

let readline = (Free.liftF << InR << InL) (ReadLine id) : Program<_>
let writeline s = Free.liftF (InR(InL(WriteLine (s,())))) : Program<_>
let readfile f = Free.liftF (InR(InR(ReadFile (f, id)))) : Program<_>
let writefile f c = Free.liftF (InR(InR(WriteFile (f,c,())))) : Program<_>
let readentity i = Free.liftF (InL(ReadEntity (i, id))) : Program<_>
let writentity i e = Free.liftF (InL(WriteEntity (i,e,()))) : Program<_>

let program = monad {
    do! writeline "podaj id encji"
    let! s = readline
    let! e = readentity s
    do! writeline (sprintf "wczytano %A" e)
    do! writeline "podaj nowe imie"
    let! n = readline
    let ne = {e with name = n}
    do! writentity s ne
    do! writeline "zapisano"
    do! writeline "podaj nazwe pliku"
    let! f = readline
    let! ee = readentity s
    let c = sprintf "%A" ee
    do! writeline $"zapisuje {c} do pliku"
    do! writefile f c
    do! writeline $"zapisano do pliku {f}:"
    let! fc = readfile f
    do! writeline fc
}

let mutable db = Map.empty<string, DbRecord>

let rec interpret (p: Program<'A>) =
    match Free.run p with
    | Pure x -> x
    | Roll v -> match v with
        | InL dbi -> match dbi with
            | ReadEntity (i,f) ->
                let e = {name = "fake"; age = 5}
                let o = db.TryFind i
                match o with
                | Some v -> interpret (f v) 
                | None -> interpret (f e)
            | WriteEntity (i,e,a) ->
                db <- db.Add(i, e)
                interpret a
        | InR o -> match o with
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
                    interpret (n s)
                | WriteFile (f,c,a) ->
                    File.WriteAllText(f, c)
                    interpret a
                    
interpret program