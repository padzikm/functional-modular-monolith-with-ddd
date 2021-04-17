module CompanyName.MyMeetings.Modules.Meetings.Application.Meetings.CreateMeeting

open System
open CompanyName.MyMeetings.Modules.Meetings.Domain.Meetings

type CreateMeetingCommand = {
    MeetingGroupId: Guid
    Title: string
    AttendeesLimit: int option
}

let executeCommand (saveMeeting: Meeting -> Result<_,_>) command =
    let meeting: Meeting = {
        MeetingId = MeetingId(Guid.NewGuid())
        Title = command.Title
        EventFee = Free
    }
    saveMeeting meeting
    
