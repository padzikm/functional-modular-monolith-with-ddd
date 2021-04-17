namespace App

open System
open Fable.Import
open Feliz
open Feliz.Router
open Fable.Core
open Fetch
open Thoth.Fetch
open Thoth.Json

type GetMeetingDetails = {
            Id: Guid
            Title: string
        }

type Components =
    /// <summary>
    /// The simplest possible React component.
    /// Shows a header with the text Hello World
    /// </summary>
    [<ReactComponent>]
    static member HelloWorld() = Html.h1 "Hello World"

    /// <summary>
    /// A stateful React component that maintains a counter
    /// </summary>
    [<ReactComponent>]
    static member Counter() =
        let (count, setCount) = React.useState(0)
        let a = async {
            let t = 3
            printfn $"w async {t}"
            return t + 1
        }
        
        let p = Async.StartAsPromise a
        p.``then``(fun v -> printfn $"wartosc {v}") |> ignore
        let url = "https://jsonplaceholder.typicode.com/todos/1"
        let f = async {
            let u = "/api/meetings/EA0231DF-12A7-475C-A1B1-53F9EF0297B7"
            let! r = Fetch.get<_, GetMeetingDetails>(u, properties = [RequestProperties.Mode RequestMode.Cors], caseStrategy = CamelCase) |> Async.AwaitPromise
            printfn $"from api {r.Id} - {r.Title}"
            setCount(-1);
            return r
        }
        f |> Async.StartAsPromise |> ignore
//        let ap = async {
//            return! Fetch.fetch url [] |> Async.AwaitPromise
//        }
        Html.div [
            Html.h1 count
            Html.button [
                prop.onClick (fun _ -> setCount(count + 1))
                prop.text "Increment"
            ]
        ]

    /// <summary>
    /// A React component that uses Feliz.Router
    /// to determine what to show based on the current URL
    /// </summary>
    [<ReactComponent>]
    static member Router() =
        let (currentUrl, updateUrl) = React.useState(Router.currentUrl())
        React.router [
            router.onUrlChanged updateUrl
            router.children [
                match currentUrl with
                | [ ] -> Html.h1 "Index"
                | [ "hello" ] -> Components.HelloWorld()
                | [ "counter" ] -> Components.Counter()
                | otherwise -> Html.h1 "Not found"
            ]
        ]