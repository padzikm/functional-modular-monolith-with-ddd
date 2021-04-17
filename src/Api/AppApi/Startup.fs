namespace CompanyName.MyMeetings.Api

open System
open System.Collections.Generic
open System.Linq
open System.Text.Json.Serialization
open System.Threading.Tasks
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.HttpsPolicy;
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting


type Startup(configuration: IConfiguration) =
    member _.Configuration = configuration

    // This method gets called by the runtime. Use this method to add services to the container.
    member _.ConfigureServices(services: IServiceCollection) =
        services.AddCors() |> ignore
        services.AddControllers().AddJsonOptions(fun options -> options.JsonSerializerOptions.Converters.Add(JsonFSharpConverter())) |> ignore

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    member _.Configure(app: IApplicationBuilder, env: IWebHostEnvironment) =
        if (env.IsDevelopment()) then
            app.UseDeveloperExceptionPage() |> ignore
        //app.UseHttpsRedirection()
        app.UseCors(fun a -> a.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin() |> ignore) |> ignore
        app.UseRouting()
           .UseAuthorization()
           .UseEndpoints(fun endpoints ->
                endpoints.MapControllers() |> ignore
            ) |> ignore
