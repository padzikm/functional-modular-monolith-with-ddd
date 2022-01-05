namespace CompanyName.MyMeetings.Modules.Meetings.Service

open System
open System.Collections.Generic
open System.Data.Common
open System.Linq
open System.Threading.Tasks
open CompanyName.MyMeetings.Modules.Meetings.Infrastructure
open Microsoft.Data.SqlClient
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open NServiceBus
open NServiceBus.Persistence.Sql
open Microsoft.EntityFrameworkCore

module Program =
    let createHostBuilder args =
        Host.CreateDefaultBuilder(args)
            .UseNServiceBus(fun ctx ->
                let endpoint = EndpointConfiguration("Meetings")
                endpoint.EnableInstallers()
                let t = endpoint.UseTransport<RabbitMQTransport>()
                t.UseConventionalRoutingTopology() |> ignore
                t.ConnectionString("host=localhost;port=5672;username=guest;password=guest") |> ignore
                let p = endpoint.UsePersistence<SqlPersistence>()
                let d = p.SqlDialect<SqlDialect.MsSqlServer>()
                d.Schema("meetings_bus")
                p.ConnectionBuilder(fun () ->
                    let con = SqlConnection("Server=localhost;Database=MyMeetings;User Id=sa;Password=SqlServer2019;")
                    con :> DbConnection
                    )
                endpoint
                )
            .ConfigureServices(fun ctx services ->
                services.AddDbContext<MeetingsDbContext>(fun opt ->                    
                    opt.UseSqlServer("Server=localhost;Database=MyMeetings;User Id=sa;Password=SqlServer2019;") |> ignore             
                    ) |> ignore      
                )
//            .ConfigureServices(fun hostContext services ->
//                services.AddHostedService<Worker>() |> ignore)

    [<EntryPoint>]
    let main args =
        createHostBuilder(args).Build().Run()

        0 // exit code