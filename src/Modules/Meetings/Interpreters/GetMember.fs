namespace CompanyName.MyMeetings.Modules.Meetings.Interpreters.GetMember

open CompanyName.MyMeetings.Modules.Meetings.Application.GetMember.Types
open CompanyName.MyMeetings.Modules.Meetings.Infrastructure
open Microsoft.Extensions.Logging
open MediatR
open Microsoft.EntityFrameworkCore
open FsToolkit.ErrorHandling

type GetMemberHandler (logger: ILogger<GetMemberHandler>, dbContext: MeetingsDbContext) =
    inherit RequestHandler<GetMemberQuery, Async<Validation<GetMemberQueryResult, string>>>()
        override this.Handle(request) =
            async {
                logger.LogInformation (sprintf "inside handler %A" request)
                let! res = dbContext.Members.FirstAsync() |> Async.AwaitTask
                logger.LogInformation (sprintf "got result %A" res)
                let qr: GetMemberQueryResult = {
                    Id = res.Id
                    FirstName = res.FirstName
                    LastName = res.LastName
                    Name = res.Name
                    Email = res.Email
                    Login = res.Login                    
                }
                return Validation.ofResult (Ok qr)
            }