module CompanyName.MyMeetings.Modules.Meetings.Application.Meetings.GetMeetingDetails

open System
open System.Data.Common
open System.Data.SqlClient
open Dapper
open FSharpPlus
open FsToolkit.ErrorHandling

type MeetingsDetailDto = { Id: Guid; Title: string }

type GetMeetingDetailsQuery = GetMeetingDetailsQuery of id: Guid

let executeQuery connectionString (GetMeetingDetailsQuery id) =
    let conn = new SqlConnection(connectionString)
    let uncheckedDefault = Unchecked.defaultof<MeetingsDetailDto>

    let sql =
        "SELECT "
        + $"[MeetingsDetails].[Id] as [{nameof uncheckedDefault.Id}], "
        + $"[MeetingsDetails].[Title] as [{nameof uncheckedDefault.Title}] "
        + $"FROM [meetings].[v_MeetingDetails] as [MeetingsDetails] WHERE [MeetingsDetails].[Id] = @MeetingId"

    let sqlParams = {| MeetingId = id |}
    
    conn.QuerySingleAsync<MeetingsDetailDto>(sql, sqlParams)
            |> AsyncResult.ofTask
