namespace CompanyName.MyMeetings.Modules.Meetings.Domain.Members

open System
open CompanyName.MyMeetings.BuildingBlocks.Domain
open FsToolkit.ErrorHandling
//open CompanyName.MyMeetings.Modules.Meetings.Domain.Members.Events
open FsToolkit.ErrorHandling.Operator.Validation

type private MemberInternal = {
    MemberId: Guid
    Login: string
    FirstName: string
    LastName: string
    Name: string
    Email: string
    CreatedDate: DateTime
}

type Member = private Member of MemberInternal

type DomainResult = {
    Entity: Member
    Events: IDomainEvent list
}

module Member =
    
    let create id login firstname lastname name email =
        let f clock =
            let ev = Events.MemberCreatedDomainEvent(id)
            let m = Member ({MemberId = id; Login = login; FirstName = firstname; LastName = lastname; Name = name; Email = email; CreatedDate = clock})
            let e: IDomainEvent list = [ev]
            {Entity = m; Events = e}
        f