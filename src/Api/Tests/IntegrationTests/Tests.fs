module Tests

open System
open System.Net
open System.Net.Http
open System.Net.Http.Json
open System.Text
open System.Text.Json
open System.Text.Json.Serialization
open CompanyName.MyMeetings.Api
open CompanyName.MyMeetings.Api.Controllers.MeetingGroupProposalController
open CompanyName.MyMeetings.Api.Controllers.MeetingsController
open CompanyName.MyMeetings.Modules.Meetings.Application.Meetings.CreateMeeting
open CompanyName.MyMeetings.Modules.Meetings.Application.Meetings.EditMeeting
open Microsoft.AspNetCore.Http.Extensions
open Microsoft.AspNetCore.Mvc.Testing
open Microsoft.AspNetCore.TestHost
open Microsoft.AspNetCore.WebUtilities
open NServiceBus
open NServiceBus.Testing
open Microsoft.Extensions.DependencyInjection
open Xunit
open FsUnit.Xunit
open Xunit.Abstractions

let options = JsonSerializerOptions()
options.Converters.Add(JsonFSharpConverter())

let serverFactory = new WebApplicationFactory<Startup>()

type Tests (output: ITestOutputHelper) =
    
    [<Fact>]
    let ``propose meeting group`` () =
        let s = TestableMessageSession()
        let client = serverFactory.WithWebHostBuilder(fun b ->
            b.ConfigureTestServices(fun sc ->
                sc.AddScoped<IMessageSession>(fun _ -> s :> IMessageSession) |> ignore) |> ignore).CreateClient()
        
        let req: ProposeMeetingGroupRequest = {
            Name = "agf"
            Description = "asfgads asfd adsf"
            LocationCity = "tgwojw ffe"
            LocationCountryCode = "kjfd"
        }
        async {
            let! r = client.PostAsJsonAsync("/api/meetings/meetinggroupproposal/", req) |> Async.AwaitTask 
            output.WriteLine (sprintf "%O" r)
            r.IsSuccessStatusCode |> should equal true
            s.SentMessages.Length |> should equal 1
        }
    
    [<Fact>]
    let ``sent msg`` () =
        let session = TestableMessageSession()
        let client = serverFactory.WithWebHostBuilder(fun b ->
            b.ConfigureTestServices(fun s ->               
                s.AddScoped<IMessageSession>(fun p -> session :> IMessageSession) |> ignore ) |> ignore).CreateClient()

        async {
            let qq: M = {
                Login = "asdfasdfasdf"
                Name = "asdfadf"
            }
            let uri = $"/meetings/do"
            let d = Map(seq {
                yield (nameof qq.Login, qq.Login)
                yield (nameof qq.Name, qq.Name)
            })
            let r = QueryHelpers.AddQueryString(uri, d)
            output.WriteLine(r)
     
            let! value = client.GetAsync(r) |> Async.AwaitTask
            let! res = value.Content.ReadAsStringAsync() |> Async.AwaitTask
            output.WriteLine(res)
            session.SentMessages.Length |> should equal 1
            value.IsSuccessStatusCode |> should be True
        }
    
    [<Fact>]
    let ``get nonexistent meeting returns not found`` () =
        let client = serverFactory.CreateClient()
        async {
            let g = Guid.NewGuid()
            let uri = $"/meetings/{g}"
            let! value = client.GetAsync(uri) |> Async.AwaitTask
            let! res = value.Content.ReadAsStringAsync() |> Async.AwaitTask
            output.WriteLine(res)
            value.StatusCode |> should equal HttpStatusCode.NotFound
        }
        
    [<Fact>]
    let ``create new meeting`` () =
        let client = serverFactory.CreateClient()
        async {
            let data = {|
                MeetingGroupId = Guid.NewGuid()
                Title = "test"
            |}
            let uri = $"/meetings"
            let json = JsonContent.Create(data)
            let! s = json.ReadAsStringAsync() |> Async.AwaitTask
            output.WriteLine(s)
            let! res = client.PostAsync(uri, json) |> Async.AwaitTask
            let! out = res.Content.ReadAsStringAsync() |> Async.AwaitTask
            output.WriteLine(out)
            res.IsSuccessStatusCode |> should equal true
        }
        
    [<Fact>]
    let ``get existing meeting returns ok`` () =
        let client = serverFactory.CreateClient()
        async {
            let g = Guid.Parse("EA0231DF-12A7-475C-A1B1-53F9EF0297B7")
            let uri = $"/meetings/{g}"
            let! value = client.GetAsync(uri) |> Async.AwaitTask
            let! res = value.Content.ReadAsStringAsync() |> Async.AwaitTask
            output.WriteLine(res)
            value.IsSuccessStatusCode |> should equal true
        }

    [<Fact>]
    let ``edit meeting to be paid`` () =
        let client = serverFactory.CreateClient()
        async {
            let id = Guid.Parse("EA0231DF-12A7-475C-A1B1-53F9EF0297B7")
            let data: EditMeetingRequest = {
                EventFeeValue = Some(10m)
                EventFeeCurrency = Some("usd")
            }
            let d = JsonSerializer.Serialize(data, options)
            let dd = JsonSerializer.Deserialize<EditMeetingRequest>(d, options)
            output.WriteLine(d)
            let uri = $"/meetings/{id.ToString()}"
            let str = new StringContent(d, Encoding.UTF8, "application/json")
            let! s = str.ReadAsStringAsync() |> Async.AwaitTask
            output.WriteLine(s)
            let! res = client.PutAsync(uri, str) |> Async.AwaitTask
            let! out = res.Content.ReadAsStringAsync() |> Async.AwaitTask
            output.WriteLine(out)
            res.IsSuccessStatusCode |> should equal true
        }
        
    [<Fact>]
    let ``edit meeting to be free`` () =
        let client = serverFactory.CreateClient()
        async {
            let id = Guid.Parse("EA0231DF-12A7-475C-A1B1-53F9EF0297B7")
            let data: EditMeetingRequest = {
                EventFeeValue = None
                EventFeeCurrency = None
            }
            let d = JsonSerializer.Serialize(data, options)
            let dd = JsonSerializer.Deserialize<EditMeetingRequest>(d, options)
            output.WriteLine(d)
            let uri = $"/meetings/{id.ToString()}"
            let str = new StringContent(d, Encoding.UTF8, "application/json")
            let! s = str.ReadAsStringAsync() |> Async.AwaitTask
            output.WriteLine(s)
            let! res = client.PutAsync(uri, str) |> Async.AwaitTask
            let! out = res.Content.ReadAsStringAsync() |> Async.AwaitTask
            output.WriteLine(out)
            res.IsSuccessStatusCode |> should equal true
        }