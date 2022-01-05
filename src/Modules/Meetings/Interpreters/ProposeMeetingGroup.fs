namespace CompanyName.MyMeetings.Modules.Meetings.Interpreters.ProposeMeetingGroup

open System
open System.Threading.Tasks
open CompanyName.MyMeetings.BuildingBlocks.Application.Errors
open CompanyName.MyMeetings.Modules.Meetings.Application.ProposeMeetingGroup.Algebra
open CompanyName.MyMeetings.Modules.Meetings.Application.ProposeMeetingGroup.Types
open CompanyName.MyMeetings.Modules.Meetings.Application.ProposeMeetingGroup.Implementation
open CompanyName.MyMeetings.Modules.Meetings.Infrastructure
open CompanyName.MyMeetings.Modules.Meetings.Infrastructure.Database
open FSharpPlus
open FSharpPlus.Data
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.Result
open MediatR
open Microsoft.Extensions.Logging
open NServiceBus
open FsToolkit.ErrorHandling.Operator.Validation
open CompanyName.MyMeetings.Modules.Meetings.Domain

type ProposeMeetingGroupCommandValidator (logger: ILogger<ProposeMeetingGroupCommandValidator>, msgSession: IMessageSession) =
    inherit RequestHandler<ProposeMeetingGroupCommand, Async<Result<unit, Error>>>()

    override this.Handle(request) =
        let f (req: ProposeMeetingGroupCommand) = AsyncResult.ofTaskAction (msgSession.Send req) |> AsyncResult.mapError InfrastructureError
        let v = validate request
        let q = f <!> v
        async {
            let s = Result.sequenceAsync q
            let! r = s
            let u = r |> Result.mapError (fun x -> InvalidCommandError {ValidationErrors = x})
            let j = Result.flatten u
            return j
        }
        
type ProposeMeetingGroupHandler (logger: ILogger<ProposeMeetingGroupHandler>, dbContext: MeetingsDbContext) =
    let rec interpret (p: Program<_>) (ctx:IMessageHandlerContext) =
        let go v =
            match v with
            | InL l ->
                match l with
                | InL dbi ->
                    match dbi with
                    | SaveMeetingGroupProposal (m, i) ->
                        logger.LogInformation (sprintf "save member %A" m)
                        match m with
                        | InVerificationMeetingGroupProposal up ->
                            let (MeetingGroupProposalId id) = up.Id
                            let memb = MeetingGroupProposal(Id = id, Name = up.Name, Description = up.Description, ProposalDate = up.ProposalDate,
                                                            ProposalMemberId = up.ProposalMemberId, LocationCity = up.Location.City,
                                                            LocationCountryCode = up.Location.CountryCode, Status = MeetingGroupProposalStatus.InVerification)
                            async {
                                let _ = dbContext.MeetingGroupProposals.Add(memb)// |> Async.AwaitTask
//                            let! _ = dbContext.SaveChangesAsync() |> Async.AwaitTask
                                return i
                            }
                        | AcceptedMeetingGroupProposal up ->
                            let (MeetingGroupProposalId id) = up.Id
                            let memb = MeetingGroupProposal(Id = id, Name = up.Name, Description = up.Description, ProposalDate = up.ProposalDate,
                                                            ProposalMemberId = up.ProposalMemberId, LocationCity = up.Location.City,
                                                            LocationCountryCode = up.Location.CountryCode, Status = MeetingGroupProposalStatus.Accepted)
                            async {
                                let _ = dbContext.MeetingGroupProposals.Add(memb)// |> Async.AwaitTask
//                            let! _ = dbContext.SaveChangesAsync() |> Async.AwaitTask
                                return i
                            }
                | InR evi ->
                    match evi with
                    | PublishMeetingGroupProposedEvent (ev, i) ->
                        logger.LogInformation (sprintf "publish member created event %A" ev)
                        async {
                            let! _ = ctx.Publish ev |> Async.AwaitTask
                            return i
                        }
            | InR r ->
                match r with
                | LogInfo (s, i) ->
                    logger.LogInformation (sprintf "log information %A" s)
                    async {
                        return i
                    }
        Free.fold go p
        
    interface IHandleMessages<ProposeMeetingGroupCommand> with
        member this.Handle(message, context) =
            async {                
                logger.LogInformation "message received"
                logger.LogInformation (sprintf "%A" message)
                let program = handler message DateTime.UtcNow (Guid.NewGuid()) (Guid.NewGuid())
                let! result = interpret program context
//                let! _ = dbContext.SaveChangesAsync() |> Async.AwaitTask
                logger.LogInformation (sprintf "interpret result %A" result)
                logger.LogInformation "message handled"
            } |> Async.StartAsTask :> Task