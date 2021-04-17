//these are similar to C# using statements
open canopy.runner.classic
open canopy.configuration
open canopy.classic

canopy.configuration.chromeDir <- System.AppContext.BaseDirectory

//start an instance of chrome
start chrome

//this is how you define a test
"main page" &&& fun _ ->
    url "http://localhost:8080"

    "h1" == "Index"

"hello page" &&& fun _ ->
    url "http://localhost:8080#hello"

    containsInsensitive "hello" (read "h1")
    
"counter page" &&& fun _ ->
    url "http://localhost:8080#counter"

    "h1" == "0"
    let btn = element "button"
    containsInsensitive "increment" (read btn)
    
    click btn
    "h1" == "1"
    click btn
    "h1" == "2"

//run all tests
run()

//printfn "press [enter] to exit"
//System.Console.ReadLine() |> ignore

quit()