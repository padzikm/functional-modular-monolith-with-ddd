module Tests

open System
open System.Collections.Generic
open System.Threading
open CompanyName.MyMeetings.BuildingBlocks.Application.Errors
open CompanyName.MyMeetings.Modules.Meetings.Application.CreateMember.Types
open CompanyName.MyMeetings.Modules.Meetings.Application.ProposeMeetingGroup.Types
open CompanyName.MyMeetings.Modules.Meetings.Infrastructure
open CompanyName.MyMeetings.Modules.Meetings.Interpreters.CreateMember
open CompanyName.MyMeetings.Modules.Meetings.Interpreters.ProposeMeetingGroup
open FSharpPlus
open FSharpPlus
open MediatR
open Microsoft.EntityFrameworkCore
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Logging.Abstractions
open NServiceBus.MessageMutator
open NServiceBus.MessageMutator
open NServiceBus.Testing
open Xunit
open FsUnit.Xunit
open NServiceBus
open FsUnit.CustomMatchers

[<Fact>]
let ``msg mutator``() =
    let m = ProposeMeetingGroupCmdMutator()
    let cmd: ProposeMeetingGroupCommand = {
        Name = "adsf"
        Description = "adf"
        LocationCity = "qwer"
        LocationCountryCode = "lkjh"
    }
//    let d = Dictionary<string,string>()
    let c = MutateIncomingMessageContext(cmd, Dictionary<string,string>())
    async {
        do! (m :> IMutateIncomingMessages).MutateIncoming(c) |> Async.AwaitTask
        c.Message |> should not' (equal cmd)
    }
  
[<Fact>]
let ``msg mutator null``() =
    let m = ProposeMeetingGroupCmdMutator()
    let cmd: ProposeMeetingGroupCommand = {
        Name = String.replicate 1025 "q"
        Description = "adf"
        LocationCity = "qwer"
        LocationCountryCode = "lkjh"
    }
//    let d = Dictionary<string,string>()
    let c = MutateIncomingMessageContext(cmd, Dictionary<string,string>())
    async {
        do! (m :> IMutateIncomingMessages).MutateIncoming(c) |> Async.AwaitTask
        c.Message |> should not' (equal cmd)
    }

type Sth = {Tmp: string}

[<Fact>]
let ``msg mutator other``() =
    let m = ProposeMeetingGroupCmdMutator()
    let cmd: Sth = {Tmp = "asdf"}
//    let d = Dictionary<string,string>()
    let c = MutateIncomingMessageContext(cmd, Dictionary<string,string>())
    async {
        do! (m :> IMutateIncomingMessages).MutateIncoming(c) |> Async.AwaitTask
        c.Message |> should equal cmd
    }

[<Fact>]
let ``create member cmd interpreter`` () =
    let cmd: CreateMemberCommand = {
        MemberId = Guid.NewGuid()
        Login = "intertest"
        Name = "adfafdads"
        FirstName = "qwerqwer"
        LastName = "ergafsd"
        Email = "asdf@asdf.sdgh"
    }
    let context = TestableMessageHandlerContext()
    let r = NullLogger<CreateMemberHandler>.Instance
    let opt = DbContextOptionsBuilder().Options
    
    async{
        use ctx = MeetingsDbContext(opt)
        let handler = CreateMemberHandler(r, ctx) :> IHandleMessages<CreateMemberCommand>
        let! _ = handler.Handle(cmd, context) |> Async.AwaitTask
        context.PublishedMessages.Length |> should equal 0
    }

[<Fact>]
let ``propose meeting group`` () =
    let cmd: ProposeMeetingGroupCommand = {
        Name = "bla"
        Description = "some desc"
        LocationCity = "stw"
        LocationCountryCode = "23-42"
    }
    let ctx = TestableMessageHandlerContext()
    let l = NullLogger<ProposeMeetingGroupHandler>.Instance
    let o = DbContextOptionsBuilder().Options
    async {
        use dbctx = MeetingsDbContext(o)
        let h = ProposeMeetingGroupHandler(l, dbctx) :> IHandleMessages<ProposeMeetingGroupCommand>
        let! _ = h.Handle(cmd, ctx) |> Async.AwaitTask
        ctx.SentMessages.Length |> should equal 0
    }
    
[<Fact>]
let ``propose meeting group validator invalid`` () =
   let cmd: ProposeMeetingGroupCommand = {
       Name = null; Description = null; LocationCity = null; LocationCountryCode = null
   }
   let s = TestableMessageSession()
   let l = NullLogger<ProposeMeetingGroupCommandValidator>.Instance
   let v = ProposeMeetingGroupCommandValidator(l, s) :> IRequestHandler<ProposeMeetingGroupCommand, Async<Result<unit, Error>>>
   async {
    let! r = v.Handle(cmd, CancellationToken.None) |> Async.AwaitTask |> Async.join
    
    match r with
    | Ok _ as x -> r |> should be (ofCase<@ Error Unchecked.defaultof<Error> :> Result<unit, Error> @>)
    | Error z -> match z with
        | InvalidCommandError cmderr -> cmderr.ValidationErrors.Length |> should equal 4
        | _ as y -> z |> should be (ofCase<@ InvalidCommandError Unchecked.defaultof<InvalidCommandError> @>)
    
    s.SentMessages.Length |> should equal 0
   }
   
[<Fact>]
let ``propose meeting group validator valid`` () =
   let cmd: ProposeMeetingGroupCommand = {
       Name = null; Description = null; LocationCity = null; LocationCountryCode = null
   }
   let s = TestableMessageSession()
   let l = NullLogger<ProposeMeetingGroupCommandValidator>.Instance
   let v = ProposeMeetingGroupCommandValidator(l, s) :> IRequestHandler<ProposeMeetingGroupCommand, Async<Result<unit, Error>>>
   async {
    let! r = v.Handle(cmd, CancellationToken.None) |> Async.AwaitTask |> Async.join
    
    match r with
    | Ok _ as x -> r |> should be (ofCase<@ Error Unchecked.defaultof<Error> :> Result<unit, Error> @>)
    | Error z -> match z with
        | InvalidCommandError cmderr -> cmderr.ValidationErrors.Length |> should equal 4
        | _ as y -> z |> should be (ofCase<@ InvalidCommandError Unchecked.defaultof<InvalidCommandError> @>)
    
    s.SentMessages.Length |> should equal 1
   }
