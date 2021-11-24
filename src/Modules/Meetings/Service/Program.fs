namespace CompanyName.MyMeetings.Modules.Meetings.Service

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open NServiceBus

module Program =
    let createHostBuilder args =
        Host.CreateDefaultBuilder(args)
            .UseNServiceBus(fun ctx ->
                let endpoint = EndpointConfiguration("Meetings")
                endpoint.UseTransport<LearningTransport>() |> ignore
                endpoint.UsePersistence<LearningPersistence>() |> ignore
                endpoint
                )
//            .ConfigureServices(fun hostContext services ->
//                services.AddHostedService<Worker>() |> ignore)

    [<EntryPoint>]
    let main args =
        createHostBuilder(args).Build().Run()

        0 // exit code