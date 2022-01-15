namespace CompanyName.MyMeetings.BuildingBlocks.Domain.Errors

type StringError =
    | Null
    | Empty
    | MaxLengthExceeded of int
    | ContainsNewline

type ValidationError =
    | StringError of StringError
