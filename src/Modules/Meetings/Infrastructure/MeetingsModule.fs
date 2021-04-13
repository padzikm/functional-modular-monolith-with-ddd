module CompanyName.MyMeetings.Modules.Meetings.Infrastructure.MeetingsModule

open CompanyName.MyMeetings.Modules.Meetings.Application.Meetings
open CompanyName.MyMeetings.Modules.Meetings.Application.Meetings.CreateMeeting
open CompanyName.MyMeetings.Modules.Meetings.Application.Meetings.EditMeeting
open CompanyName.MyMeetings.Modules.Meetings.Application.Meetings.GetMeetingDetails
open CompanyName.MyMeetings.Modules.Meetings.Infrastructure.Domain.Meetings
open CompanyName.MyMeetings.Modules.Meetings.Infrastructure.Domain.Meetings.MeetingsDbContext
open CompanyName.MyMeetings.Modules.Meetings.Infrastructure.Domain.Meetings.MeetingsRepository
open System.Linq
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.AsyncResult
open Microsoft.EntityFrameworkCore

let saveChanges (dbContext: MeetingsDbContext) _ = dbContext.SaveChangesAsync() |> AsyncResult.ofTaskAction

let createMeeting connectionString command =
    let dbContext = new MeetingsDbContext(connectionString)
    let saveMeeting = dbContext |> save 
    CreateMeeting.executeCommand saveMeeting command
    >>= saveChanges dbContext
    
let editMeeting connectionString (command: EditMeetingCommand) =
    let dbContext = new MeetingsDbContext(connectionString)
    let getMeeting = dbContext |> getById
    let updateMeeting = dbContext |> update
    EditMeeting.executeCommand getMeeting updateMeeting command.Id command
    >>= saveChanges dbContext

let getMeetingDetails connectionString query =
    GetMeetingDetails.executeQuery connectionString query