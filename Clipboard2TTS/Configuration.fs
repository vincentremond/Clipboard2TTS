namespace Clipboard2TTS

open Newtonsoft.Json
open Pinicola.FSharp.IO

type Configuration = {
    Culture: string
    Rate: int
} with

    static member getFromAppSettings() =
        "appsettings.json"
        |> File.readAllText
        |> JsonConvert.DeserializeObject<Configuration>
