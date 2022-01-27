namespace CompanyName.MyMeetings.Modules.Meetings.Application.ProposeMeetingGroup

open System
open System
open CompanyName.MyMeetings.BuildingBlocks.Application.Errors
open CompanyName.MyMeetings.BuildingBlocks.Domain.Errors
//open CompanyName.MyMeetings.Modules.Meetings.Application.ProposeMeetingGroup.Types
open CompanyName.MyMeetings.Modules.Meetings.Domain
open CompanyName.MyMeetings.Modules.Meetings.Domain.DomainEvents
open CompanyName.MyMeetings.Modules.Meetings.Domain.SimpleTypes
open FSharpPlus
open FSharpPlus.Data
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.Validation
open MediatR
open NServiceBus

module Statee =
    type IEvens =
        interface
        end
    
    type FirstEnt =
        {v1: string}
    
    type FirstEv =
        {f: string}
        interface IEvens with
        
    type SecondEv =
        {g: string}
        interface IEvens with
        
    type Evs1 =
        | FirstEv of FirstEv
        | SecondEv of SecondEv
        
    type Evs2 =
        | ThirdEv of int
        
    type SecondEnt =
        {v2: string; first: FirstEnt}
    
    type Ar1 = {
        sec: SecondEnt
    }
    
    type ThirdEnt = {
        v3: int
    }
    
    type Ar2 = {
        th: ThirdEnt//; evs: Evs2 list
    }
    
    type Evss = {l: IEvens list}
    
    let do1 s =
        monad{
            let! ar = State.get
            let ar1 = { ar with
                sec = {
                    v2 = s + s; first = {v1 = s + s + s}
                }
//                evs = []
            }
            do! State.put ar1
            let e = SecondEv ({g = s})
            return e
        }
        
    let do2 v =
        monad{
            let! ar = State.get
            let ar2 = { ar with
                th = { v3 = ar.th.v3 + v }
//                evs = []
            }
            do! State.put ar2
            let e = ThirdEv v
            return e
        }
        
    let as1 s =
        let ar1 = {
                sec = {
                    v2 = ""; first = {v1 = ""}
                }
//                evs = []
            }
        let ar2 = {
            th = {v3 = 0}
        }
        let m = monad{
//            let! ss = State.get
//            let ss1 = fst ss
//            do! State.put ss1
            let! e1 = do1 s
            let! s1 = State.get
//            do! State.put ar2
//            let! e3 = do2 v
//            do! State.
            let! e2 = do1 s
            return [e1; e2]
        }
        let mm = m |> State.run
        let r = mm ar1
        r
    
    let g s =
        let ent = {v1 = s}
        let ss = {g = s}
        (ent, ss)
        
    let f s =
        let ss = {f = s}
        let h = g s
        let ent = {v2 = s; first = fst h}
        let e = {l = [ss; snd h]}
        (ent, e)
    
//    let f1 s =
//        let st = State.get
//        let u = st |> State.bind (fun t ->
//            let ent = {v1 = s}
//            let sta = State.put ent
//            let ss = {f = s}
//            let stm = State.map (fun _ -> ss) sta
//            stm)
//        u
        
    let f1 s f =
        let ent = {f with v1 = s}
        let ss = {f = s}
        ent, ss
        
    let g1 s =
        monad{
            let! st = State.get
            let b = st.first
//            let b = st.
//            let sf = monad{
//                let! gg = f1 s
//                return gg
//            }
            
            let gg = f1 s b
            let ent = {st with v2 = s; first = fst gg}
            do! State.put ent
            let ss = {g = s}
            let e = {l = [ss]}
            let ge = (snd gg) :> IEvens
            let ee = {e with l = ge :: e.l}
            return ee
        }
    
   

module Types =
    open CompanyName.MyMeetings.BuildingBlocks
    
    type CommandStatus =
    | Accepted
    | Rejected
    | Completed of Guid
    | Failed of string
    
    type ProposeMeetingGroupCommandResult = {CommandId: Guid; CommandStatus: CommandStatus}
    
    [<CLIMutable>]
    type ProposeMeetingGroupCommandRequest =
        {
        Name: string
        Description: string
        LocationCity: string
        LocationCountryCode: string
        }
        interface IRequest<Async<Result<ProposeMeetingGroupCommandResult,Error>>> with
        
        
    type ProposeMeetingGroupCommand =
            {
            CommandId: Guid
            Name: MeetingName
            Description: string option
            LocationCity: MeetingLocationCity
            LocationPostcode: MeetingLocationPostcode
            MemberId: Guid
            DateTime: DateTime
            }
        
    type GetProposeMeetingGroupCommandStatusQuery =
        {
            CommandId: Guid
        }
        interface IRequest<Async<Validation<ProposeMeetingGroupCommandResult option, Error>>> with
        
    let createCmd (cmd: ProposeMeetingGroupCommandRequest) (ctx: {|Id: Guid; MemberId: Guid|}) =
        let f n d lc lpc =
            {Name = n; Description = d; LocationCity = lc; LocationPostcode = lpc; CommandId = ctx.Id; MemberId = ctx.MemberId; DateTime = DateTime.UtcNow}
        
        f
        <!^> (MeetingName.create cmd.Name |> Result.mapError (fun er -> {Target = nameof cmd.Name; Errors = er }))
        <*^> Ok (if cmd.Description = null then None else Some cmd.Description)
        <*^> (MeetingLocationCity.create cmd.LocationCity |> Result.mapError (fun er -> {Target = nameof cmd.LocationCity; Errors = er }))
        <*^> (MeetingLocationPostcode.create cmd.LocationCountryCode |> Result.mapError (fun er -> {Target = nameof cmd.LocationCountryCode; Errors = er }))

