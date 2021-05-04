open System.IO

#r "nuget: FSharpPlus"

open FSharpPlus
open FSharpPlus.Data

type CommandLineInstruction<'A> =
    | ReadLine of (string -> 'A)
    | WriteLine of string * 'A

type CommandLineInstruction<'A> with
    static member Map(x: CommandLineInstruction<'A>, f: 'A -> 'B) =
        match x with
        | ReadLine g -> ReadLine(g >> f)
        | WriteLine (s, a) -> WriteLine(s, f a)

type FileInstruction<'A> =
    | ReadFile of string * (string -> 'A)
    | WriteFile of string * string * 'A

type FileInstruction<'A> with
    static member Map(x: FileInstruction<'A>, f: 'A -> 'B) =
        match x with
        | ReadFile (s, g) -> ReadFile(s, g >> f)
        | WriteFile (s, c, a) -> WriteFile(s, c, f a)

type DbRecord = { name: string; age: int }

type DatabaseInstruction<'A> =
    | ReadEntity of string * (DbRecord -> 'A)
    | WriteEntity of string * DbRecord * 'A

type DatabaseInstruction<'A> with
    static member Map(x: DatabaseInstruction<'A>, f: 'A -> 'B) =
        match x with
        | ReadEntity (s, g) -> ReadEntity(s, g >> f)
        | WriteEntity (s, e, a) -> WriteEntity(s, e, f a)


type Program<'A> =
    Free<Coproduct<DatabaseInstruction<'A>, Coproduct<CommandLineInstruction<'A>, FileInstruction<'A>>>, 'A>

let readline : Program<_> = (Free.liftF << InR << InL) (ReadLine id)
let writeline s : Program<_> = Free.liftF(InR(InL(WriteLine(s, ()))))
let readfile f : Program<_> = Free.liftF(InR(InR(ReadFile(f, id))))
let writefile f c : Program<_> = Free.liftF(InR(InR(WriteFile(f, c, ()))))
let readentity i : Program<_> = Free.liftF(InL(ReadEntity(i, id)))
let writentity i e : Program<_> = Free.liftF(InL(WriteEntity(i, e, ())))

let program =
    monad {
        do! writeline "podaj id encji"
        let! s = readline
        let! e = readentity s
        do! writeline(sprintf "wczytano %A" e)
        do! writeline "podaj nowe imie"
        let! n = readline
        let ne = { e with name = n }
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
        return fc
    }

type TestStateDb = {
    ReadLines: string list
    WriteLines: string list
    Db: Map<string,DbRecord>
}

let interpretConsole (p: CommandLineInstruction<_>) =
    match p with
    | ReadLine f ->
//        printfn "readline"
        async {
            let s = System.Console.ReadLine()
            return f s
        }
    | WriteLine (s, a) ->
//        printfn $"writeline {s}"
        async {
            do System.Console.WriteLine(s)
            return a
        }

let interpretConsoleTest (p: CommandLineInstruction<_>) : State<_,_> =
    match p with
    | ReadLine f ->
//        printfn "readline"
        monad {
            //let s = System.Console.ReadLine()
            let! s = State.get
            match s.ReadLines with
            | [] ->
                let ns = {s with ReadLines = []}
                do! State.put ns
                return f ""
            | h::t ->
                let ns = {s with ReadLines = t}
                do! State.put ns
                return f h
        }
    | WriteLine (s, a) ->
//        printfn $"writeline {s}"
        monad {
            //do System.Console.WriteLine(s)
            let! st = State.get
            let nl = s::st.WriteLines
            let ns = {st with WriteLines = nl}
            do! State.put ns
            return a
        }

let interpretFile (p: FileInstruction<_>) =
    match p with
    | ReadFile (f, n) ->
//        printfn $"readfile {f}"
        async {
            let! s = File.ReadAllTextAsync f |> Async.AwaitTask
            return n s
        }
    | WriteFile (f, c, a) ->
//        printfn $"writefile {f} {c}"
        async {
            do! File.WriteAllTextAsync(f, c) |> Async.AwaitTask
            return a
        }
        
let interpretFileTest (p: FileInstruction<_>): State<_,_> =
    match p with
    | ReadFile (f, n) ->
//        printfn $"readfile {f}"
        monad {
            //let! s = File.ReadAllTextAsync f |> Async.AwaitTask
            let s = "test file contet"
            return n s
        }
    | WriteFile (f, c, a) ->
//        printfn $"writefile {f} {c}"
        monad {
            //do! File.WriteAllTextAsync(f, c) |> Async.AwaitTask
            return a
        }

let mutable db = Map.empty<string, DbRecord>

let interpretDb (p: DatabaseInstruction<_>) =
    match p with
    | ReadEntity (i, f) ->
//        printfn $"readentity {i}"
        let e = { name = "fake"; age = 5 }

        async {
            let o = db.TryFind i

            return
                match o with
                | Some v -> f v
                | None -> f e
        }
    | WriteEntity (i, e, a) ->
//        printfn $"writeentity {i} {e}"
        async {
            db <- db.Add(i, e)
            return a
        }
        
let interpretDbTest (p: DatabaseInstruction<_>): State<_,_> =
    match p with
    | ReadEntity (i, f) ->
//        printfn $"readentity {i}"
        let e = { name = "fake"; age = 5 }

        monad {
            let! s = State.get
            let o = s.Db.TryFind i

            return
                match o with
                | Some v -> f v
                | None -> f e
        }
    | WriteEntity (i, e, a) ->
//        printfn $"writeentity {i} {e}"
        monad {
            let! s = State.get
            let ndb = s.Db.Add(i, e)
            //db <- db.Add(i, e)
            let ns = {s with Db = ndb}
            do! State.put ns
            return a
        }

let rec interpret (p: Program<_>) =
    let go v =
        match v with
        | InL dbi ->
            interpretDb dbi
        | InR o ->
            match o with
            | InL cmd ->
                interpretConsole cmd            
            | InR file ->
                interpretFile file
    Free.fold go program
    
let rec interpretTest (p: Program<_>) =
    let go v =
        match v with
        | InL dbi ->
            interpretDbTest dbi
        | InR o ->
            match o with
            | InL cmd ->
                interpretConsoleTest cmd            
            | InR file ->
                interpretFileTest file
    Free.fold go program

let t = interpretTest program

let init = {
    ReadLines = []
    WriteLines = []
    Db = Map.empty<string, DbRecord>
}

let (r,s) = State.run t init

printfn "state: %A" s
printfn "value: %A" r 