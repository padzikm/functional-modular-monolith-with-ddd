namespace CompanyName.MyMeetings.Modules.Meetings.Application.GetMember

open System
open FsToolkit.ErrorHandling
open MediatR
open NServiceBus

module Types =
    [<CLIMutable>]
    type GetMemberQueryResult = {
        Id: Guid
        Name: string
        FirstName: string
        LastName: string
        Email: string
        Login: string
    }
    
    [<CLIMutable>]
    type GetMemberQuery =
        {
            Id: Guid
        }
        interface IRequest<Async<Validation<GetMemberQueryResult option, string>>> with    
    