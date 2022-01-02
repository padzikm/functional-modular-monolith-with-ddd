namespace CompanyName.MyMeetings.Api.Controllers.ExceptionFilter

//open CompanyName.MyMeetings.Modules.Meetings.InternalEvents.Errors
open CompanyName.MyMeetings.Api.Controllers.MeetingGroupProposalController
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Mvc.Filters

type ExFilter () =
    interface IActionFilter with
        member this.OnActionExecuted(context) =
            printfn "wykonuje sie filter!"
            let r = context.Result
            match r with
            | :? InfrastructureErrorResult as br ->
                printfn "bad request to jest"
                printfn "%O" br.Value
                match br.Value with
                | :? exn as er ->
                    printfn "error to jest"
                    context.Exception <- er
                    context.ExceptionHandled <- false
                | _ -> ()
            | _ -> ()
                
        member this.OnActionExecuting(context) = () 
        
    
