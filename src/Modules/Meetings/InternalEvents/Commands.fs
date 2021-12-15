namespace CompanyName.MyMeetings.Modules.Meetings.InternalEvents.Commands

open CompanyName.MyMeetings.Modules.Meetings.InternalEvents.Errors
open MediatR
open NServiceBus
open FsToolkit.ErrorHandling


[<CLIMutable>]
    type ProposeMeetingGroupCommand =
        {
        Name: string
        Description: string
        LocationCity: string
        LocationCountryCode: string
        }
        interface ICommand with
        interface IRequest<Async<Validation<unit,Error>>> with