namespace CompanyName.MyMeetings.Modules.Meetings.Infrastructure

open CompanyName.MyMeetings.Modules.Meetings.Infrastructure.Domain
open Microsoft.EntityFrameworkCore

type MeetingsDbContext(connectionString: string) =
    inherit DbContext()
    
    [<DefaultValue>]
    val mutable membersDbSet: DbSet<Member>
    
    member public this.Members
        with get () = this.membersDbSet
        and set (v) = this.membersDbSet <- v
    
    override this.OnConfiguring (optionsBuilder: DbContextOptionsBuilder) =
        optionsBuilder.UseSqlServer(connectionString) |> ignore
    
    override this.OnModelCreating builder =
        builder.ApplyConfigurationsFromAssembly(this.GetType().Assembly) |> ignore

