namespace CompanyName.MyMeetings.Modules.Meetings.Domain

open System
open CompanyName.MyMeetings.Modules.Meetings.Domain.SimpleTypes

type Member = {
    MemberId: Guid
    Login: string
    FirstName: string
    LastName: string
    Name: string
    Email: string
    CreatedDate: DateTime
}

type MeetingGroupLocation = {
    City: MeetingLocationCity
    Postcode: MeetingLocationPostcode
}

type MeetingGroupProposalId = MeetingGroupProposalId of Guid

type MeetingGroupProposalDetails = {
    Id: MeetingGroupProposalId
    Name: MeetingName
    Description: string option
    ProposalMemberId: Guid
    ProposalDate: DateTime
    Location: MeetingGroupLocation
}

type MeetingGroupProposal =
    | AcceptedMeetingGroupProposal of MeetingGroupProposalDetails
    | InVerificationMeetingGroupProposal of MeetingGroupProposalDetails

type MemberJoinMeetingGroupDetails = {
    MeetingGroupId: Guid
    MemberId: Guid
    JoinedDate: DateTime
}

type MemberLeaveMeetingGroupDetails = {
    MeetingGroupId: Guid
    MemberId: Guid
    LeaveDate: DateTime
}

type MeetingGroupMember =
    | ActiveMeetingGroupMember of MemberJoinMeetingGroupDetails
    | InactiveMeetingGroupMember of MemberJoinMeetingGroupDetails * MemberLeaveMeetingGroupDetails


type MeetingGroup = {
    Id: Guid
    Name: string
    Description: string
    Members: MeetingGroupMember list
    CreatedDate: DateTime
    PaymentDateTo: DateTime option
}
