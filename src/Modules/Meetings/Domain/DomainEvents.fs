namespace CompanyName.MyMeetings.Modules.Meetings.Domain.DomainEvents

open System
open CompanyName.MyMeetings.Modules.Meetings.Domain

type MeetingGroupProposedDomainEvent = {
    Id: MeetingGroupProposalId
    Name: string
    Description: string
    ProposalUserId: Guid
    ProposalDate: DateTime
    LocationCity: string
    LocationCountryCode: string
}
    