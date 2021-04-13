module CompanyName.MyMeetings.Web.UnitTests

open Fable.Jester
open Fable.ReactTestingLibrary
open Fable.FastCheck
open Fable.FastCheck.Jest
open App

Jest.test("test jest", fun () ->
    let v = 2* 2
    printfn "Hello %d" v
    Jest.expect(v).toBe(4)
    )

Jest.test("rtl test", fun () ->
    let c = Components.HelloWorld()
    let _ = RTL.render(c)
    Jest.expect(RTL.screen.getByRole("heading")).toHaveTextContent("Hello World")
    )
          
Jest.test("fastcheck test jest", fun () ->
    FastCheck.assert'(FastCheck.property(Arbitrary.Defaults.integer, fun i -> Jest.expect(i + 2).toBe(i + 1 + 1)))
    )

Jest.test.prop("fastcheck test jest prop", Arbitrary.Defaults.integer, fun i -> Jest.expect(i + 2).toBe(i + 1 + 1))
