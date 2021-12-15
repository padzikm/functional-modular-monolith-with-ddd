namespace CompanyName.MyMeetings.Modules.Meetings.InternalEvents.Events

open System

type MeetingGroupProposedDomainEvent = {
    Id: Guid
    Name: string
    Description: string
    ProposalUserId: Guid
    ProposalDate: DateTime
    LocationCity: string
    LocationCountryCode: string
}