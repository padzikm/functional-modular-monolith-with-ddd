namespace CompanyName.MyMeetings.BuildingBlocks.Application.Errors

open System
open CompanyName.MyMeetings.BuildingBlocks.Domain.Errors

type TargetedValidationError = {
    Target: string
    Errors: ValidationError list
}

type Error =
    | CommandValidationError of TargetedValidationError list
    | InfrastructureError of exn
    
module Helpers =
        
    let toMap (cmdErr: TargetedValidationError list) =
        
        let rec go (ver: TargetedValidationError list) =
            if ver.Length = 0 then
                Seq.empty
            elif ver.Length = 1 then
                let h = ver.Head
                seq {
                    (h.Target, Array.ofList h.Errors)
                }
            else
                let h = ver.Head
                let t = ver.Tail
                seq{
                    yield! go [h]
                    yield! go t
                }
        
        let r = go cmdErr
        Map(r)
    