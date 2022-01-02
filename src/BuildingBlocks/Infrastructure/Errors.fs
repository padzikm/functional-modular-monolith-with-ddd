namespace CompanyName.MyMeetings.BuildingBlocks.Infrastructure.Errors

type DatabaseError = {
    Exception: exn
}

type MessageQueueError = {
    Exception: exn
}
