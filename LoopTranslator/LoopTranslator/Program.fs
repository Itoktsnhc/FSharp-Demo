// Learn more about F# at http://fsharp.org

open System
open System.Net.Http
open Newtonsoft.Json
open System.Text
open System.IO

type TranslateDirection =
    | CnToEng
    | EngToCn

let getTranslateQueryStr transDirection =
    match transDirection with
    | CnToEng -> "sl=zh-cn&tl=en"
    | EngToCn -> "tl=zh-cn&sl=en"

let key = (File.ReadAllText "./config.txt").Trim()

let endpoint =
    "https://api.cognitive.microsofttranslator.com/"

let http = new HttpClient()

http.DefaultRequestHeaders.TryAddWithoutValidation
    ("user-agent",
     "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.125 Safari/537.36 Edg/84.0.522.59")
|> ignore

type TransItem = { text: string }

type TransResult = { translations: TransItem list }

let getTranslateResult direction content =
    async {
        let req = new HttpRequestMessage()
        req.Method <- HttpMethod.Post

        let route =
            match direction with
            | CnToEng -> "translate?api-version=3.0&from=zh-Hans&to=en"
            | EngToCn -> "translate?api-version=3.0&from=en&to=zh-Hans"

        req.RequestUri <- new Uri(endpoint + route)
        req.Content <-
            new StringContent(JsonConvert.SerializeObject([| {| Text = content |} |]), Encoding.UTF8, "application/json")
        req.Headers.Add("Ocp-Apim-Subscription-Key", key)
        req.Headers.Add("Ocp-Apim-Subscription-Region", "eastasia")
        let! response = http.SendAsync(req) |> Async.AwaitTask
        response.EnsureSuccessStatusCode() |> ignore

        let! resp =
            response.Content.ReadAsStringAsync()
            |> Async.AwaitTask

        let respObj =
            resp
            |> JsonConvert.DeserializeObject<TransResult list>

        let text =
            match respObj with
            | d when d.Length > 0 -> respObj.Head.translations.Head.text
            | _ -> failwith "sasdfsadf"

        return text
    }

let rec loopTranslator content count direction =
    match count with
    | x when x = 0 -> content
    | y ->
        match direction with
        | CnToEng ->
            let innerContent =
                getTranslateResult CnToEng content
                |> Async.RunSynchronously

            printfn "[%s --> %s]" content innerContent
            loopTranslator innerContent (y - 1) EngToCn
        | EngToCn ->
            let innerContent =
                getTranslateResult EngToCn content
                |> Async.RunSynchronously

            printfn "[%s --> %s]" content innerContent
            loopTranslator innerContent (y - 1) CnToEng

[<EntryPoint>]
let main argv =
    printfn "\r\n"
    printfn "result: %s" (loopTranslator """古来圣贤皆寂寞，惟有饮者留其名。""" 10 CnToEng)
    0 // return an integer exit code
