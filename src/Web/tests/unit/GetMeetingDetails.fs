module CompanyName.MyMeetings.Web.UnitTests.GetMeetingDetails

open System
open System.Text.RegularExpressions
open CompanyName.MyMeetings.Web.MeetingDetails
open Fable.Jester
open Fable.ReactTestingLibrary
open Fable.Core.JS
open Fable.RegexProvider

Jest.test("correctly displays meeting details", promise {
    let fakeId = Guid.NewGuid()
    let c = DisplayMeetingDetails fakeId
    RTL.render(c) |> ignore
    let! id = RTL.screen.findByLabelText("id")
    Jest.expect(id).toHaveValue(fakeId)
    let! title = RTL.screen.findByLabelText("title")
    Jest.expect(title).toHaveDisplayValue(SafeRegex.Create<"\w+">())
    })
