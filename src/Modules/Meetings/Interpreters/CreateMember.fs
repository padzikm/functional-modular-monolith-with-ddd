namespace CompanyName.MyMeetings.Modules.Meetings.Interpreters.CreateMember

open System
open System.Threading.Tasks
open CompanyName.MyMeetings.Modules.Meetings.Application.CreateMember.Types
open CompanyName.MyMeetings.Modules.Meetings.Application.CreateMember.Implementation
open CompanyName.MyMeetings.Modules.Meetings.Application.CreateMember.Algebra
open CompanyName.MyMeetings.Modules.Meetings.Infrastructure
open CompanyName.MyMeetings.Modules.Meetings.Infrastructure.Database
open FSharpPlus
open FSharpPlus.Data
open Microsoft.Extensions.Logging
open NServiceBus

type CreateMemberHandler (logger: ILogger<CreateMemberHandler>, dbContext: MeetingsDbContext) =
    let rec interpret (p: Program<_>) =
        let go v =
            match v with
            | InL l ->
                match l with
                | InL dbi ->
                    match dbi with
                    | SaveMember (m, i) ->
                        logger.LogInformation (sprintf "save member %A" m)
                        let memb = Member(Id = m.MemberId, Name = m.Name, FirstName = m.FirstName, LastName = m.LastName,
                                              Login = m.Login, Email = m.Email, CreatedDate = m.CreatedDate)                            
                        async {
                            dbContext.Members.Add(memb) |> ignore
                            let! _ = dbContext.SaveChangesAsync() |> Async.AwaitTask
                            return i
                        }
                | InR evi ->
                    match evi with
                    | PublishMemberCreatedEvent (ev, i) ->
                        logger.LogInformation (sprintf "publish member created event %A" ev)
                        async {
                            return i
                        }
            | InR r ->
                match r with
                | LogInfo (s, i) ->
                    logger.LogInformation (sprintf "log information %A" s)
                    async {
                        return i
                    }
        Free.fold go p
        
    interface IHandleMessages<CreateMemberCommand> with
        member this.Handle(message, context) =
            async {
                logger.LogInformation "message received"
                logger.LogInformation (sprintf "%A" message)
                let program = handler message DateTime.UtcNow
                let! result = interpret program
                logger.LogInformation (sprintf "interpret result %A" result)
                logger.LogInformation "message handled"
            } |> Async.StartAsTask :> Task
            
    
    
    
