module Main

open Browser.Dom
open Fable.Core.JsInterop

importSideEffects "./styles/global.scss"

open Elmish
open Elmish.Debug
open Elmish.Navigation
open Elmish.UrlParser
open Elmish.HMR
open CompanyName.MyMeetings.Web.App


Program.mkProgram init update render
//|> Program.toNavigable (parseHash Router.pageParser) urlUpdate
#if DEBUG
|> Program.withConsoleTrace
#endif
|> Program.withReactSynchronous "app"
#if DEBUG
|> Program.withDebugger
#endif
|> Program.run
