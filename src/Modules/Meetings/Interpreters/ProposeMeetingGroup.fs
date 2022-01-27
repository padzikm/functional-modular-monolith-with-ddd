namespace CompanyName.MyMeetings.Modules.Meetings.Interpreters.ProposeMeetingGroup

open System
open System.Threading.Tasks
open System.Threading.Tasks
open CompanyName.MyMeetings.BuildingBlocks.Application.Errors
open CompanyName.MyMeetings.Modules.Meetings.Application.ProposeMeetingGroup.Algebra
open CompanyName.MyMeetings.Modules.Meetings.Application.ProposeMeetingGroup.Types
open CompanyName.MyMeetings.Modules.Meetings.Application.ProposeMeetingGroup.Implementation
open CompanyName.MyMeetings.Modules.Meetings.Application.ProposeMeetingGroup.Types
open CompanyName.MyMeetings.Modules.Meetings.Domain.DomainEvents
open CompanyName.MyMeetings.Modules.Meetings.Domain.SimpleTypes
open CompanyName.MyMeetings.Modules.Meetings.Infrastructure
open CompanyName.MyMeetings.Modules.Meetings.Infrastructure.Database
open FSharpPlus
open FSharpPlus
open FSharpPlus.Data
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.AsyncResult
open FsToolkit.ErrorHandling.Operator.Result
open MediatR
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Logging
open NServiceBus
open FsToolkit.ErrorHandling.Operator.Validation
open CompanyName.MyMeetings.Modules.Meetings.Domain
open NServiceBus.Logging
open NServiceBus.MessageMutator
open Microsoft.EntityFrameworkCore

//type ProposeMeetingGroupCommandSender (logger: ILogger<ProposeMeetingGroupCommandSender>, msgSession: IMessageSession) =
//    inherit RequestHandler<ProposeMeetingGroupCommandInternal, Async<unit>>()
       
[<CLIMutable>]
type ProposeMeetingGroupCommandDto =
    {
    CommandId: Guid
    Name: string
    Description: string
    LocationCity: string
    LocationCountryCode: string
    DateTime: DateTime
    MemberId: Guid
    }
    interface ICommand with
        
