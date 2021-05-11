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
    | ReadEntity of string * (OptionT<Result<DbRecord option,exn>> -> 'A)
    | WriteEntity of string * DbRecord * 'A
    | StoreSuccessMetadata of string * 'A
    | StoreFailureMetadata of string * 'A
    | SaveChanges of (Result<unit, exn> -> 'A)

type DatabaseInstruction<'A> with
    static member Map(x: DatabaseInstruction<'A>, f: 'A -> 'B) =
        match x with
        | ReadEntity (s, g) -> ReadEntity(s, g >> f)
        | WriteEntity (s, e, a) -> WriteEntity(s, e, f a)
        | StoreSuccessMetadata (s, a) -> StoreSuccessMetadata(s, f a)
        | StoreFailureMetadata (s, a) -> StoreFailureMetadata(s, f a)
        | SaveChanges (g) -> SaveChanges(g >> f)


type Program<'A> =
    Free<Coproduct<DatabaseInstruction<'A>, Coproduct<CommandLineInstruction<'A>, FileInstruction<'A>>>, 'A>

let readline : Program<_> = (Free.liftF << InR << InL) (ReadLine id)
let writeline s : Program<_> = Free.liftF(InR(InL(WriteLine(s, ()))))
let readfile f : Program<_> = Free.liftF(InR(InR(ReadFile(f, id))))
let writefile f c : Program<_> = Free.liftF(InR(InR(WriteFile(f, c, ()))))
let readentity i : Program<_> = Free.liftF(InL(ReadEntity(i, id)))
let writentity i e : Program<_> = Free.liftF(InL(WriteEntity(i, e, ())))
let storesuccessmeta s : Program<_> = Free.liftF(InL(StoreSuccessMetadata(s, ())))
let storefailuremeta s : Program<_> = Free.liftF(InL(StoreFailureMetadata(s, ())))
let savechanges : Program<_> = Free.liftF(InL(SaveChanges(id)))

let optT nn (opt: OptionT<Result<DbRecord option, exn>>) =
    monad {
        let! v = opt
        return {v with name = nn}
    }
    
let program =
    monad {
        do! storesuccessmeta "success meta"
        do! storefailuremeta "failure meta"
        do! writeline "podaj id encji"
        let! s = readline
        let! e = readentity s
        do! writeline(sprintf "wczytano %A" e)
        do! writeline "podaj nowe imie"
        let! n = readline
        let ort = monad {
            let! b = optT n e
            monad {
                do! writeline "opt trans"
                do! writeline (sprintf "z opt %A" b)
                do! writentity s b
                do! writeline "podaj nowe imie jeszcze raz"
                let! rs = readline
                let ne = {b with name = $"{rs} plus"}
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
        }
        let! _ = ort |> OptionT.run |> Result.defaultValue None |> Option.defaultValue (Pure ())
        return! savechanges
    }

type TestStateDb = {
    Metadata: string list
    ReadLines: string list
    WriteLines: string list
    Db: Map<string,DbRecord>
}

type RuntimeState = {
    OnSuccessMetadata: string option
    OnFailureMetadata: string option
}

let interpretConsole (p: CommandLineInstruction<_>): StateT<_,_> =
    match p with
    | ReadLine f ->
//        printfn "readline"
        async {
            let s = System.Console.ReadLine()
            return f s
        } |> StateT.lift
    | WriteLine (s, a) ->
//        printfn $"writeline {s}"
        async {
            do System.Console.WriteLine(s)
            return a
        } |> StateT.lift

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
                return f "pusto"
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

let interpretFile (p: FileInstruction<_>): StateT<_,_> =
    match p with
    | ReadFile (f, n) ->
//        printfn $"readfile {f}"
        async {
            let! s = File.ReadAllTextAsync f |> Async.AwaitTask
            return n s
        } |> StateT.lift
    | WriteFile (f, c, a) ->
//        printfn $"writefile {f} {c}"
        async {
            do! File.WriteAllTextAsync(f, c) |> Async.AwaitTask
            return a
        } |> StateT.lift
        
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

let seed = seq {
    yield ("testid", {name = "testowy"; age = 6})
}
let mutable db = Map(seed)

let interpretDb (p: DatabaseInstruction<_>): StateT<_,_> =
    match p with
    | ReadEntity (i, f) ->
//        printfn $"readentity {i}"
        monad {
            let o = db.TryFind i
            let t = Ok o
            let ot = OptionT t
            return f ot
        }
    | WriteEntity (i, e, a) ->
//        printfn $"writeentity {i} {e}"
        monad {
            db <- db.Add(i, e)
            return a
        }
    | StoreSuccessMetadata (s,a) ->
        monad {
            let! st = StateT.get_Get()
            let ns = {st with OnSuccessMetadata = Some s}
            do! StateT.Put ns
            return a
        }
    | StoreFailureMetadata (s,a) ->
        monad {
            let! st = StateT.get_Get()
            let ns = {st with OnFailureMetadata = Some s}
            do! StateT.Put ns
            return a
        }
    | SaveChanges f ->
        monad {
            let! st = StateT.get_Get()
            let r = Ok ()
            return f r
        }
        
let interpretDbTest (p: DatabaseInstruction<_>): State<_,_> =
    match p with
    | ReadEntity (i, f) ->
//        printfn $"readentity {i}"
        monad {
            let! s = State.get
            let o = s.Db.TryFind i
            let t = Ok o
            let ot = OptionT t
            return f ot
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
    | StoreSuccessMetadata (s,a) ->
//        printfn $"storesuccessmeta {s}"
        monad {
            let! st = State.get
            let ns = {st with Metadata = s::st.Metadata}
            do! State.put ns
            return a
        }
    | StoreFailureMetadata (s,a) ->
//        printfn $"storefailuremeta {s}"
        monad {
            let! st = State.get
            let ns = {st with Metadata = s::st.Metadata}
            do! State.put ns
            return a
        }
    | SaveChanges f ->
//        printfn $"savechanges"
        monad {
            let r = Ok ()
            return f r
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

//let t = interpretTest program
//
//let dbelems = seq {
//    yield ("testid", {name = "testowy"; age = 6})
//}
//let init = {
//    ReadLines = ["testid"]
//    WriteLines = []
//    Db = Map(dbelems)
//    Metadata = []
//}
//
//let (r,s) = State.run t init
//
//printfn "state: %A" s
//printfn "value: %A" r

let t = interpret program
let init: RuntimeState = {
    OnSuccessMetadata = None
    OnFailureMetadata = None
}

let asyn = StateT.run t init

let (r,s) = Async.RunSynchronously asyn

printfn "state: %A" s
printfn "value: %A" r
printfn "db: %A" db