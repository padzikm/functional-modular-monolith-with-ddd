namespace CompanyName.MyMeetings.Modules.Meetings.Application.CreateMember

open System
open CompanyName.MyMeetings.Modules.Meetings.Domain
open FSharpPlus
open FSharpPlus.Data
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.Validation
open MediatR
open NServiceBus

module Types =      
    [<CLIMutable>]
    type CreateMemberCommand =
        {
        MemberId: Guid
        Login: string
        FirstName: string
        LastName: string
        Email: string
        Name: string
        }
        interface ICommand with
        interface IRequest<Async<Validation<unit,string>>> with
    
    type MemberCreatedDomainEvent = {
        MemberId: Guid
    }

module Algebra =
    open Types
    
    type DatabaseInstruction<'A> =
        | SaveMember of m: Member * 'A
        
    type DatabaseInstruction<'A> with
        static member Map(x: DatabaseInstruction<'A>, f: 'A -> 'B) =
            match x with
            | SaveMember (m, a) -> SaveMember(m, f a)
            
    type DomainEventInstruction<'A> =
        | PublishMemberCreatedEvent of e: MemberCreatedDomainEvent * 'A
        
    type DomainEventInstruction<'A> with
        static member Map(x: DomainEventInstruction<'A>, f: 'A -> 'B) =
            match x with
            | PublishMemberCreatedEvent (e, a) -> PublishMemberCreatedEvent(e, f a)
            
    type LoggingInstruction<'A> =
        | LogInfo of string * 'A
        
    type LoggingInstruction<'A> with
        static member Map(x: LoggingInstruction<'A>, f: 'A -> 'B) =
            match x with
            | LogInfo (s, a) -> LogInfo(s, f a)
            
    type Program<'A> = Free<Coproduct<Coproduct<DatabaseInstruction<'A>, DomainEventInstruction<'A>>, LoggingInstruction<'A>>, 'A>

module Implementation =
    open Types
    open Algebra
    
    let saveMember m: Program<_> = SaveMember(m, ()) |> (Free.liftF << InL << InL)
    let publishMemberCreatedEvent e: Program<_> = PublishMemberCreatedEvent(e, ()) |> (Free.liftF << InL << InR)
    let logInfo s: Program<_> = LogInfo(s, ()) |> (Free.liftF << InR)
    
    let validate (cmd: CreateMemberCommand) =
        let l = Result.requireNotNull $"{nameof cmd.Login} must be not null" cmd.Login
        let n = Result.requireNotNull $"{nameof cmd.Name} must be not null" cmd.Name
        let fn = Result.requireNotNull $"{nameof cmd.FirstName} must be not null" cmd.FirstName
        let ln = Result.requireNotNull $"{nameof cmd.LastName} must be not null" cmd.LastName
        let e = Result.requireNotNull $"{nameof cmd.Email} must be not null" cmd.Email
        let g = if cmd.MemberId = Unchecked.defaultof<Guid> then Error $"{nameof cmd.MemberId} cannot be empty" else Ok cmd.MemberId
        let f _ _ _ _ _ _ = cmd
        let r = f <!^> l <*^> n <*^> fn <*^> ln <*^> e <*^> g
        r
    
    let handler (cmd: CreateMemberCommand) now = monad {
        let m: Member = {
            MemberId = cmd.MemberId; Name = cmd.Name; FirstName = cmd.FirstName; LastName = cmd.LastName
            Email = cmd.Email; Login = cmd.Login; CreatedDate = now
        }
        do! saveMember m
        let e: MemberCreatedDomainEvent = {MemberId = cmd.MemberId}
        do! logInfo "member created!"
        do! publishMemberCreatedEvent e
        do! logInfo "member created event sent"
    }