type ProposeMeetingGroupCommandValidator (logger: ILogger<ProposeMeetingGroupCommandValidator>, dbContext: MeetingsDbContext, msgSession: IMessageSession) =
    inherit RequestHandler<ProposeMeetingGroupCommandRequest, Async<Result<ProposeMeetingGroupCommandRequestResult, Error>>>()

    let rec interpret (p: Program<_>) (ctx:IMessageSession) =
        let go v =
            match v with
            | InL l ->
                match l with
                | InL ll ->
                    match ll with
                    | ProcessCommand (c, i) ->
                        async{
                            let cmd: ProposeMeetingGroupCommandDto = {
                                    CommandId = c.CommandId; Name = MeetingName.value c.Name; Description = Option.defaultValue "" c.Description
                                    MemberId = c.MemberId; DateTime = c.DateTime
                                    LocationCity = MeetingLocationCity.value c.LocationCity; LocationCountryCode = MeetingLocationPostcode.value c.LocationPostcode
                                }
                            let! _ = ctx.Send cmd |> Async.AwaitTask
                            return i
                        }
                | InR lr ->
                    match lr with 
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
//                                let! _ = dbContext.SaveChangesAsync() |> Async.AwaitTask
                                    return i
                                }
                            | AcceptedMeetingGroupProposal up ->
                                let (MeetingGroupProposalId id) = up.Id
                                let memb = MeetingGroupProposal(Id = id, Name = MeetingName.value up.Name, Description = Option.defaultValue "" up.Description, ProposalDate = up.ProposalDate,
                                                                ProposalMemberId = up.ProposalMemberId, LocationCity = MeetingLocationCity.value up.Location.City,
                                                                LocationCountryCode = MeetingLocationPostcode.value up.Location.Postcode, Status = MeetingGroupProposalStatus.Accepted)
                                async {
                                    let _ = dbContext.MeetingGroupProposals.Add(memb)// |> Async.AwaitTask
//                                let! _ = dbContext.SaveChangesAsync() |> Async.AwaitTask
                                    return i
                                }
                        | MarkCommandAsAccepted (g, i) ->
                            async{
                                let cmd = Command(Id = g, DateTime = DateTime.UtcNow, CommandStatus = CommandStatus.Accepted)
                                let _ = dbContext.Commands.Add cmd
                                return i
                            }
                        | MarkCommandAsRejected (g, v, i) ->
                            async{
                                let er = sprintf "%O" v
                                let cmd = Command(Id = g, DateTime = DateTime.UtcNow, CommandStatus = CommandStatus.Rejected, Error = er)
                                let _ = dbContext.Commands.Add cmd
                                return i
                            }
                        | MarkCommandAsCompleted (g, i) ->
                            async{
                                return i
                            }
                        | MarkCommandAsFailed (g, s, i) ->
                            async{
                                return i
                            }
                    | InR evi ->
                        match evi with
                        | PublishMeetingGroupProposedEvent (eve, i) ->
                            logger.LogInformation (sprintf "publish member created event %A" eve)
                            async {
                                let (MeetingGroupProposalId id) = eve.Id
                                let ev: MeetingGroupProposedDomainEvent = {
                                    Id = id; Name = MeetingName.value eve.Name; Description = Option.defaultValue "" eve.Description
                                    ProposalUserId = eve.ProposalUserId; ProposalDate = eve.ProposalDate
                                    LocationCity = MeetingLocationCity.value eve.LocationCity; LocationPostcode = MeetingLocationPostcode.value eve.LocationPostcode
                                }
                                let! _ = ctx.Publish ev |> Async.AwaitTask
                                return i
                            }
            | InR r ->
                match r with
                | InL rl ->
                    match rl with
                    | InL rll ->
                        match rll with
                        | LogInfo (s, i) ->
                            logger.LogInformation (sprintf "log information %A" s)
                            async {
                                return i
                            }
                    | InR rlr ->
                        match rlr with
                        | GetLoggedInUserId g ->
                            async{
                                let guid = Guid.NewGuid()
                                return g guid
                            }
                            
                | InR rr ->
                    match rr with
                    | InL rrl ->
                        match rrl with
                        | GenerateUuid g ->
                            async{
                                let gid = Guid.NewGuid()
                                return g gid
                            }
                    | InR rrr ->
                        match rrr with
                        | GetCurrentDateTime g ->
                            async{
                                let dt = DateTime.UtcNow
                                return g dt
                            }
        Free.fold go p
    
    override this.Handle(request) =
        
        let r = handleCmdRequest request
        let inter = interpret r msgSession
        
        let ime = inter |> AsyncResult.mapError CommandValidationError
        let imp = ime |> AsyncResult.map (fun c ->
            let r: ProposeMeetingGroupCommandRequestResult = {CommandId = c.CommandId}
            r)
        let c = imp |> Async.Catch
        let cr = c |> Async.map Result.ofChoice
        let crer = cr |> AsyncResult.mapError InfrastructureError
        let cs = crer |> AsyncResult.foldResult id Error
        let o = asyncResult{
            let! r = cs
            let! _ = dbContext.SaveChangesAsync() |> AsyncResult.ofTask |> AsyncResult.mapError InfrastructureError
//            let scer = sc |> AsyncResult.mapError InfrastructureError
//            let scer = sc |> Result.mapError InfrastructureError
//            let z = Result.map2 (fun res _ -> res) r scer
            return r
        }
        o
//        cs
        //imp

//[<AbstractClass>]
//type CommandValidator<'a, 'b when 'a :> IRequest<Async<Result<ProposeMeetingGroupCommandResult, Error>>> and 'b :> ICommand> (logger: ILogger, msgSession: IMessageSession) =
//    inherit RequestHandler<'a, Async<Result<ProposeMeetingGroupCommandResult, Error>>>()
//    
//    default this.Handle(request) =
//        let f (req: 'b) = AsyncResult.ofTaskAction (msgSession.Send req) |> AsyncResult.mapError InfrastructureError
//        let v = this.validate request
//        let q = f <!> (this.convertToDto <!> v)
//        async {
//            let s = Result.sequenceAsync q
//            let! r = s
//            let u = r |> Result.mapError CommandValidationError
//            let j = Result.flatten u
//            return j
//        }
//        
//    abstract member validate: 'a -> Result<'a, TargetedValidationError list>
//    
//    abstract member convertToDto: 'a -> 'b

//type ProposeMeetingGroupCommandValidator (logger: ILogger<ProposeMeetingGroupCommandValidator>, msgSession: IMessageSession) =
//    inherit CommandValidator<ProposeMeetingGroupCommandRequest, ProposeMeetingGroupCommandDto>(logger, msgSession)

//    default this.Handle(request) =
//        let f (req: ProposeMeetingGroupCommand) = AsyncResult.ofTaskAction (msgSession.Send req) |> AsyncResult.mapError InfrastructureError
//        let v = validate request
//        let q = f <!> v
//        async {
//            let s = Result.sequenceAsync q
//            let! r = s
//            let u = r |> Result.mapError CommandValidationError
//            let j = Result.flatten u
//            return j
//        }

