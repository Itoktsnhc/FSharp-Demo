#load ".paket/load/netcoreapp3.1/main.group.fsx"

open XPlot.Plotly

open FSharp.Data
open FSharp.Core
open System.IO
open System

fsi.AddPrinter<DateTime>(fun d -> d.ToShortDateString())

[<Literal>]
let SampleDataFile =
    __SOURCE_DIRECTORY__
    + "./dataSource/03-22-2020.csv"

type Covid = CsvProvider<SampleDataFile, PreferOptionals=true, IgnoreErrors=true>

Covid.GetSample().Rows

let files =
    Directory.GetFiles(__SOURCE_DIRECTORY__ + "./dataSource/", "*.csv")
    |> Seq.map Path.GetFullPath

let allData' =
    files
    |> Seq.collect (fun r ->
        let data = Covid.Load r
        data.Rows)

let allData =
    seq {
        for file in files do
            let data = Covid.Load file
            yield! data.Rows
    }

let confirmedByContryDaily =
    seq {
        let byContry =
            allData |> Seq.groupBy (fun x -> x.Country_Region)

        for country, rows in byContry do
            let countryData =
                seq {
                    let byDate =
                        rows
                        |> Seq.groupBy (fun row -> row.Last_Update.Date)

                    for date, rows in byDate do
                        date, rows |> Seq.sumBy (fun row -> row.Confirmed)
                }

            country, countryData
    }

let topTen =
    confirmedByContryDaily
    |> Seq.sortByDescending (fun (country, dates) ->
        let lastDate, confirmed = dates |> Seq.last
        confirmed)
    |> Seq.take 30



let makeScatter (country, values) =
    let dates, numbers = values |> Seq.toArray |> Array.unzip
    let trace = Scatter(x = dates, y = numbers) :> Trace
    trace.name <- country
    trace

topTen
|> Seq.map makeScatter
|> Chart.Plot
|> Chart.Show
