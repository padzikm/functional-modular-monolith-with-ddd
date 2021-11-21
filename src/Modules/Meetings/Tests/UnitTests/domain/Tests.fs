module Tests

open System
open CompanyName.MyMeetings.BuildingBlocks.Domain
open FSharpPlus.Data
open Xunit
open CompanyName.MyMeetings.Modules.Meetings.Domain.Members
open Xunit.Abstractions
open CompanyName.MyMeetings.Modules.Meetings.Application.Members.CreateMember
open FsCheck.Xunit
open FsUnit.Xunit

type Tests (output: ITestOutputHelper) =
    
    [<Fact>]
    let ``Create member`` () =
//        let r = Member.Create Guid.Empty null null 1 2 3
//        match r with
//        | Ok v ->
//            output.WriteLine "ok"
//            output.WriteLine (sprintf "%O" v)
//        | Error s ->
//            output.WriteLine "error"
//            output.WriteLine (sprintf "%O" s)
//        let s = sprintf "%O" r
//        output.WriteLine s
        let ev = Events.MemberCreatedDomainEvent(Guid.NewGuid())
        let r = ev :> IDomainEvent
        output.WriteLine (r.Id.ToString())
        output.WriteLine (r.Id.ToString())
        
        output.WriteLine (ev.MemberId.ToString())
        output.WriteLine (ev.MemberId.ToString())
        Assert.True true
        
    [<Property>]
    let ``reader handler`` (g: Guid) =
        let cmd: CreateMemberCommand = {
            MemberId = g
            FirstName = "bla"
            LastName = "test"
            Name = "tmp"
            Email = "cos@r.t"
            Login = "log"
        }
        let r = Handler.handle cmd
        let env: Env = {
            Conn = "connstring"
            Now = DateTime.UtcNow            
        }
        let (m, ev) = r |> Reader.run <| env
        output.WriteLine (sprintf "%O" m)
//        let a: Member.T = {
//            MemberId = cmd.MemberId
//            FirstName = cmd.FirstName
//            LastName = cmd.LastName
//            Email = cmd.Email
//            Name = cmd.Name
//            Login = cmd.Login
//        }
        m |> should not' (be Null)
        Assert.NotEmpty ev