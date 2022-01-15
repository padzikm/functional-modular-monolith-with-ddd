namespace CompanyName.MyMeetings.Modules.Meetings.Application.ProposeMeetingGroup

open System
open System
open CompanyName.MyMeetings.BuildingBlocks.Application.Errors
open CompanyName.MyMeetings.BuildingBlocks.Domain.Errors
open CompanyName.MyMeetings.Modules.Meetings.Domain
open CompanyName.MyMeetings.Modules.Meetings.Domain.DomainEvents
open CompanyName.MyMeetings.Modules.Meetings.Domain.SimpleTypes
open FSharpPlus
open FSharpPlus.Data
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.Validation
open MediatR
open NServiceBus

module Types =
    open CompanyName.MyMeetings.BuildingBlocks
    
    [<CLIMutable>]
    type ProposeMeetingGroupCommand =
        {
        Name: string
        Description: string
        LocationCity: string
        LocationCountryCode: string
        }
        interface ICommand with
        interface IRequest<Async<Result<unit,Error>>> with
        
    type ProposeMeetingGroupCommandInternal =
        {
        Name: MeetingName
        Description: string option
        LocationCity: MeetingLocationCity
        LocationPostcode: MeetingLocationPostcode
        }
        interface ICommand with
        
    type ValidationError = {
        Target: string
        Errors: Domain.Errors.ValidationError list
    }
        
    let createCmd n d lc lpc =
        let f n d lc lpc =
            {Name = n; Description = d; LocationCity = lc; LocationPostcode = lpc}
            
        f
        <!^> (MeetingName.create n |> Result.mapError (fun er -> {Target = "name"; Errors = er }))
        <*^> Ok (if d = null then None else Some d)
        <*^> (MeetingLocationCity.create lc |> Result.mapError (fun er -> {Target = "location city"; Errors = er }))
        <*^> (MeetingLocationPostcode.create lpc |> Result.mapError (fun er -> {Target = "location postcode"; Errors = er }))

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
    open Types
    
    let saveMeetingGroupProposal m: Program<_> = SaveMeetingGroupProposal(m, ()) |> (Free.liftF << InL << InL)
    let publishProposedMeetingGroupEvent e: Program<_> = PublishMeetingGroupProposedEvent(e, ()) |> (Free.liftF << InL << InR)
    let logInfo s: Program<_> = LogInfo(s, ()) |> (Free.liftF << InR)
    
    let validate (cmd: ProposeMeetingGroupCommand) =
        let n = Result.requireNotNull {Target = nameof cmd.Name; Message = ["must be not null"]} cmd.Name
        let d = Result.requireNotNull {Target = nameof cmd.Description; Message = ["must be not null"]} cmd.Description
        let lc = Result.requireNotNull {Target = nameof cmd.LocationCity; Message = ["must be not null"]} cmd.LocationCity
        let lcc = Result.requireNotNull {Target = nameof cmd.LocationCountryCode; Message = ["must be not null"]} cmd.LocationCountryCode
        let f _ _ _ _ = cmd
        let r = f <!^> n <*^> d <*^> lc <*^> lcc
        r
    
    let handler (cmd: ProposeMeetingGroupCommand) now g1 g2 = monad {
        let mgid = MeetingGroupProposalId g1
        let uid = g2
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
            Id = g1
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
