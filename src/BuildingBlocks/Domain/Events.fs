namespace CompanyName.MyMeetings.BuildingBlocks.Domain

open System

type IDomainEvent =
    abstract Id: Guid
    abstract OccuredOn: DateTime

type DomainEventBase () =
    interface IDomainEvent with
        member val Id = Guid.NewGuid()
        member val OccuredOn = DateTime.UtcNow
    