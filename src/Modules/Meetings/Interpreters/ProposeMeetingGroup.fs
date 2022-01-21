namespace CompanyName.MyMeetings.Modules.Meetings.Interpreters.ProposeMeetingGroup

open System
open System.Threading.Tasks
open System.Threading.Tasks
open CompanyName.MyMeetings.BuildingBlocks.Application.Errors
open CompanyName.MyMeetings.Modules.Meetings.Application.ProposeMeetingGroup.Algebra
open CompanyName.MyMeetings.Modules.Meetings.Application.ProposeMeetingGroup.Types
open CompanyName.MyMeetings.Modules.Meetings.Application.ProposeMeetingGroup.Implementation
open CompanyName.MyMeetings.Modules.Meetings.Domain.DomainEvents
open CompanyName.MyMeetings.Modules.Meetings.Domain.SimpleTypes
open CompanyName.MyMeetings.Modules.Meetings.Infrastructure
open CompanyName.MyMeetings.Modules.Meetings.Infrastructure.Database
open FSharpPlus
open FSharpPlus
open FSharpPlus.Data
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.Result
open MediatR
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Logging
open NServiceBus
open FsToolkit.ErrorHandling.Operator.Validation
open CompanyName.MyMeetings.Modules.Meetings.Domain
open NServiceBus.Logging
open NServiceBus.MessageMutator

//type ProposeMeetingGroupCommandSender (logger: ILogger<ProposeMeetingGroupCommandSender>, msgSession: IMessageSession) =
//    inherit RequestHandler<ProposeMeetingGroupCommandInternal, Async<unit>>()
       

type ProposeMeetingGroupCommandValidator (logger: ILogger<ProposeMeetingGroupCommandValidator>, msgSession: IMessageSession) =
    inherit RequestHandler<ProposeMeetingGroupCommand, Async<Result<unit, Error>>>()

    override this.Handle(request) =
        let f (req: ProposeMeetingGroupCommand) = AsyncResult.ofTaskAction (msgSession.Send req) |> AsyncResult.mapError InfrastructureError
        let v = validate request
        let q = f <!> v
        async {
            let s = Result.sequenceAsync q
            let! r = s
            let u = r |> Result.mapError CommandValidationError
            let j = Result.flatten u
            return j
        }

//type ProposeMeetingGroupCmdMutator() =
//    interface IMutateIncomingMessages with
//        member this.MutateIncoming(context) =
//            let l = LogManager.GetLogger<ProposeMeetingGroupCmdMutator>()
//            l.Info "w mutatorze propose meeting group"
//            match context.Message with
//            | :? ProposeMeetingGroupCommand as cmd ->
//                l.Info "pasuje"
//                let c = createCmd cmd
//                match c with
//                | Ok d ->
//                    l.Info "jest ok"
//                    context.Message <- d
//                    Task.CompletedTask        
//                | Error e ->
//                    l.Info "nie jest ok"
//                    Task.FromException(exn(sprintf "%O" e))
////                    failwith (sprintf "%O" e)
//            | _ ->
//                l.Info "nie pasuje"
//                Task.CompletedTask
        
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
                            let memb = MeetingGroupProposal(Id = id, Name = MeetingName.value up.Name, Description = Option.defaultValue "" up.Description, ProposalDate = up.ProposalDate,
                                                            ProposalMemberId = up.ProposalMemberId, LocationCity = MeetingLocationCity.value up.Location.City,
                                                            LocationCountryCode = MeetingLocationPostcode.value up.Location.Postcode, Status = MeetingGroupProposalStatus.InVerification)
                            async {
                                let _ = dbContext.MeetingGroupProposals.Add(memb)// |> Async.AwaitTask
//                            let! _ = dbContext.SaveChangesAsync() |> Async.AwaitTask
                                return i
                            }
                        | AcceptedMeetingGroupProposal up ->
                            let (MeetingGroupProposalId id) = up.Id
                            let memb = MeetingGroupProposal(Id = id, Name = MeetingName.value up.Name, Description = Option.defaultValue "" up.Description, ProposalDate = up.ProposalDate,
                                                            ProposalMemberId = up.ProposalMemberId, LocationCity = MeetingLocationCity.value up.Location.City,
                                                            LocationCountryCode = MeetingLocationPostcode.value up.Location.Postcode, Status = MeetingGroupProposalStatus.Accepted)
                            async {
                                let _ = dbContext.MeetingGroupProposals.Add(memb)// |> Async.AwaitTask
//                            let! _ = dbContext.SaveChangesAsync() |> Async.AwaitTask
                                return i
                            }
                | InR evi ->
                    match evi with
                    | PublishMeetingGroupProposedEvent (evi, i) ->
                        logger.LogInformation (sprintf "publish member created event %A" evi)
                        async {
                            let (MeetingGroupProposalId id) = evi.Id
                            let ev: MeetingGroupProposedDomainEvent = {
                                Id = id; Name = MeetingName.value evi.Name; Description = Option.defaultValue "" evi.Description
                                ProposalUserId = evi.ProposalUserId; ProposalDate = evi.ProposalDate
                                LocationCity = MeetingLocationCity.value evi.LocationCity; LocationPostcode = MeetingLocationPostcode.value evi.LocationPostcode
                            }
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
                let r = result{
                    logger.LogInformation "message received"
                    logger.LogInformation (sprintf "%A" message)
                    let! msgInternal = createCmd message
                    let program = handler msgInternal DateTime.UtcNow (Guid.NewGuid()) (Guid.NewGuid())
                    return interpret program context
                }
                let a = Result.sequenceAsync r
                let! b = a
                match b with
                | Ok v -> 
//                    logger.LogInformation (sprintf "interpret result %A" result)
                    logger.LogInformation "message handled"
                    return v
                | Error err ->
                    logger.LogInformation "message validation failed"
                    failwith (sprintf "%O" err)
                
//                let program = handler message DateTime.UtcNow (Guid.NewGuid()) (Guid.NewGuid())
                
//                let! _ = dbContext.SaveChangesAsync() |> Async.AwaitTask
                
            } |> Async.StartAsTask :> Task
            
//    interface IHandleMessages<ProposeMeetingGroupCommandInternal> with
//        member this.Handle(message, context) =
//            async {
//                logger.LogInformation "internal"
//                logger.LogInformation "message received"
//                logger.LogInformation (sprintf "%A" message)
////                let program = handler message DateTime.UtcNow (Guid.NewGuid()) (Guid.NewGuid())
////                let! result = interpret program context
////                let! _ = dbContext.SaveChangesAsync() |> Async.AwaitTask
////                logger.LogInformation (sprintf "interpret result %A" result)
//                logger.LogInformation "message handled"
//            } |> Async.StartAsTask :> Task