module CompanyName.MyMeetings.Modules.Meetings.Application.Meetings.EditMeeting

open System
open CompanyName.MyMeetings.Modules.Meetings.Domain.Meetings
open FsToolkit.ErrorHandling

type EditMeetingCommand =
    { Id: Guid
      EventFeeValue: decimal option
      EventFeeCurrency: string option }

let executeCommand (getById: Guid -> Async<Result<_,_>>) (update: Meeting -> Async<Result<_,_>>) (meetingId: Guid) (command: EditMeetingCommand) : Async<Result<_, _>> =
    asyncResult {
        let! meeting = getById meetingId

        let updatedMeeting =
            match (command.EventFeeValue, command.EventFeeCurrency) with
            | (Some v, Some c) ->
                { meeting with
                      EventFee = Paid({ Value = v; Currency = c }) }
            | (None, None) -> { meeting with EventFee = Free }
            | _ -> meeting

        return! update updatedMeeting
    }
