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
