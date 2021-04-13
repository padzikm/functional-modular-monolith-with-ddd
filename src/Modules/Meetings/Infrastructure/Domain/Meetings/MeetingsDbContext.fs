module CompanyName.MyMeetings.Modules.Meetings.Infrastructure.Domain.Meetings.MeetingsDbContext

open System
open System.Data.SqlClient
open CompanyName.MyMeetings.Modules.Meetings.Domain.Meetings
open Microsoft.EntityFrameworkCore
open Dapper
open CompanyName.MyMeetings.Modules.Meetings.Infrastructure.Domain.Meetings.MeetingsDbModel

type MeetingsDbContext(connectionString: string) =
    inherit DbContext()
    
    [<DefaultValue>]
    val mutable meetingsDbSet: DbSet<DbMeeting>
    
    member public this.DbMeetings
        with get () = this.meetingsDbSet
        and set (v) = this.meetingsDbSet <- v
        
    
    override this.OnConfiguring (optionsBuilder: DbContextOptionsBuilder) =
        optionsBuilder.UseSqlServer(connectionString) |> ignore
    
    override this.OnModelCreating builder =
        builder.ApplyConfigurationsFromAssembly(this.GetType().Assembly) |> ignore
    