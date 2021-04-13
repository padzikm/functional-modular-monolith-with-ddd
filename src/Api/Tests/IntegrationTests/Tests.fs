module Tests

open System
open System.Net
open System.Net.Http
open System.Net.Http.Json
open System.Text
open System.Text.Json
open System.Text.Json.Serialization
open CompanyName.MyMeetings.Api
open CompanyName.MyMeetings.Api.Controllers.MeetingsController
open CompanyName.MyMeetings.Modules.Meetings.Application.Meetings.CreateMeeting
open CompanyName.MyMeetings.Modules.Meetings.Application.Meetings.EditMeeting
open Microsoft.AspNetCore.Mvc.Testing
open Xunit
open FsUnit.Xunit
open Xunit.Abstractions

let options = JsonSerializerOptions()
options.Converters.Add(JsonFSharpConverter())

[<Fact>]
let ``My test`` () =
    Assert.True(true)
    
[<Fact>]
let ``Fsunit test`` () =
    1 |> should equal 1

let serverFactory = new WebApplicationFactory<Startup>()

type Tmp = {
    raz: int
    dwa: string
    trzy: ResizeArray<bool>
}

type Ta = {
    raz: int
    trzy: ResizeArray<bool>
}

type C = {
    id: Guid
}

type Bla (output: ITestOutputHelper) =
    
    [<Fact>]
    let ``get nonexistent meeting returns not found`` () =
        (*let t = {
            raz = 5
            dwa = "testowy"
            trzy = new ResizeArray<bool>([
                false
                true
            ])
        }
        let s = JsonSerializer.Serialize(t)
        output.WriteLine(s)
        let d = JsonSerializer.Deserialize<Ta>(s)
        output.WriteLine(d.raz.ToString())
        output.WriteLine(d.trzy.[0].ToString())
        output.WriteLine(d.trzy.[1].ToString())*)
        let client = serverFactory.CreateClient()
        async {
            let g = Guid.NewGuid()
            let uri = $"/meetings/{g}"
            let! value = client.GetAsync(uri) |> Async.AwaitTask
            let! res = value.Content.ReadAsStringAsync() |> Async.AwaitTask
            output.WriteLine(res)
            //let dd = JsonSerializer.Deserialize<C>(res)
            //output.WriteLine(dd.id.ToString())
            //dd.id |> should equal g
            //value.IsSuccessStatusCode |> should equal true
            value.StatusCode |> should equal HttpStatusCode.NotFound
        }
        
    [<Fact>]
    let ``create new meeting`` () =
        let client = serverFactory.CreateClient()
        async {
            let data = {|
                MeetingGroupId = Guid.NewGuid()
                Title = "test"
                //AttendeesLimit = None
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
        (*let t = {
            raz = 5
            dwa = "testowy"
            trzy = new ResizeArray<bool>([
                false
                true
            ])
        }
        let s = JsonSerializer.Serialize(t)
        output.WriteLine(s)
        let d = JsonSerializer.Deserialize<Ta>(s)
        output.WriteLine(d.raz.ToString())
        output.WriteLine(d.trzy.[0].ToString())
        output.WriteLine(d.trzy.[1].ToString())*)
        let client = serverFactory.CreateClient()
        async {
            let g = Guid.Parse("EA0231DF-12A7-475C-A1B1-53F9EF0297B7")
            let uri = $"/meetings/{g}"
            let! value = client.GetAsync(uri) |> Async.AwaitTask
            let! res = value.Content.ReadAsStringAsync() |> Async.AwaitTask
            output.WriteLine(res)
            //let dd = JsonSerializer.Deserialize<C>(res)
            //output.WriteLine(dd.id.ToString())
            //dd.id |> should equal g
            //value.IsSuccessStatusCode |> should equal true
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
            //let j = JsonContent.Create(d)
            let str = new StringContent(d, Encoding.UTF8, "application/json")
            //let json = JsonContent.Create(data)
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
            //let j = JsonContent.Create(d)
            let str = new StringContent(d, Encoding.UTF8, "application/json")
            //let json = JsonContent.Create(data)
            let! s = str.ReadAsStringAsync() |> Async.AwaitTask
            output.WriteLine(s)
            let! res = client.PutAsync(uri, str) |> Async.AwaitTask
            let! out = res.Content.ReadAsStringAsync() |> Async.AwaitTask
            output.WriteLine(out)
            res.IsSuccessStatusCode |> should equal true
        }