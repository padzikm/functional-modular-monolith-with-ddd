namespace CompanyName.MyMeetings.Modules.Meetings.Domain.SimpleTypes

open FSharpPlus
open FSharpPlus.Data
open FsToolkit.ErrorHandling.Operator.Validation

module Helpers =
    let checkEmpty msg v =
        if v = "" then
            Error msg
        else
            Ok v
            
    let checkLength l msg (v:string) =
        if v.Length > l then
            Error msg
        else
            Ok v
            
    let checkContains (c:string) msg (v:string) =
        if v.Contains(c) then
            Error msg
        else
            Ok v

type MeetingsName = private MeetingsName of string

module MeetingsName =
    open Helpers
    
    let create s =
        let c _ _ s = MeetingsName s
        c            
        <!^> checkEmpty "not empty" s
        <*^> checkLength 1024 "at most 1024 chars" s
        <*^> checkContains "\n" "not contains new line" s
    
    let value (MeetingsName s) = s
    
    let map f (MeetingsName s) = MeetingsName (f s)
    
