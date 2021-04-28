module CompanyName.MyMeetings.Web.App

open System
open Elmish
open Feliz
open Thoth.Fetch
open Thoth.Json
open Fable.Core

type MeetingDetails = { Id: string; Title: string }

type State =
    | Init
    | Loading
    | Loaded of MeetingDetails
    | LoadedProblem of string

type Msg =
    | Load of Guid
    | Success of MeetingDetails
    | Failure of string

let init () = Init, Cmd.none

let update (msg: Msg) (state: State) =
    match msg with
    | Load id ->
        let load =
            async {
                printfn "zaczynam pobieranie"

                let! data =
                    Fetch.tryGet<_, MeetingDetails>($"/api/meetings/{id}", caseStrategy = CaseStrategy.CamelCase)
                    |> Async.AwaitPromise

                printfn "koniec pobierania"
                printfn "%O" data

                return
                    match data with
                    | Ok m -> Success m
                    | Error ex ->
                        match ex with
                        | PreparingRequestFailed e -> Failure e.Message
                        | NetworkError e -> Failure e.Message
                        | FetchFailed e -> Failure e.StatusText
                        | DecodingFailed e -> Failure e
            }

        Loading, Cmd.OfAsync.result load
    | Success m -> Loaded m, Cmd.none
    | Failure e -> LoadedProblem e, Cmd.none

let render (state: State) (dispatch: Msg -> unit) =
    match state with
    | Init -> Html.div [ Html.button [ prop.text "load"; prop.onClick(fun _ -> dispatch (Load (Guid.Parse("EA0231DF-12A7-475C-A1B1-53F9EF0297B7")))) ] ]
    | Loading -> Html.div "loading"
    | Loaded meetingDetails ->
        React.fragment [ Html.h1 "meeting details"
                         Html.div [ Html.label [ prop.htmlFor "id"; prop.text "id" ]
                                    Html.input [ prop.id "id"
                                                 prop.type'.text
                                                 prop.readOnly true
                                                 prop.value meetingDetails.Id ]
                                    Html.label [ prop.htmlFor "title"; prop.text "title" ]
                                    Html.input [ prop.id "title"
                                                 prop.type'.text
                                                 prop.readOnly true
                                                 prop.value meetingDetails.Title ] ] ]
    | LoadedProblem problem -> Html.div problem
