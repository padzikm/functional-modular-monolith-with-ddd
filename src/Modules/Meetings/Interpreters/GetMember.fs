namespace CompanyName.MyMeetings.Modules.Meetings.Interpreters.GetMember

open CompanyName.MyMeetings.Modules.Meetings.Application.GetMember.Types
open CompanyName.MyMeetings.Modules.Meetings.Infrastructure
open Microsoft.Extensions.Logging
open MediatR
open Microsoft.EntityFrameworkCore
open FsToolkit.ErrorHandling
open FSharpPlus
open FsToolkit.ErrorHandling

type GetMemberHandler (logger: ILogger<GetMemberHandler>, dbContext: MeetingsDbContext) =
    inherit RequestHandler<GetMemberQuery, Async<Validation<GetMemberQueryResult, string>>>()
        override this.Handle(request) =
            //logger.LogInformation (sprintf "inside handler %A" request)
            let r = AsyncResult.ofTask (dbContext.Members.FirstAsync(fun m -> m.Id = request.Id))
            let u = r |> AsyncResult.map (fun m ->
                    let qr: GetMemberQueryResult = {
                        Id = m.Id
                        FirstName = m.FirstName
                        LastName = m.LastName
                        Name = m.Name
                        Email = m.Email
                        Login = m.Login                    
                        }
                    qr
                    ) |> AsyncResult.mapError (fun e -> [e.ToString()])
            u
//            async {
//                logger.LogInformation (sprintf "inside handler %A" request)
//                //Result.protect
//                try
//                    let! res = dbContext.Members.FirstAsync(fun m -> m.Id = request.Id) |> Async.AwaitTask
//                    logger.LogInformation (sprintf "got result %A" res)
//                    let qr: GetMemberQueryResult = {
//                        Id = res.Id
//                        FirstName = res.FirstName
//                        LastName = res.LastName
//                        Name = res.Name
//                        Email = res.Email
//                        Login = res.Login                    
//                    }
//                    return Validation.ok qr
//                with
//                | ex -> return Validation.error (ex.ToString())
//            }