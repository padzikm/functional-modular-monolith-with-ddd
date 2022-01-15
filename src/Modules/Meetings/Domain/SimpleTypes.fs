namespace CompanyName.MyMeetings.Modules.Meetings.Domain.SimpleTypes

open FSharpPlus
open FSharpPlus.Data
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.Validation
open CompanyName.MyMeetings.BuildingBlocks.Domain.Errors

module Helpers =
    let checkNotNull v =
        if v = null then
            Error (StringError Null)
        else
            Ok v
    
    let checkNotEmpty v =
        if v = "" then
            Error (StringError Empty)
        else
            Ok v
            
    let checkMaxLength l (v:string) =
        if v.Length > l then
            Error (StringError (MaxLengthExceeded l))
        else
            Ok v
            
    let checkNotContains (c:string) er (v:string) =
        if v.Contains(c) then
            Error (StringError er)
        else
            Ok v

type MeetingName = private MeetingName of string

module MeetingName =
    open Helpers
    
    let create s =
        let c _ _ st = MeetingName st
        result{
            let! sv = checkNotNull s |> Result.mapError (fun e -> [e])
            return! c
                <!^> checkNotEmpty sv
                <*^> checkMaxLength 1024 sv
                <*^> checkNotContains "\n" ContainsNewline sv
        }
        
       
    
    let value (MeetingName s) = s
    
    let map f (MeetingName s) = MeetingName (f s)
    
type MeetingLocationCity = private MeetingLocationCity of string

module MeetingLocationCity =
    
    let create c: Validation<MeetingLocationCity, ValidationError> =
//        failwith ""
        Ok (MeetingLocationCity c)
        
type MeetingLocationPostcode = private MeetingLocationPostcode of string

module MeetingLocationPostcode =
    
    let create c: Validation<MeetingLocationPostcode, ValidationError> =
//        failwith ""
        Ok (MeetingLocationPostcode c)