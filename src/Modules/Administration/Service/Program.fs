namespace CompanyName.MyMeetings.Modules.Administration.Service

open System
open System.Collections.Generic
open System.Data.Common
open System.Linq
open System.Threading.Tasks
open Microsoft.Data.SqlClient
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open NServiceBus
open Microsoft.EntityFrameworkCore

module Program =
    let createHostBuilder args =
        Host.CreateDefaultBuilder(args)
            .UseNServiceBus(fun ctx ->
                let c = EndpointConfiguration("Administration")
                c.EnableInstallers()
                c.EnableOutbox() |> ignore
                let t = c.UseTransport<RabbitMQTransport>()
                t.UseConventionalRoutingTopology() |> ignore
                t.ConnectionString("host=localhost;port=5672;username=guest;password=guest") |> ignore
                let p = c.UsePersistence<SqlPersistence>()
                let d = p.SqlDialect<SqlDialect.MsSqlServer>()  
//                d.Schema("administration_bus")
                p.ConnectionBuilder(fun () ->
                    let con = SqlConnection("Server=localhost;Database=MyMeetings;User Id=sa;Password=SqlServer2019;")
                    con :> DbConnection
                    )
                c)
            .ConfigureServices(fun hostContext services ->
//                services.AddDbContext(fun b ->
//                    b.UseSqlServer() |> ignore) |> ignore
                    ()
                  )

    [<EntryPoint>]
    let main args =
        createHostBuilder(args).Build().Run()

        0 // exit code