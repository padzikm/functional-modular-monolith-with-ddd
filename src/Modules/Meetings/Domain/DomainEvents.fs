namespace CompanyName.MyMeetings.Modules.Meetings.Domain.DomainEvents

open System
open CompanyName.MyMeetings.Modules.Meetings.Domain
open CompanyName.MyMeetings.Modules.Meetings.Domain.SimpleTypes
open NServiceBus

type MeetingGroupProposedDomainEventInternal =
    {
    Id: MeetingGroupProposalId
    Name: MeetingName
    Description: string option
    ProposalUserId: Guid
    ProposalDate: DateTime
    LocationCity: MeetingLocationCity
    LocationPostcode: MeetingLocationPostcode
    }
 
[<CLIMutable>]
type MeetingGroupProposedDomainEvent =
    {
    Id: Guid
    Name: string
    Description: string
    ProposalUserId: Guid
    ProposalDate: DateTime
    LocationCity: string
    LocationPostcode: string
    }
    interface IEvent with
   
    