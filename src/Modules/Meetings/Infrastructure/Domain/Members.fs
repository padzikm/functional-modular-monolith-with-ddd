namespace CompanyName.MyMeetings.Modules.Meetings.Infrastructure.Domain

open System
open System.Linq.Expressions
open Microsoft.EntityFrameworkCore
open Microsoft.FSharp.Linq.RuntimeHelpers

[<CLIMutable>]
type Member = {
    Id: Guid
    Login: string
    FirstName: string
    LastName: string
    Name: string
    Email: string
    CreatedDate: DateTime
} 

module Lambda =
    let toExpression (``f# lambda`` : Quotations.Expr<'a>) =
        ``f# lambda``
        |> LeafExpressionConverter.QuotationToExpression 
        |> unbox<Expression<'a>>

    let expr f = f |> toExpression
    

type MeetingEntityTypeConfiguration() =
    interface IEntityTypeConfiguration<Member> with
        member _.Configure builder =
            builder.ToTable("Members", "meetings") |> ignore
            