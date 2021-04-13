module CompanyName.MyMeetings.Modules.Meetings.Domain.Meetings

open System

type MeetingId = MeetingId of Guid

type Money = {
    Value: decimal
    Currency: string
}

type EventFee =
    | Free
    | Paid of Money
    

type Meeting = {
    MeetingId: MeetingId
    Title: string
    EventFee: EventFee
}