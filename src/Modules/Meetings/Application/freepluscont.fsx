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
//    do! writeline "write sth"
//    let! s = readline
//    do! writeline $"you wrote {s}"
        do! writeline "stack overflow test"
        for i=1 to 100000 do
            do! writeline "bla"
}

//let rec interpret (p: FreeCmd<'A>): Cont<_,_> =
//    match Free.run p with
//    | Pure x -> monad { return x }
//    | Roll v -> match v with
//        | ReadLine f ->
//                monad {
//                    let l = System.Console.ReadLine()
//                    return! interpret (f(l))
//                }
//        | WriteLine (s,a) ->
//                monad {
//                    System.Console.WriteLine(s)
//                    return! interpret a
//                }
let rec interpret2 (p: FreeCmd<'A>) =
    let go v : Cont<_,_> =
        match v with
        | ReadLine f ->
                monad {
                    let l = System.Console.ReadLine()
                    return f l
                }
        | WriteLine (s,a) ->
                monad {
                    System.Console.WriteLine(s)
                    return a
                }
    Free.fold go program
                    
let c = Cont.run (interpret2 program) id