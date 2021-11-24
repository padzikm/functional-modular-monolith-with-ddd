namespace CompanyName.MyMeetings.Modules.Meetings.Application.Members.CreateMember

open System
open CompanyName.MyMeetings.Modules.Meetings.Domain.Members
open FSharpPlus
open FSharpPlus.Data

type CreateMemberCommand =
    { MemberId: Guid
      Login: string
      FirstName: string
      LastName: string
      Email: string
      Name: string }

type Env = { Conn: string; Now: DateTime }

type Env1 = { Now: DateTime }

type Env2 = { Conn: string }

module Handler =

    let save m =
        monad {
            let! (env: Env2) = Reader.ask
            return "zapisano! " + env.Conn
        }

    let create cmd =
        monad {
            let! (r: Env1) = Reader.ask
            let mem =
                Member.create cmd.MemberId cmd.Login cmd.FirstName cmd.LastName cmd.Name cmd.Email r.Now

            return mem
        //let! (en: Env) = Reader.ask
        //let! uu = Reader(fun er -> {Now = er.Now})
        //let a = Reader.local (fun (e:Env) -> {Now = e.Now}) Reader.ask
        //let! u = Reader.map (fun e -> {|Now = e.Now|}) reader
//            let! r = handle cmd |> Reader.local (fun (e:Env) -> {Now = e.Now})
        //let! a = Reader.local (fun (e:Env) -> {Now = e.Now})
        //let! b = Reader.local (fun (e:Env) -> {Now = e.Now})
//            return r
        }

    let handle cmd =
        //        let m = Member.create cmd.MemberId cmd.Login cmd.FirstName cmd.LastName cmd.Name cmd.Email

        monad {
            //            let! (env:Env) = Reader.ask
//            let mem = m r.Now
//            return mem
            let! m =
                create cmd
                |> Reader.local (fun (e: Env) -> { Now = e.Now })

            let! r =
                save m
                |> Reader.local (fun (e: Env) -> { Conn = e.Conn })

            return r//, m.DomainEvents
        }
