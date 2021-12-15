namespace CompanyName.MyMeetings.Modules.Meetings.Domain

open System

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
    City: string
    CountryCode: string
}

type MeetingGroupProposalId = MeetingGroupProposalId of Guid

type MeetingGroupProposalDetails = {
    Id: MeetingGroupProposalId
    Name: string
    Description: string
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
