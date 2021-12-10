module Tests

open System
open CompanyName.MyMeetings.Modules.Meetings.Application.CreateMember.Types
open CompanyName.MyMeetings.Modules.Meetings.Infrastructure
open CompanyName.MyMeetings.Modules.Meetings.Interpreters.CreateMember
open Microsoft.EntityFrameworkCore
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Logging.Abstractions
open NServiceBus
open NServiceBus.Testing
open Xunit
open FsUnit.Xunit

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
