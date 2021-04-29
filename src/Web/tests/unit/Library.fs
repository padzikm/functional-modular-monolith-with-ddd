module CompanyName.MyMeetings.Web.UnitTests.Tmp

open Fable.Jester
open Fable.ReactTestingLibrary
open Fable.FastCheck
open Fable.FastCheck.Jest
open App
open Fable.Core
open Fable.Core.JsInterop
open CompanyName.MyMeetings.Web.App

type HttpStub = {
    listen: unit -> unit
    close: unit -> unit
    reset: unit -> unit
}
    

[<Import("httpStub", "./httpStubs/test")>]
let httpStub: HttpStub = jsNative

Jest.beforeAll(fun _ -> httpStub.listen())
Jest.afterEach(fun _ -> httpStub.reset())
Jest.afterAll(fun _ -> httpStub.close())

//Jest.test("counter", fun () ->
//    let s = {Count = 5}
//    let d _ = ()
//    RTL.render(render s d) |> ignore
//    Jest.expect(RTL.screen.getByRole("heading")).toHaveTextContent(s.Count.ToString())
//    )

Jest.test("test jest", fun () ->
    let v = 2* 2
    printfn "Hello %d" v
    Jest.expect(v).toBe(4)
    )

Jest.test("http test", promise {
    let c = Components.Counter()
    let _ = RTL.render(c)
    Jest.expect(RTL.screen.getByRole("button")).toHaveTextContent("Increment")
    do! RTL.waitFor(fun () -> Jest.expect(RTL.screen.getByRole("heading")).toHaveTextContent("-1"))
    })

Jest.test("http test2", promise {
    let c = Components.Counter()
    let _ = RTL.render(c)
    Jest.expect(RTL.screen.getByRole("button")).toHaveTextContent("Increment")
    do! RTL.waitFor(fun () -> Jest.expect(RTL.screen.getByRole("heading")).toHaveTextContent("-1"))
    })

Jest.test("rtl test", fun () ->
    let c = Components.HelloWorld()
    let _ = RTL.render(c)
    Jest.expect(RTL.screen.getByRole("heading")).toHaveTextContent("Hello World")
    )
          
Jest.test("fastcheck test jest", fun () ->
    FastCheck.assert'(FastCheck.property(Arbitrary.Defaults.integer, fun i -> Jest.expect(i + 2).toBe(i + 1 + 1)))
    )

Jest.test.prop("fastcheck test jest prop", Arbitrary.Defaults.integer, fun i -> Jest.expect(i + 2).toBe(i + 1 + 1))
