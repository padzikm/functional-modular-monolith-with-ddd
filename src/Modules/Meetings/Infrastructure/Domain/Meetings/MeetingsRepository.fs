module CompanyName.MyMeetings.Modules.Meetings.Infrastructure.Domain.Meetings.MeetingsRepository

open System
open System.Data.SqlClient
open CompanyName.MyMeetings.Modules.Meetings.Domain
open CompanyName.MyMeetings.Modules.Meetings.Domain.Meetings
open CompanyName.MyMeetings.Modules.Meetings.Infrastructure.Domain.Meetings.MeetingsDbModel
open FSharpPlus
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.AsyncResult
open Microsoft.EntityFrameworkCore
open Dapper
open CompanyName.MyMeetings.Modules.Meetings.Infrastructure.Domain.Meetings.MeetingsDbContext

let getById (dbContext: MeetingsDbContext) (id: Guid) =
        let q = dbContext.DbMeetings.FindAsync(id).AsTask() |> AsyncResult.ofTask

        let m res =
            { MeetingId = MeetingId(res.Id)
              Title = res.Title
              EventFee =
                  if res.EventFeeValue.HasValue = false then
                      Free
                  else
                      Paid(
                          { Value = res.EventFeeValue.Value
                            Currency = res.EventFeeCurrency }
                      ) }
        m <!> q

let save (dbContext: MeetingsDbContext) (meeting: Meeting) =
        let (MeetingId id) = meeting.MeetingId

        let eventFeeValue, eventFeeCurrency =
            match meeting.EventFee with
            | Free -> (Nullable(), null)
            | Paid ({ Value = v; Currency = c }) -> (Nullable(v), c)

        let dbMeeting : DbMeeting =
            { Id = id
              Title = meeting.Title
              EventFeeValue = eventFeeValue
              EventFeeCurrency = eventFeeCurrency }

        dbContext.DbMeetings.AddAsync(dbMeeting).AsTask() |> AsyncResult.ofTask
        

let update (dbContext: MeetingsDbContext) (meeting: Meeting) =
    asyncResult {
        let (MeetingId id) = meeting.MeetingId
        let dbMeeting = dbContext.meetingsDbSet.Find(id)
        
        let eventFeeValue, eventFeeCurrency =
            match meeting.EventFee with
            | Free -> (Nullable(), null)
            | Paid ({ Value = v; Currency = c }) -> (Nullable(v), c)

        dbMeeting.Title <- meeting.Title
        dbMeeting.EventFeeValue <- eventFeeValue
        dbMeeting.EventFeeCurrency <- eventFeeCurrency
        return ()
    }
