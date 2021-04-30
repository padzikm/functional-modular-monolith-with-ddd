type CommandLineInstruction<'A> =
    | ReadLine of (string -> 'A)
    | WriteLine of string * 'A

type CommandLineProgram<'A> =
    | Pure of 'A
    | Free of CommandLineInstruction<CommandLineProgram<'A>>
    
let p1 = Free (WriteLine ("heelooo", Free (ReadLine (fun s -> Free (WriteLine ($"wypisane {s}", Pure s))))))

let rec interpret p =
    match p with
    | Pure a -> a
    | Free cmd -> match cmd with
        | ReadLine f ->
            let l = System.Console.ReadLine()
            interpret (f(l))
        | WriteLine (s,a) ->
            System.Console.WriteLine(s)
            interpret a
            
            
let mapI f = function
    | ReadLine g -> ReadLine (g >> f)
    | WriteLine (s, a) -> WriteLine (s, f a) 

let rec bind f = function
    | Pure a -> f a
    | Free cmd -> mapI (bind f) cmd |> Free
    
let map f = bind (f >> Pure)

let readline = Free (ReadLine (Pure))
let writeline s = Free (WriteLine (s, Pure ()))

type CommandLineBuilder () =
    member _.Return x = Pure x
    member _.Bind (x, f) = bind f x
    member _.Zero () = Pure ()
    member _.ReturnFrom x = x
    
let commandline = CommandLineBuilder ()

let p = commandline {
    do! writeline "napisz cos"
    let! s = readline
    do! writeline $"napisales {s}"
}

interpret p
    