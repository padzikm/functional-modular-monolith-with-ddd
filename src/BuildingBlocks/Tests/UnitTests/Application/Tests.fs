module Tests

open System
open System.Collections.Generic
open CompanyName.MyMeetings.BuildingBlocks.Application.Errors
open FsCheck.Xunit
open Xunit
open Xunit
open Xunit.Abstractions
open FsUnit.Xunit
open FsCheck

type Tests (output: ITestOutputHelper) =

    [<Fact>]
    let ``empty list to empty dict`` () =
        let er = {
            ValidationErrors = []
        }
        
        let res = er |> Helpers.toMap
        
        res.Count |> should equal 0
    
    [<Fact>]
    let ``one validation error to one key in dict`` () =
        let er = {
            ValidationErrors = [
                {Target = "bla"; Message = ["cos"]}
            ]
        }
        
        let res = er |> Helpers.toMap
        
        res.Count |> should equal 1
        res.["bla"].Length |> should equal 1
        
    [<Fact>]
    let ``two validation errors to one key in dict`` () =
        let er = {
            ValidationErrors = [
                {Target = "bla"; Message = ["cos"; "tmp"]}
            ]
        }
        
        let res = er |> Helpers.toMap
        
        res.Count |> should equal 1
        res.["bla"].Length |> should equal 2
        
    [<Fact>]
    let ``two validation errors to two keys in dict`` () =
        let er = {
            ValidationErrors = [
                {Target = "bla"; Message = ["cos"; "tmp"]}
                {Target = "ops"; Message = ["cos2"; "tmp2"]}
            ]
        }
        
        let res = er |> Helpers.toMap
        
        res.Count |> should equal 2
        res.["bla"].Length |> should equal 2
        res.["ops"].Length |> should equal 2
        
    [<Property>]
    let ``validation errors to keys in dict`` (m: Map<string, string list>) =
//        output.WriteLine (sprintf "%O" m)
        let s = Map.toSeq m
        let se = seq {
                for (t, m) in s ->
                    {Target = t; Message = m}
            }
        let l = List.ofSeq se
        let er = {
            ValidationErrors = l
        }
        
        let res = er |> Helpers.toMap
        
        res.Count |> should equal m.Count
        
        |> Prop.trivial (m.Count = 0)
        |> Prop.classify (m.Count = 1) "tylko 1"
        |> Prop.classify (m.Count > 1) "wiecej niz 1"
        |> Prop.collect (m.Count)
        