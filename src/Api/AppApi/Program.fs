namespace CompanyName.MyMeetings.Api

open System
open System.Collections.Generic
open System.Data.Common
open System.IO
open System.Linq
open System.Threading.Tasks
open CompanyName.MyMeetings.Modules.Meetings.Application.CreateMember.Types
open CompanyName.MyMeetings.Modules.Meetings.Application.ProposeMeetingGroup.Types
open Microsoft.AspNetCore
open Microsoft.AspNetCore.Hosting
open Microsoft.Data.SqlClient
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
                endpoint.EnableOutbox() |> ignore
                let t = endpoint.UseTransport<RabbitMQTransport>()
                t.UseConventionalRoutingTopology() |> ignore
                t.ConnectionString("host=localhost;port=5672;username=guest;password=guest") |> ignore
//                endpoint.UsePersistence<LearningPersistence>() |> ignore
                let p = endpoint.UsePersistence<SqlPersistence>()
                let d = p.SqlDialect<SqlDialect.MsSqlServer>()  
//                d.Schema("administration_bus")
                p.ConnectionBuilder(fun () ->
                    let con = SqlConnection("Server=localhost;Database=MyMeetings;User Id=sa;Password=SqlServer2019;")
                    con :> DbConnection
                    )
                let routing = t.Routing()
                //let t = typeof<CreateMemberCommand>
                routing.RouteToEndpoint(typeof<CreateMemberCommand>.Assembly, "Meetings") |> ignore
//                routing.RouteToEndpoint(typeof<ProposeMeetingGroupCommand>, "Meetings") |> ignore
                endpoint
                )
            .ConfigureWebHostDefaults(fun webBuilder ->
                webBuilder.UseStartup<Startup>() |> ignore
            )

    [<EntryPoint>]
    let main args =
        CreateHostBuilder(args).Build().Run()

        exitCode
