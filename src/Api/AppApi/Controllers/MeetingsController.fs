namespace CompanyName.MyMeetings.Api.Controllers.MeetingsController

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open CompanyName.MyMeetings.Modules.Meetings.Application.Meetings
open CompanyName.MyMeetings.Modules.Meetings.Application.Meetings.CreateMeeting
open CompanyName.MyMeetings.Modules.Meetings.Application.Meetings.EditMeeting
open CompanyName.MyMeetings.Modules.Meetings.Infrastructure
open CompanyName.MyMeetings.Modules.Meetings.Infrastructure.MeetingsModule
open FSharpPlus
open FSharpPlus.Data
open FsToolkit.ErrorHandling
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Logging
open CompanyName.MyMeetings.Modules.Meetings.Application.Meetings.GetMeetingDetails

type EditMeetingRequest =
    { EventFeeValue: decimal option
      EventFeeCurrency: string option }

[<ApiController>]
[<Route("[controller]")>]
type MeetingsController(logger: ILogger<MeetingsController>, config: IConfiguration) =
    inherit ControllerBase()

    member private _.connectionString = config.["MeetingsConnectionString"]
    member private this.ok x = this.Ok(x) :> ActionResult

    member private this.notFound(ex: Exception) =
        this.NotFound(ex.ToString()) :> ActionResult

    member private this.badRequest(ex: Exception) =
        this.BadRequest(ex.ToString()) :> ActionResult
    
    [<HttpGet("{id}")>]
    member this.GetMeetingDetails(id: Guid) =
        let query : GetMeetingDetailsQuery = GetMeetingDetailsQuery(id)

        MeetingsModule.getMeetingDetails this.connectionString query
        |> AsyncResult.foldResult this.ok this.notFound


    [<HttpPost>]
    member this.CreateNewMeeting([<FromBody>] request: CreateMeetingCommand) =
        MeetingsModule.createMeeting this.connectionString request
        |> AsyncResult.foldResult this.ok this.badRequest


    [<HttpPut("{id}")>]
    member this.EditMeeting([<FromRoute>] id: Guid, [<FromBody>] request: EditMeetingRequest) =
        let command : EditMeetingCommand =
                { Id = id
                  EventFeeValue = request.EventFeeValue
                  EventFeeCurrency = request.EventFeeCurrency }
        MeetingsModule.editMeeting this.connectionString command
        |> AsyncResult.foldResult this.ok this.badRequest
