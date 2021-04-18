module CompanyName.MyMeetings.Web.MeetingDetails

open System
open Feliz
open Thoth.Fetch
open Fable.Core
open Thoth.Json

type MeetingDetails = {
    Id: string
    Title: string
}

[<ReactComponent>]
let DisplayMeetingDetails = fun (id: Guid) ->
    let isLoading, setLoading = React.useState(true)
    let meetingDetails, setMeetingDetails = React.useState(Unchecked.defaultof<MeetingDetails>)
    let load () = async {
        printfn "zaczynam pobieranie"
        let! data = Fetch.get<_, MeetingDetails>($"/api/meetings/{id}", caseStrategy = CaseStrategy.CamelCase) |> Async.AwaitPromise
        printfn "koniec pobierania"
        printfn "%O" data
        setMeetingDetails data
        setLoading false
    }
    React.useEffect(load >> Async.StartImmediate, [|id :> obj|])
    
    if isLoading then
        Html.div [prop.text "loading"]
    else
    React.fragment [
        Html.h1 "meeting details"
        Html.div [
            Html.label [prop.htmlFor "id"; prop.text "id"]
            Html.input [
                prop.id "id"
                prop.type'.text
                prop.readOnly true
                prop.value meetingDetails.Id
            ]
            Html.label [prop.htmlFor "title"; prop.text "title"]
            Html.input [
                prop.id "title"
                prop.type'.text
                prop.readOnly true
                prop.value meetingDetails.Title
            ]
        ]
    ]