namespace CompanyName.MyMeetings.Api

open System
open System.Collections.Generic
open System.IO
open System.Linq
open System.Threading.Tasks
open CompanyName.MyMeetings.Modules.Meetings.Application.CreateMember.Types
open Microsoft.AspNetCore
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open NServiceBus

module Program =
    let exitCode = 0

    let CreateHostBuilder args =
        Host.CreateDefaultBuilder(args)
            .UseNServiceBus(fun ctx ->
                let endpoint = EndpointConfiguration("Api")
                endpoint.SendOnly()
                let transport = endpoint.UseTransport<LearningTransport>()
                endpoint.UsePersistence<LearningPersistence>() |> ignore
                let routing = transport.Routing()
                //let t = typeof<CreateMemberCommand>
                routing.RouteToEndpoint(typeof<CreateMemberCommand>, "Meetings") |> ignore
                endpoint
                )
            .ConfigureWebHostDefaults(fun webBuilder ->
                webBuilder.UseStartup<Startup>() |> ignore
            )

    [<EntryPoint>]
    let main args =
        CreateHostBuilder(args).Build().Run()

        exitCode
