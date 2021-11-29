namespace CompanyName.MyMeetings.Api.Controllers.MeetingsController

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open CompanyName.MyMeetings.Modules.Meetings.Application.GetMember.Types
open CompanyName.MyMeetings.Modules.Meetings.Application.Meetings
open CompanyName.MyMeetings.Modules.Meetings.Application.Meetings.CreateMeeting
open CompanyName.MyMeetings.Modules.Meetings.Application.Meetings.EditMeeting
open CompanyName.MyMeetings.Modules.Meetings.Application.CreateMember.Types
open FSharpPlus
//open FSharpPlus.Data
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.AsyncResult
open MediatR
open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Logging
open CompanyName.MyMeetings.Modules.Meetings.Application.Meetings.GetMeetingDetails
open NServiceBus

type EditMeetingRequest =
    { EventFeeValue: decimal option
      EventFeeCurrency: string option }
    
[<CLIMutable>]
type M =
    {Login: string; Name: string}

[<ApiController>]
[<Route("[controller]")>]
type MeetingsController(logger: ILogger<MeetingsController>, config: IConfiguration, dispatch: IMediator) =
    inherit ControllerBase()

    member private _.connectionString = config.["MeetingsConnectionString"]
    member private this.ok x = this.Ok(x) :> ActionResult

    member private this.notFound(ex: Exception) =
        this.NotFound(ex.ToString()) :> ActionResult

    member private this.badRequest(ex: Exception) =
        this.BadRequest(ex.ToString()) :> ActionResult
    
    [<HttpGet("/do")>]
    member this.Bla([<FromQuery>] login: string, [<FromQuery>] name: string) =
        let cmd: CreateMemberCommand = {
            MemberId = Guid.NewGuid()
            Login = login
            FirstName = "pierwsze"
            LastName = "drugie"
            Name = name
            Email = "mail@domena"
        }
        let r = AsyncResult.ofTask (dispatch.Send cmd)// |> Async.AwaitTask |> Async.join
        //let m = r |> AsyncResult.mapError (fun e -> Error (e.ToString()) |> Validation.ofResult |> Async.singleton)
        let y = r |> AsyncResult.foldResult id (fun e -> Error (e.ToString()) |> Validation.ofResult |> Async.singleton)
        let q = y |> Async.join
        let u = q |> AsyncResult.foldResult this.ok (fun er -> this.BadRequest(er) :> ActionResult)
        u
//        async {
//            let! _ = dispatch.Send cmd |> Async.AwaitTask |> Async.join
//            return "udao sie"
//        }
        //this.ok "udalo sie"
        
    [<HttpGet("get/{str}")>]
    member this.Cos(str: Guid) =
        let query: GetMemberQuery = {
            Id = str
        }
        let r = AsyncResult.ofTask (dispatch.Send query)// |> Async.AwaitTask |> Async.join
        let p = r |> AsyncResult.foldResult id (fun e -> Error (e.ToString()) |> Validation.ofResult |> Async.singleton)
        let w = p |> Async.join
        let b = w |> AsyncResult.foldResult this.ok (fun er -> this.BadRequest(er) :> ActionResult)
        b
//        async {
//            let! r = dispatch.Send query |> Async.AwaitTask |> Async.join
//            let b = r |> Result.fold this.ok (fun er -> this.BadRequest(er) :> ActionResult)
//            return b
//            return match r with
//                | Ok v -> this.ok v
//                | Error er -> this.BadRequest(er) :> ActionResult
        //}
    
//    [<HttpGet("{id}")>]
//    member this.GetMeetingDetails(id: Guid) =
//        let query : GetMeetingDetailsQuery = GetMeetingDetailsQuery(id)
//
//        MeetingsModule.getMeetingDetails this.connectionString query
//        |> AsyncResult.foldResult this.ok this.notFound
//
//
//    [<HttpPost>]
//    member this.CreateNewMeeting([<FromBody>] request: CreateMeetingCommand) =
//        MeetingsModule.createMeeting this.connectionString request
//        |> AsyncResult.foldResult this.ok this.badRequest
//
//
//    [<HttpPut("{id}")>]
//    member this.EditMeeting([<FromRoute>] id: Guid, [<FromBody>] request: EditMeetingRequest) =
//        let command : EditMeetingCommand =
//                { Id = id
//                  EventFeeValue = request.EventFeeValue
//                  EventFeeCurrency = request.EventFeeCurrency }
//        MeetingsModule.editMeeting this.connectionString command
//        |> AsyncResult.foldResult this.ok this.badRequest
