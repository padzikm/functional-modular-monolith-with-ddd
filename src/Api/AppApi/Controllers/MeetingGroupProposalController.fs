namespace CompanyName.MyMeetings.Api.Controllers.MeetingGroupProposalController

open System
open System.Collections.Generic
open CompanyName.MyMeetings.Modules.Meetings.Application.ProposeMeetingGroup.Types
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.AsyncResult
open FSharpPlus
open MediatR
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Logging
open CompanyName.MyMeetings.BuildingBlocks.Application.Errors

[<CLIMutable>]
type ProposeMeetingGroupRequest = {
    Name: string
    Description: string
    LocationCity: string
    LocationCountryCode: string
}

type InfrastructureErrorResult (ex: exn) =
    inherit ObjectResult(ex)

[<Route("api/meetings/[controller]")>]
[<ApiController>]
type MeetingGroupProposalController (logger: ILogger<MeetingGroupProposalController>, config: IConfiguration, dispatch: IMediator) as this =
    inherit ControllerBase()
    
    let sendd (dispatch: IMediator) cmd =
        let r = AsyncResult.ofTask (dispatch.Send cmd)
        let y = r |> AsyncResult.foldResult id (InfrastructureError >> Error >> Async.singleton)
        let q = y |> Async.join
        q
    
    let mapError (err: Error) =
        match err with
        | InvalidCommandError verr ->
            let dict = Helpers.toMap verr
            this.ValidationProblem(ValidationProblemDetails(dict))
        | InfrastructureError dberr ->
            InfrastructureErrorResult(dberr) :> ActionResult
            
    [<HttpPost>]
    member this.ProposeMeetingGroup(req: ProposeMeetingGroupRequest) =
        let cmd: ProposeMeetingGroupCommand = {
            Name = req.Name
            Description = req.Description
            LocationCity = req.LocationCity
            LocationCountryCode = req.LocationCountryCode
        }
        
        let q = sendd dispatch cmd
        let u = q |> AsyncResult.foldResult (fun x -> this.Ok x :> ActionResult) mapError
        u
