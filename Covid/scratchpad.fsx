#load ".paket/load/netcoreapp3.1/main.group.fsx"

open FSharp.Data
open FSharp.Core
open System
open System.IO

fsi.AddPrinter<DateTime>(fun d -> d.ToShortDateString())

[<Literal>]
let SampleDataFile =
    __SOURCE_DIRECTORY__
    + "./dataSource/03-22-2020.csv"

type Covid = CsvProvider<SampleDataFile, PreferOptionals=true>

Covid.GetSample().Rows

let files =
    Directory.GetFiles(__SOURCE_DIRECTORY__ + "./dataSource/", "*.csv")
    |> Seq.map Path.GetFullPath
    |> Seq.take 61

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
            allData
            |> Seq.groupBy (fun x -> x.Country_Region)

        for country, rows in byContry do
            let countryData =
                seq {
                    let byDate =
                        rows
                        |> Seq.groupBy (fun row -> row.Last_Update.Date)

                    for date, rows in byDate do
                        date,
                        rows
                        |> Seq.sumBy (fun row -> row.Confirmed)
                }
            country, countryData
    }
