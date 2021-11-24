namespace CompanyName.MyMeetings.Modules.Meetings.Service.CreateMember

open System.Threading.Tasks
open CompanyName.MyMeetings.Modules.Meetings.Application.CreateMember.Types
open FSharpPlus
open Microsoft.Extensions.Logging
open NServiceBus

type CreateMemberHandler (logger: ILogger<CreateMemberHandler>) =
    interface IHandleMessages<CreateMemberCommand> with
        member this.Handle(message, context) =
            logger.LogInformation "message received"
            logger.LogInformation (sprintf "%A" message)
            logger.LogInformation "message handled"
            Task.CompletedTask
    
    
    
