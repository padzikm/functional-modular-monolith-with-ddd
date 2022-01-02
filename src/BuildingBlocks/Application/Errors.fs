namespace CompanyName.MyMeetings.BuildingBlocks.Application.Errors

open System

type ValidationError = {
    Target: string
    Message: string list
}

type InvalidCommandError = {
    ValidationErrors: ValidationError list
}

type Error =
    | InvalidCommandError of InvalidCommandError
    | InfrastructureError of exn
    
module Helpers =
    let toMap (cmdErr: InvalidCommandError) =
        
        let rec go (ver: ValidationError list) =
            if ver.Length = 0 then
                Seq.empty
            elif ver.Length = 1 then
                let h = ver.Head
                seq {
                    (h.Target, Array.ofList h.Message)
                }
            else
                let h = ver.Head
                let t = ver.Tail
                seq{
                    yield! go [h]
                    yield! go t
                }
        
        let r = go cmdErr.ValidationErrors
        Map(r)
    