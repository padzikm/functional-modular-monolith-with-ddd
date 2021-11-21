namespace CompanyName.MyMeetings.Modules.Meetings.Domain.Members

open System
open CompanyName.MyMeetings.BuildingBlocks.Domain

module Events =
    type MemberCreatedDomainEvent (id: Guid) =
        inherit DomainEventBase()
        member val MemberId: Guid = id with get
