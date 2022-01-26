module CompanyName.MyMeetings.Modules.Meetings.Interpreters.IntegrationTests.ProposeMeetingGroup

open System
open CompanyName.MyMeetings.Modules.Meetings.Application.ProposeMeetingGroup.Types
open CompanyName.MyMeetings.Modules.Meetings.Domain.DomainEvents
open CompanyName.MyMeetings.Modules.Meetings.Infrastructure
open CompanyName.MyMeetings.Modules.Meetings.Interpreters.ProposeMeetingGroup
open FSharpPlus
open Microsoft.EntityFrameworkCore
open Microsoft.EntityFrameworkCore.SqlServer
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Logging.Abstractions
open NServiceBus
open NServiceBus.Testing
open Xunit
open FsUnit.Xunit
open Xunit.Abstractions

type Loggy<'T> (o: ITestOutputHelper) =
    interface ILogger<'T> with
        member this.BeginScope(state) =
            let t = {new IDisposable with
                        member _.Dispose() = ()
                    }
            t
        member this.IsEnabled(logLevel) = true
        member this.Log(logLevel, eventId, state, ``exception``, formatter) =
            let s = formatter.Invoke(state, ``exception``)
            o.WriteLine s

type Tests(logger: ITestOutputHelper) =

    [<Fact>]
    let ``cos``() =
        let s = Seq.initInfinite (fun _ -> Guid.NewGuid())
        let t = Seq.take 5 s
        for i in t do
            logger.WriteLine (i.ToString())
            
        for i in t do
            logger.WriteLine (i.ToString())
        true |> should equal true
    
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
        let ll = Loggy(logger)
        let o = DbContextOptionsBuilder().UseSqlServer("Server=localhost;Database=MyMeetings;User Id=sa;Password=SqlServer2019;").Options
        async {
            use dbctx = MeetingsDbContext(o)
            let h = ProposeMeetingGroupHandler(ll, dbctx) :> IHandleMessages<ProposeMeetingGroupCommand>
            let! _ = h.Handle(cmd, ctx) |> Async.AwaitTask
            let! _ = dbctx.SaveChangesAsync() |> Async.AwaitTask
            ctx.PublishedMessages.Length |> should equal 1
            let e = unbox<MeetingGroupProposedDomainEvent> ctx.PublishedMessages.[0].Message
            logger.WriteLine (sprintf "%A" e)
            use db2 = MeetingsDbContext(o)
            let! o = db2.MeetingGroupProposals.SingleOrDefaultAsync(fun m -> m.Id = e.Id) |> Async.AwaitTask
            logger.WriteLine (sprintf "%A" o)
            o |> should not' (be Null)
        }