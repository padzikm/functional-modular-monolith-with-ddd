namespace CompanyName.MyMeetings.Modules.Administration.Service.ProposeMeetingGroup

open System.Threading.Tasks
open CompanyName.MyMeetings.Modules.Meetings.Domain.DomainEvents
open Microsoft.Extensions.Logging
open NServiceBus

type ProposeMeetingGroupHandler(logger: ILogger<ProposeMeetingGroupHandler>) =
    interface IHandleMessages<MeetingGroupProposedDomainEvent> with
        member this.Handle(message, context) =
            logger.LogInformation((sprintf "message received %O" message))
            Task.CompletedTask
