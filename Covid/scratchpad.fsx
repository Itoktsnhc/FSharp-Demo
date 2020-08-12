#load ".paket/load/netcoreapp3.1/main.group.fsx"

open FSharp.Data
open FSharp.Core
open System
open System.IO

fsi.AddPrinter<DateTime>(fun d -> d.ToShortDateString())

[<Literal>]
let SampleDataFile =
    __SOURCE_DIRECTORY__
    + "./dataSource/01-22-2020.csv"

type Covid = CsvProvider<SampleDataFile>

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


