open System
open System.Globalization
open System.Speech.Synthesis
open Pinicola.FSharp.SpectreConsole
open Spectre.Console
open TextCopy

let getVoice (culture: string) (s: SpeechSynthesizer) =
    let cultureInfo = CultureInfo(culture)
    let voices = s.GetInstalledVoices(cultureInfo) |> Seq.toList

    match voices with
    | [ voice ] -> voice
    | [] -> failwith $"no voice is matching culture {culture}"
    | _ -> failwith $"multiple voices are matching culture {culture}: {voices}"

let splitText (text: string) =
    text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries ||| StringSplitOptions.TrimEntries)

let text = ClipboardService.GetText() |> splitText
let speechSynthesizer = new SpeechSynthesizer()
let voice = speechSynthesizer |> getVoice "fr-FR"
speechSynthesizer.SelectVoice(voice.VoiceInfo.Name)
speechSynthesizer.Rate <- 6

let table =
    Table.init ()
    |> Table.withWidth 80
    |> Table.addColumns [
        "X"
        "Text"
    ]
    |> Table.withBorder TableBorder.None
    |> Table.withShowHeaders false

let panelHeader =
    PanelHeader.fromString "([red]Clipboard2TTS[/])"
    |> PanelHeader.withJustification Justify.Right

let panel =
    Panel.fromRenderable table
    |> Panel.withBorder BoxBorder.Rounded
    |> Panel.withHeader panelHeader

AnsiConsole.live panel
|> LiveDisplay.withAutoClear false
|> LiveDisplay.withOverflow VerticalOverflow.Visible
|> LiveDisplay.start (fun ctx ->

    text
    |> Seq.iter (fun line ->
        table
        |> Table.addRow [
            " "
            line
        ]
        |> ignore
    )

    LiveDisplayContext.refresh ctx

    text
    |> Seq.iteri (fun rowIndex line ->

        (Markup.fromString "➤", table) ||> Table.updateCell rowIndex 0

        (Markup.fromInterpolated $"[bold black on white]{line}[/]", table)
        ||> Table.updateCell rowIndex 1

        LiveDisplayContext.refresh ctx

        speechSynthesizer.Speak(line)

        (Markup.fromString " ", table) ||> Table.updateCell rowIndex 0

        (Markup.fromInterpolated $"[default on default]{line}[/]", table)
        ||> Table.updateCell rowIndex 1

        LiveDisplayContext.refresh ctx
    )

)
