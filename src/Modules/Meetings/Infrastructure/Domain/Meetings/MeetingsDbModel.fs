module CompanyName.MyMeetings.Modules.Meetings.Infrastructure.Domain.Meetings.MeetingsDbModel

open System

[<CLIMutable>]
type DbMeeting =
    { mutable Id: Guid
      mutable Title: string
      mutable EventFeeValue: Nullable<decimal>
      mutable EventFeeCurrency: string }
