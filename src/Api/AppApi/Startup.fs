namespace CompanyName.MyMeetings.Api

open System
open System.Collections.Generic
open System.Linq
open System.Text.Json.Serialization
open System.Threading.Tasks
open CompanyName.MyMeetings.Api.Controllers.ExceptionFilter
open CompanyName.MyMeetings.Modules.Meetings.Infrastructure
open CompanyName.MyMeetings.Modules.Meetings.Interpreters.CreateMember
open CompanyName.MyMeetings.Modules.Meetings.Interpreters.GetMember
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.HttpsPolicy;
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.EntityFrameworkCore
open MediatR
open Hellang.Middleware.ProblemDetails

type Startup(configuration: IConfiguration) =
    member _.Configuration = configuration

    // This method gets called by the runtime. Use this method to add services to the container.
    member _.ConfigureServices(services: IServiceCollection) =
        services.AddCors() |> ignore
        services.AddControllers(fun opt ->
            opt.Filters.Add<ExFilter>() |> ignore
            ).AddJsonOptions(fun options -> options.JsonSerializerOptions.Converters.Add(JsonFSharpConverter())) |> ignore
        services.AddDbContext<MeetingsDbContext>(fun opt ->                    
                    opt.UseSqlServer("Server=localhost;Database=MyMeetings;User Id=sa;Password=SqlServer2019;") |> ignore             
                    ) |> ignore
        services.AddMediatR(typeof<CreateMemberHandler>) |> ignore
        services.AddProblemDetails() |> ignore

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    member _.Configure(app: IApplicationBuilder, env: IWebHostEnvironment) =
//        if (env.IsDevelopment()) then
//            app.UseDeveloperExceptionPage() |> ignore 
        app.UseProblemDetails() |> ignore
        app.Use(fun ctx next ->
                
                    
                next.Invoke() 
                    
                ) |> ignore
        //app.UseHttpsRedirection()
        app.UseCors(fun a -> a.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin() |> ignore) |> ignore
        app.UseRouting()
           .UseAuthorization()
           .UseEndpoints(fun endpoints ->
                endpoints.MapControllers() |> ignore
            ) |> ignore
