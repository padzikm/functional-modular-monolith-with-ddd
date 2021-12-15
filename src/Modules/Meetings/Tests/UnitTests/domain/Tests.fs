module Tests

open System
open CompanyName.MyMeetings.BuildingBlocks.Domain
open CompanyName.MyMeetings.Modules.Meetings.Application.CreateMember.Algebra
open CompanyName.MyMeetings.Modules.Meetings.Application.CreateMember.Types
open CompanyName.MyMeetings.Modules.Meetings.Domain
open CompanyName.MyMeetings.Modules.Meetings.Domain
open FSharpPlus.Data
//open CompanyName.MyMeetings.Modules.Meetings.Domain.Members
open FsCheck.Xunit
open FsUnit.Xunit
open FsUnit.CustomMatchers
open Xunit
open FsCheck
open Xunit.Abstractions
//open CompanyName.MyMeetings.Modules.Meetings.Application.Members.CreateMember
open CompanyName.MyMeetings.Modules.Meetings.Application.CreateMember.Implementation
open FSharpPlus

type CallResult<'a> =
    | NotCalled
    | CalledWith of 'a
    | CalledWithWrongType

type TestState = {
            SaveMemberCalled: CallResult<Member>
            PublishMemberCreatedEventCalled: CallResult<MemberCreatedDomainEvent>
        }

let nonEmptyStr () =
    Arb.generate<NonEmptyString> |> Gen.map (fun (NonEmptyString s) -> s)

let createMemberCmd: Gen<CreateMemberCommand> =
    gen {
        let! i = Arb.generate<Guid>      
        let! l = nonEmptyStr() |> Gen.where (fun s -> s.Length <= 20) //Gen.sized <| (fun s -> (Gen.resize (max 1 s) Arb.generate<NonNull<string>>) |> Gen.map (fun (NonNull x) -> x))
        let! fn = nonEmptyStr()
        let! ln = nonEmptyStr()
        let! e = Gen.map3 (fun p1 p2 p3 -> $"{p1}@{p2}.{p3}") <| nonEmptyStr() <| nonEmptyStr() <| nonEmptyStr()
        let! n = nonEmptyStr() |> Gen.where (fun s -> s.Length <= 20)
        let r: CreateMemberCommand = {
            MemberId = i
            Login = l
            FirstName = fn
            LastName = ln
            Email = e
            Name = n
        }
        return r
    }

type MyGens =
    static member CreateMemberCmd() = Arb.fromGen createMemberCmd
    
        
type Tests (output: ITestOutputHelper) =
    

//    do Arb.register<MyGens>() |> ignore
    
//    [<Fact>]
//    let ``Create member`` () =
////        let r = Member.Create Guid.Empty null null 1 2 3
////        match r with
////        | Ok v ->
////            output.WriteLine "ok"
////            output.WriteLine (sprintf "%O" v)
////        | Error s ->
////            output.WriteLine "error"
////            output.WriteLine (sprintf "%O" s)
////        let s = sprintf "%O" r
////        output.WriteLine s
//        let ev = Events.MemberCreatedDomainEvent(Guid.NewGuid())
//        let r = ev :> IDomainEvent
//        output.WriteLine (r.Id.ToString())
//        output.WriteLine (r.Id.ToString())
//        
//        output.WriteLine (ev.MemberId.ToString())
//        output.WriteLine (ev.MemberId.ToString())
//        Assert.True true
        
//    [<Property>]
//    let ``reader handler`` (g: Guid) =
//        let cmd: CreateMemberCommand = {
//            MemberId = g
//            FirstName = "bla"
//            LastName = "test"
//            Name = "tmp"
//            Email = "cos@r.t"
//            Login = "log"
//        }
//        let r = Handler.handle cmd
//        let env: Env = {
//            Conn = "connstring"
//            Now = DateTime.UtcNow            
//        }
//        let (m, ev) = r |> Reader.run <| env
//        output.WriteLine (sprintf "%O" m)
////        let a: Member.T = {
////            MemberId = cmd.MemberId
////            FirstName = cmd.FirstName
////            LastName = cmd.LastName
////            Email = cmd.Email
////            Name = cmd.Name
////            Login = cmd.Login
////        }
//        m |> should not' (be Null)
//        Assert.NotEmpty ev
        
    
    
    [<Property(Arbitrary = [|typeof<MyGens>|])>]
    let ``create member interpreter`` (cmd: CreateMemberCommand) =
            
        let o = {SaveMemberCalled = NotCalled; PublishMemberCreatedEventCalled = NotCalled}
        
        let interpret (p: Program<_>) =
            let go v =
                match v with
                | InL l ->
                    match l with
                    | InL dbi ->
                        match dbi with
                        | SaveMember (m, i) ->
                            //logger.LogInformation (sprintf "save member %A" m)
//                            let memb = Member(Id = m.MemberId, Name = m.Name, FirstName = m.FirstName, LastName = m.LastName,
//                                                  Login = m.Login, Email = m.Email, CreatedDate = m.CreatedDate)                            
                            monad {
                                let! s = State.get
                                let ns = {s with SaveMemberCalled = CalledWith m}
                                do! State.put ns
                                return i;
//                                dbContext.Members.Add(memb) |> ignore
//                                let! _ = dbContext.SaveChangesAsync() |> Async.AwaitTask
//                                return i
                            }
                    | InR evi ->
                        match evi with
                        | PublishMemberCreatedEvent (ev, i) ->
                            //logger.LogInformation (sprintf "publish member created event %A" ev)
                            monad {
                                let! s = State.get
                                let ns = {s with PublishMemberCreatedEventCalled = CalledWith ev}
                                do! State.put ns
                                return i;
//                                return i
                            }
                | InR r ->
                    match r with
                    | LogInfo (s, i) ->
                        //logger.LogInformation (sprintf "log information %A" s)
                        monad {
                            //let! s = State.get
                            return i;
//                            return i
                        }
            Free.fold go p
        
//        output.WriteLine (sprintf "%O" cmd)
        let program = handler cmd DateTime.UtcNow
        let rs = interpret program
        let r = State.run rs
        let (_, s) = r o
                
        match s.SaveMemberCalled with
        | CalledWith m ->
            m.MemberId |> should equal cmd.MemberId
            m.Name |> should equal cmd.Name
            m.FirstName |> should equal cmd.FirstName
            m.LastName |> should equal cmd.LastName
            m.Email |> should equal cmd.Email
            m.Login |> should equal cmd.Login
        | _ as x -> x |> should be (ofCase<@ CalledWith Unchecked.defaultof<Member> @>)
        
        match s.PublishMemberCreatedEventCalled with
        | CalledWith e ->
            e.MemberId |> should equal cmd.MemberId
        | _ as x -> x |> should be (ofCase<@ CalledWith Unchecked.defaultof<MemberCreatedDomainEvent> @>)
