namespace CompanyName.MyMeetings.Modules.Meetings.InternalEvents.Errors

type ValidationError = {
    Target: string
    Value: obj
    Message: string
}

type DatabaseError = {
    Exception: exn
}

type MessageQueueError = {
    Exception: exn
}

type Error =
    | ValidationError of ValidationError
    | DatabaseError of DatabaseError
    | MessageQueueError of MessageQueueError