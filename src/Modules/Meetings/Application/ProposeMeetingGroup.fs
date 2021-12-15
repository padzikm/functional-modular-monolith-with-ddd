namespace CompanyName.MyMeetings.Modules.Meetings.Application.ProposeMeetingGroup

open System
open System
open CompanyName.MyMeetings.Modules.Meetings.Domain
open CompanyName.MyMeetings.Modules.Meetings.Domain.DomainEvents
open CompanyName.MyMeetings.Modules.Meetings.InternalEvents.Commands
open FSharpPlus
open FSharpPlus.Data
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.Validation

module Algebra =
    
    type DatabaseInstruction<'A> =
        | SaveMeetingGroupProposal of MeetingGroupProposal * 'A
        
    type DatabaseInstruction<'A> with
        static member Map(x: DatabaseInstruction<'A>, f: 'A -> 'B) =
            match x with
            | SaveMeetingGroupProposal (m, a) -> SaveMeetingGroupProposal(m, f a)
            
    type DomainEventInstruction<'A> =
        | PublishMeetingGroupProposedEvent of e: MeetingGroupProposedDomainEvent * 'A
        
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
            
    type FreeStructure<'A> = Coproduct<Coproduct<DatabaseInstruction<'A>, DomainEventInstruction<'A>>, LoggingInstruction<'A>>
    type Program<'A> = Free<FreeStructure<'A>, 'A>

module Implementation =
    open Algebra
    
    let saveMeetingGroupProposal m: Program<_> = SaveMeetingGroupProposal(m, ()) |> (Free.liftF << InL << InL)
    let publishProposedMeetingGroupEvent e: Program<_> = PublishMeetingGroupProposedEvent(e, ()) |> (Free.liftF << InL << InR)
    let logInfo s: Program<_> = LogInfo(s, ()) |> (Free.liftF << InR)
    
    let validate (cmd: ProposeMeetingGroupCommand) =
        let n = Result.requireNotNull $"{nameof cmd.Name} must be not null" cmd.Name
        let d = Result.requireNotNull $"{nameof cmd.Description} must be not null" cmd.Description
        let lc = Result.requireNotNull $"{nameof cmd.LocationCity} must be not null" cmd.LocationCity
        let lcc = Result.requireNotNull $"{nameof cmd.LocationCountryCode} must be not null" cmd.LocationCountryCode
        let f _ _ _ _ = cmd
        let r = f <!^> n <*^> d <*^> lc <*^> lcc
        r
    
    let handler (cmd: ProposeMeetingGroupCommand) now guid = monad {
        let mgid = MeetingGroupProposalId guid
        let uid = Guid.NewGuid()
        let pd = now
        let m: MeetingGroupProposal = InVerificationMeetingGroupProposal({
            Id = mgid
            Name = cmd.Name
            Description = cmd.Description          
            ProposalDate = pd
            ProposalMemberId = uid
            Location = {
                City = cmd.LocationCity
                CountryCode = cmd.LocationCountryCode
            }
        })
        do! saveMeetingGroupProposal m
        let e: MeetingGroupProposedDomainEvent = {
            Id = mgid
            Name = cmd.Name
            Description = cmd.Description
            ProposalUserId = uid
            ProposalDate = pd
            LocationCity = cmd.LocationCity
            LocationCountryCode = cmd.LocationCountryCode
        }
        do! logInfo "meeting group proposal created!"
        do! publishProposedMeetingGroupEvent e
        do! logInfo "meeting group proposed event sent"
    }
