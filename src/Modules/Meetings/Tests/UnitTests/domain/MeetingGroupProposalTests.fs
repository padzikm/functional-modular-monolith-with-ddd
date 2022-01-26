module CompanyName.MyMeetings.Modules.Meetings.Domain.UnitTests.MeetingGroupProposalTests
//
//open System
//open System
//open CompanyName.MyMeetings.Modules.Meetings.Application.ProposeMeetingGroup.Types
//open CompanyName.MyMeetings.Modules.Meetings.Application
//open CompanyName.MyMeetings.Modules.Meetings.Domain
//open CompanyName.MyMeetings.Modules.Meetings.Domain.DomainEvents
//open CompanyName.MyMeetings.Modules.Meetings.Domain.SimpleTypes
////open CompanyName.MyMeetings.Modules.Meetings.InternalEvents.Commands
//open FSharpPlus.Data
//open FSharpPlus
//open FsCheck.Xunit
//open Xunit
//open FsUnit.Xunit
//open Xunit.Abstractions
//open FsCheck
//open FsUnit.CustomMatchers
//
//let proposeMeetingGroupCmdGen: Gen<ProposeMeetingGroupCommand> = gen {
//    let! n = Gen.sized (fun s -> Gen.arrayOfLength (min s 3) Arb.generate<char>) |> Gen.map String
//    let! d = Arb.generate<string>
//    let! c = Arb.generate<NonEmptyString> |> Gen.map (fun (NonEmptyString x) -> x)
//    let! cc = Arb.generate<NonEmptyString> |> Gen.map (fun (NonEmptyString x) -> x)
//    return {
//        Name = n
//        Description = d
//        LocationCity = c
//        LocationCountryCode = cc
//    }
//}
//
//type CallResult<'A> =
//    | CalledWith of 'A
//    | NotCalled
//
//type ProposeMeetingGroupTestState = {
//    SaveMeetingGroupProposalCalled: CallResult<MeetingGroupProposal>
//    PublishMeetingGroupProposedEventCalled: CallResult<MeetingGroupProposedDomainEvent>
//}
//
//type MyGens =
//    static member ProposeMeetingGroupCmd() = Arb.fromGen proposeMeetingGroupCmdGen
//    
//let interpret (p: ProposeMeetingGroup.Algebra.Program<_>) =
//            let go (v: ProposeMeetingGroup.Algebra.FreeStructure<_>) =
//                match v with
//                | InL l ->
//                    match l with
//                    | InL dbi ->
//                        match dbi with
//                        | ProposeMeetingGroup.Algebra.SaveMeetingGroupProposal (mgp, n) ->
//                            monad {
//                                let! s = State.get
//                                let ns = {s with SaveMeetingGroupProposalCalled = CalledWith mgp}
//                                do! State.put ns
//                                return n
//                            }
//                    | InR evi ->
//                        match evi with
//                        | ProposeMeetingGroup.Algebra.PublishMeetingGroupProposedEvent (ev, n) ->
//                            monad {
//                                let! s = State.get
//                                let ns = {s with PublishMeetingGroupProposedEventCalled = CalledWith ev}
//                                do! State.put ns
//                                return n
//                            }
//                | InR r ->
//                    match r with
//                        | ProposeMeetingGroup.Algebra.LogInfo (_, n) ->
//                            monad {
//                                return n
//                            }
//            Free.fold go p
//
//
//type MeetingNameTests(output: ITestOutputHelper) =
//    
//    [<Fact>]
//    let ``some``() =
//        let mr = MeetingName.create "some name"
//        
//        match mr with
//        | Ok m ->
//            let v = MeetingName.value m
//            v |> should equal "some name"
//        | _ -> mr |> should be (ofCase<@ Ok Unchecked.defaultof<MeetingName> @>)
//        
//    [<Fact>]
//    let ``at most 1024 chars``() =
//        let s = String.replicate 1024 "d"
//        let mr = MeetingName.create s
//        
//        match mr with
//        | Ok m ->
//            let v = MeetingName.value m
//            v |> should equal s
//        | _ -> mr |> should be (ofCase<@ Ok Unchecked.defaultof<MeetingName> @>)
//
//    [<Fact>]
//    let ``null``() =
//        let mr = MeetingName.create null
//        
//        match mr with
//        | Ok _ -> mr |> should be (ofCase<@ Error Unchecked.defaultof<string list> :> Result<MeetingName, string list> @>)
//        | Error er -> er.Length |> should equal 1
//    
//    [<Fact>]
//    let ``empty``() =
//        let mr = MeetingName.create ""
//        
//        match mr with
//        | Ok _ -> mr |> should be (ofCase<@ Error Unchecked.defaultof<string list> :> Result<MeetingName, string list> @>)
//        | Error er -> er.Length |> should equal 1
//        
//    [<Fact>]
//    let ``newline``() =
//        let mr = MeetingName.create @"ad
//fs"
//        
//        match mr with
//        | Ok _ -> mr |> should be (ofCase<@ Error Unchecked.defaultof<string list> :> Result<MeetingName, string list> @>)
//        | Error er -> er.Length |> should equal 1
//        
//    [<Fact>]
//    let ``more than 1024 chars``() =
//        let s = String.replicate 1025 "d"
//        let mr = MeetingName.create s
//        
//        match mr with
//        | Ok _ -> mr |> should be (ofCase<@ Error Unchecked.defaultof<string list> :> Result<MeetingName, string list> @>)
//        | Error er -> er.Length |> should equal 1
//        
//    [<Fact>]
//    let ``combine newline and more than 1024 chars``() =
//        let s = (String.replicate 600 "a" + @"
//" + String.replicate 800 "b")
//        
//        let mr = MeetingName.create s
//        
//        match mr with
//        | Ok _ -> mr |> should be (ofCase<@ Error Unchecked.defaultof<string list> :> Result<MeetingName, string list> @>)
//        | Error er -> er.Length |> should equal 2
//        
//        
//
//type Tests (output: ITestOutputHelper) =
//    
//    [<Property(Arbitrary = [|typeof<MyGens>|])>]
//    let ``tests`` (cmd: ProposeMeetingGroupCommand) =
//        let is = {
//            SaveMeetingGroupProposalCalled = NotCalled
//            PublishMeetingGroupProposedEventCalled = NotCalled
//        }
//        output.WriteLine (sprintf "%O" cmd)
//        if cmd.Name.Length = 0 then
//            failwith cmd.Name
//        let now = DateTime.UtcNow
//        let guid = Guid.NewGuid()
//        let p = ProposeMeetingGroup.Implementation.handler cmd now guid (Guid.NewGuid())
//        let s = interpret p
//        let rs = State.exec s is
//        
//        match rs.SaveMeetingGroupProposalCalled with
//        | CalledWith v ->
//            match v with
//            | InVerificationMeetingGroupProposal details ->
//                details.Id |> should equal (MeetingGroupProposalId guid)
//                details.Name |> should equal cmd.Name
//                details.Description |> should equal cmd.Description
//                details.ProposalDate |> should equal now
//                details.Location.City |> should equal cmd.LocationCity
//                details.Location.CountryCode |> should equal cmd.LocationCountryCode
//            | _ -> v |> should be (ofCase<@ InVerificationMeetingGroupProposal Unchecked.defaultof<MeetingGroupProposalDetails> @>)
//        | _ as x -> x |> should be (ofCase<@ CalledWith Unchecked.defaultof<MeetingGroupProposal> @>)
//            
//        match rs.PublishMeetingGroupProposedEventCalled with
//        | CalledWith v ->
//                v.Id |> should equal (MeetingGroupProposalId guid)
//                v.Name |> should equal cmd.Name
//                v.Description |> should equal cmd.Description
//                v.ProposalDate |> should equal now
//                v.LocationCity |> should equal cmd.LocationCity
//                v.LocationCountryCode |> should equal cmd.LocationCountryCode
//        | _ as x -> x |> should be (ofCase<@ CalledWith Unchecked.defaultof<MeetingGroupProposedDomainEvent> @>)