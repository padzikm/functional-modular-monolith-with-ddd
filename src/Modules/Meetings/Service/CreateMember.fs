namespace CompanyName.MyMeetings.Modules.Meetings.Service.CreateMember

open System
open System.Threading.Tasks
open CompanyName.MyMeetings.Modules.Meetings.Application.CreateMember.Types
open CompanyName.MyMeetings.Modules.Meetings.Application.CreateMember.Implementation
open CompanyName.MyMeetings.Modules.Meetings.Application.CreateMember.Algebra
open FSharpPlus
open FSharpPlus.Data
open Microsoft.Extensions.Logging
open NServiceBus

type CreateMemberHandler (logger: ILogger<CreateMemberHandler>) =
    let rec interpret (p: Program<_>) =
        let go v =
            match v with
            | InL l ->
                match l with
                | InL dbi ->
                    match dbi with
                    | SaveMember (m, i) ->
                        logger.LogInformation (sprintf "save member %A" m)
                        async {
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
            
    
    
    
