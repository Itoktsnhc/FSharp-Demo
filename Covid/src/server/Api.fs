module Api

open FSharp.Data
open FSharp.Core
open System.IO


[<Literal>]
let SampleDataFile = "../../dataSource/05-01-2020.csv"

type Daily = CsvProvider<SampleDataFile, PreferOptionals=true, IgnoreErrors=true>



let files =
    Directory.GetFiles("../../dataSource/", "*.csv")
    |> Seq.map Path.GetFullPath

let allData =
    files
    |> Seq.map Daily.Load
    |> Seq.collect (fun data -> data.Rows)
    |> Seq.distinctBy (fun row -> row.Country_Region, row.Province_State, row.Last_Update.Date)
    |> Seq.sortBy (fun row -> row.Last_Update.Date)
    |> Seq.filter (fun row -> row.Country_Region <> "Others")

let confirmedByContryDaily =
    [|
       let byContry =
           allData |> Seq.groupBy (fun x -> x.Country_Region)

       for country, rows in byContry do
           let countryData =
               [|
               for date, row in rows
                                |> Seq.groupBy (fun row -> row.Last_Update.Date) do
                   {| Date = date
                      Confirmed = row |> Seq.sumBy (fun row -> row.Confirmed)
                      Deaths = row |> Seq.sumBy (fun row -> row.Deaths)
                      Recovered = row |> Seq.sumBy (fun row -> row.Recovered) |} |]

           country, countryData |]

let countryLookup = confirmedByContryDaily |> Map

let allCountries = confirmedByContryDaily |> Array.map fst

let countryStats =
    [| for country, stats in confirmedByContryDaily do
        let mostRecent = stats |> Array.tryLast
        match mostRecent with
        | Some s -> {| s with Country = country |}
        | None -> () |]
