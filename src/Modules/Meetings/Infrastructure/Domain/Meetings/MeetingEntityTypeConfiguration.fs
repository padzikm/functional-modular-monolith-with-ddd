module CompanyName.MyMeetings.Modules.Meetings.Infrastructure.Domain.Meetings.MeetingEntityTypeConfiguration

open CompanyName.MyMeetings.Modules.Meetings.Infrastructure.Domain.Meetings.MeetingsDbModel
open System
open System.Linq.Expressions
open Microsoft.EntityFrameworkCore
open Microsoft.FSharp.Linq.RuntimeHelpers

module Lambda =
    let toExpression (``f# lambda`` : Quotations.Expr<'a>) =
        ``f# lambda``
        |> LeafExpressionConverter.QuotationToExpression 
        |> unbox<Expression<'a>>

let expr f = f |> Lambda.toExpression
    

type MeetingEntityTypeConfiguration() =
    interface IEntityTypeConfiguration<DbMeeting> with
        member _.Configure builder =
            builder.ToTable("Meetings", "meetings") |> ignore 

//            builder
//                .HasKey(expr <@ Func<_,_>(fun x -> x.Id :> obj) @>) |> ignore            
//            builder
//                .Property(expr <@ Func<_,_>(fun p -> p.Title) @>)
//                .HasColumnName("Title") |> ignore
//            builder
//                .Property(expr <@ Func<_,_>(fun p -> p.EventFeeValue) @>)
//                .HasColumnName("EventFeeValue") |> ignore
//            builder
//                .Property(expr <@ Func<_,_>(fun p -> p.EventFeeCurrency) @>)
//                .HasColumnName("EventFeeCurrency") |> ignore
//            builder
//                .Property(expr <@ Func<_,_>(fun p -> p.MeetingId) @>)
//                .HasColumnName("Id")
//                .HasConversion(
//                    expr <@ Func<_,_>(fun p ->
//                        let (MeetingId v) = p
//                        v) @>,
//                    expr <@ Func<_,_>(fun v -> MeetingId(v)) @>
//                    ) |> ignore
//            builder
//                .Property(expr <@ Func<_,_>(fun p -> p.EventFee) @>)
//                .HasConversion()