module Algebra =
    open Types
    
    type DateTime<'A> =
        | GetCurrentDateTime of (DateTime -> 'A)
        
        static member Map((GetCurrentDateTime (g)), f: 'A -> 'B) =
            GetCurrentDateTime (g >> f)
    
    type UuidGenerator<'A> =
        | GenerateUuid of (Guid -> 'A)
        
        static member Map((GenerateUuid (g)), f: 'A -> 'B) =
            GenerateUuid (g >> f)
    
    type LoggedInUser<'A> =
        | GetLoggedInUserId of (Guid -> 'A)
        
        static member Map(x: LoggedInUser<'A>, f: 'A -> 'B) =
            let (GetLoggedInUserId g) = x
            GetLoggedInUserId (g >> f)
            
    type CommandProcessor<'A> =
        | ProcessCommand of ProposeMeetingGroupCommand * 'A
        
        static member Map((ProcessCommand (c, a)), f: 'A -> 'B) =
            ProcessCommand (c, f a)
    
    type DatabaseInstruction<'A> =
        | MarkCommandAsAccepted of Guid * 'A
        | MarkCommandAsRejected of Guid * TargetedValidationError list * 'A
        | MarkCommandAsCompleted of Guid * 'A
        | MarkCommandAsFailed of Guid * string * 'A
        | SaveMeetingGroupProposal of MeetingGroupProposal * 'A
        
        static member Map(x: DatabaseInstruction<'A>, f: 'A -> 'B) =
            match x with
            | SaveMeetingGroupProposal (m, a) -> SaveMeetingGroupProposal(m, f a)
            | MarkCommandAsAccepted (m, a) -> MarkCommandAsAccepted(m, f a)
            | MarkCommandAsCompleted (m, a) -> MarkCommandAsCompleted(m, f a)
            | MarkCommandAsRejected (m, e, a) -> MarkCommandAsRejected(m, e, f a)
            | MarkCommandAsFailed (m, e, a) -> MarkCommandAsFailed(m, e, f a)
        
//    type DatabaseInstruction<'A> with
//        member _.Map(x: DatabaseInstruction<'A>, f: 'A -> 'B) =
//            match x with
//            | SaveMeetingGroupProposal (m, a) -> SaveMeetingGroupProposal(m, f a)
            
    type DomainEventInstruction<'A> =
        | PublishMeetingGroupProposedEvent of e: MeetingGroupProposedDomainEventInternal * 'A
        
    type DomainEventInstruction<'A> with
        static member Map(x: DomainEventInstruction<'A>, f: 'A -> 'B) =
            match x with
            | PublishMeetingGroupProposedEvent (e, a) -> PublishMeetingGroupProposedEvent(e, f a)
            
    type LoggingInstruction<'A> =
        | LogInfo of string * 'A
        
    type LoggingInstruction<'A> with
        static member Map(x: LoggingInstruction<'A>, f: 'A -> 'B) =
            match x with
            | LogInfo (s, a) -> LogInfo(s, f a)
            
    type FreeStructure<'A> = Coproduct<
        Coproduct<
            CommandProcessor<'A>, Coproduct<DatabaseInstruction<'A>, DomainEventInstruction<'A>>>,
        Coproduct<
            Coproduct<LoggingInstruction<'A>, LoggedInUser<'A>>, Coproduct<UuidGenerator<'A>, DateTime<'A>>>>
    type Program<'A> = Free<FreeStructure<'A>, 'A>

module Implementation =
    open Algebra
    open Types
    
    let processCmd cmd: Program<_> = ProcessCommand(cmd, ()) |> (Free.liftF << InL << InL)
    let saveMeetingGroupProposal m: Program<_> = SaveMeetingGroupProposal(m, ()) |> (Free.liftF << InL << InR << InL)
    let markCommandAsAccepted m: Program<_> = MarkCommandAsAccepted(m, ()) |> (Free.liftF << InL << InR << InL)
    let markCommandAsRejected m e: Program<_> = MarkCommandAsRejected(m, e, ()) |> (Free.liftF << InL << InR << InL)
    let markCommandAsCompleted m: Program<_> = MarkCommandAsCompleted(m, ()) |> (Free.liftF << InL << InR << InL)
    let markCommandAsFailed m e: Program<_> = MarkCommandAsFailed(m, e, ()) |> (Free.liftF << InL << InR << InL)
    let publishProposedMeetingGroupEvent e: Program<_> = PublishMeetingGroupProposedEvent(e, ()) |> (Free.liftF << InL << InR << InR)
    let logInfo s: Program<_> = LogInfo(s, ()) |> (Free.liftF << InR << InL << InL)
    let getLoggedInUserId: Program<_> = GetLoggedInUserId(id) |> (Free.liftF << InR << InL << InR)
    let generateUuid: Program<_> = GenerateUuid(id) |> (Free.liftF << InR << InR << InL)
    let getCurrentDateTime: Program<_> = GetCurrentDateTime(id) |> (Free.liftF << InR << InR << InR)
    
    
//    let validate2 (cmd: ProposeMeetingGroupCommand) =
//        let n = Result.requireNotNull {Target = nameof cmd.Name; Message = ["must be not null"]} cmd.Name
//        let d = Result.requireNotNull {Target = nameof cmd.Description; Message = ["must be not null"]} cmd.Description
//        let lc = Result.requireNotNull {Target = nameof cmd.LocationCity; Message = ["must be not null"]} cmd.LocationCity
//        let lcc = Result.requireNotNull {Target = nameof cmd.LocationCountryCode; Message = ["must be not null"]} cmd.LocationCountryCode
//        let f _ _ _ _ = cmd
//        let r = f <!^> n <*^> d <*^> lc <*^> lcc
//        r
        
//    let validate (cmd: ProposeMeetingGroupCommand) =
//        let c = createCmd cmd
//        (fun _ -> cmd) <!> c
        
    let validate2 (cmd: ProposeMeetingGroupCommand) =
//        let c = createCmd cmd
        let t = Validation.ok cmd
        t
        
    let createCmd2 (cmd: ProposeMeetingGroupCommandRequest) (ctx: {|CommandId: Guid; MemberId: Guid; dt: DateTime|}) =
        let f n d lc lpc =
            {Name = n; Description = d; LocationCity = lc; LocationPostcode = lpc; CommandId = ctx.CommandId; MemberId = ctx.MemberId; DateTime = ctx.dt}
        
        f
        <!^> (MeetingName.create cmd.Name |> Result.mapError (fun er -> {Target = nameof cmd.Name; Errors = er }))
        <*^> Ok (if cmd.Description = null then None else Some cmd.Description)
        <*^> (MeetingLocationCity.create cmd.LocationCity |> Result.mapError (fun er -> {Target = nameof cmd.LocationCity; Errors = er }))
        <*^> (MeetingLocationPostcode.create cmd.LocationCountryCode |> Result.mapError (fun er -> {Target = nameof cmd.LocationCountryCode; Errors = er }))
        
    let handleCmdRequest (req: ProposeMeetingGroupCommandRequest) = monad{
        do! logInfo (sprintf "received cmd req: %O" req)
        let! uuid = generateUuid
        do! logInfo (sprintf "uuid: %O" uuid)
        let! dt = getCurrentDateTime
        do! logInfo (sprintf "datetime: %O" dt)
        let! u = getLoggedInUserId
        do! logInfo (sprintf "userid: %O" u)
        let c = createCmd2 req {|CommandId = uuid; MemberId = u; dt = dt|}
        let! _ = match c with
        | Ok cmd -> monad{
                do! logInfo (sprintf "cmd accepted %O" cmd)
                do! markCommandAsAccepted uuid
                do! processCmd cmd
            }
        | Error errs -> markCommandAsRejected uuid errs
        do! logInfo "request processed"
//        let b = monad{
//            let! c = createCmd2 req {|CommandId = uuid; MemberId = u; dt = dt|}
//            let! m = Result.Ok (monad{
//                do! markCommandAsAccepted uuid
//            })
//            return c
//        }
        return c
    }
    
    let handler (cmd: ProposeMeetingGroupCommand)= monad {
        let! g1 = generateUuid
        let mgid = MeetingGroupProposalId g1
        let m: MeetingGroupProposal = InVerificationMeetingGroupProposal({
            Id = mgid
            Name = cmd.Name
            Description = cmd.Description     
            ProposalDate = cmd.DateTime
            ProposalMemberId = cmd.MemberId
            Location = {
                City = cmd.LocationCity
                Postcode = cmd.LocationPostcode
            }
        })
        do! saveMeetingGroupProposal m
        let e: MeetingGroupProposedDomainEventInternal = {
            Id = mgid
            Name = cmd.Name
            Description = cmd.Description
            ProposalUserId = cmd.MemberId
            ProposalDate = cmd.DateTime
            LocationCity = cmd.LocationCity
            LocationPostcode = cmd.LocationPostcode
        }
        do! logInfo "meeting group proposal created!"
        do! publishProposedMeetingGroupEvent e
        do! logInfo "meeting group proposed event sent"
        do! markCommandAsCompleted cmd.CommandId
        do! logInfo "command completed"
    }