//    override this.validate a =
//        let c = createCmd a {|Id = Guid.NewGuid(); MemberId = Guid.NewGuid()|}
//        (fun _ -> a) <!> c
//        
//    override this.convertToDto b =
//        let c = {
//            Id = Guid.NewGuid()
//            Name = b.Name
//            Description = b.Description
//            LocationCity = b.LocationCity
//            LocationCountryCode = b.LocationCountryCode
//            MemberId = Guid.NewGuid()
//        }
//        c

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
                | InL ll ->
                    match ll with
                    | ProcessCommand (c, i) ->
                        async{
                            return i
                        }
                | InR lr ->
                    match lr with 
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
//                                let! _ = dbContext.SaveChangesAsync() |> Async.AwaitTask
                                    return i
                                }
                            | AcceptedMeetingGroupProposal up ->
                                let (MeetingGroupProposalId id) = up.Id
                                let memb = MeetingGroupProposal(Id = id, Name = MeetingName.value up.Name, Description = Option.defaultValue "" up.Description, ProposalDate = up.ProposalDate,
                                                                ProposalMemberId = up.ProposalMemberId, LocationCity = MeetingLocationCity.value up.Location.City,
                                                                LocationCountryCode = MeetingLocationPostcode.value up.Location.Postcode, Status = MeetingGroupProposalStatus.Accepted)
                                async {
                                    let _ = dbContext.MeetingGroupProposals.Add(memb)// |> Async.AwaitTask
//                                let! _ = dbContext.SaveChangesAsync() |> Async.AwaitTask
                                    return i
                                }
                        | MarkCommandAsAccepted (g, i) ->
                            async{
                                return i
                            }
                        | MarkCommandAsRejected (g, v, i) ->
                            async{
                                return i
                            }
                        | MarkCommandAsCompleted (g, i) ->
                            async{
                                let! cm = dbContext.Commands.SingleAsync(fun c -> c.Id = g) |> Async.AwaitTask
                                cm.CommandStatus <- CommandStatus.Completed
//                                let _ = dbContext.Commands.Add cmd
                                return i
                            }
                        | MarkCommandAsFailed (g, s, i) ->
                            async{
                                return i
                            }
                    | InR evi ->
                        match evi with
                        | PublishMeetingGroupProposedEvent (eve, i) ->
                            logger.LogInformation (sprintf "publish member created event %A" eve)
                            async {
                                let (MeetingGroupProposalId id) = eve.Id
                                let ev: MeetingGroupProposedDomainEvent = {
                                    Id = id; Name = MeetingName.value eve.Name; Description = Option.defaultValue "" eve.Description
                                    ProposalUserId = eve.ProposalUserId; ProposalDate = eve.ProposalDate
                                    LocationCity = MeetingLocationCity.value eve.LocationCity; LocationPostcode = MeetingLocationPostcode.value eve.LocationPostcode
                                }
                                let! _ = ctx.Publish ev |> Async.AwaitTask
                                return i
                            }
            | InR r ->
                match r with
                | InL rl ->
                    match rl with
                    | InL rll ->
                        match rll with
                        | LogInfo (s, i) ->
                            logger.LogInformation (sprintf "log information %A" s)
                            async {
                                return i
                            }
                    | InR rlr ->
                        match rlr with
                        | GetLoggedInUserId g ->
                            async{
                                let guid = Guid.NewGuid()
                                return g guid
                            }
                            
                | InR rr ->
                    match rr with
                    | InL rrl ->
                        match rrl with
                        | GenerateUuid g ->
                            async{
                                let gid = Guid.NewGuid()
                                return g gid
                            }
                    | InR rrr ->
                        match rrr with
                        | GetCurrentDateTime g ->
                            async{
                                let dt = DateTime.UtcNow
                                return g dt
                            }
        Free.fold go p
//        
    interface IHandleMessages<ProposeMeetingGroupCommandDto> with
        member this.Handle(message, context) =
            async {                
                let r = result{
                    logger.LogInformation "message received"
                    logger.LogInformation (sprintf "%A" message)
                    let c = { Name = message.Name
                              Description = message.Description
                              LocationCity = message.LocationCity
                              LocationCountryCode = message.LocationCountryCode }
                    
                    let! msgInternal = createCmd2 c {|CommandId = message.CommandId; MemberId = message.MemberId; dt = message.DateTime|}
                    let program = handler msgInternal
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