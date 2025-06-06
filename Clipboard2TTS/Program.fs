open System
open Clipboard2TTS
open Pinicola.FSharp
open Pinicola.FSharp.SpectreConsole
open Spectre.Console
open TextCopy

let configuration = Configuration.getFromAppSettings ()

let speechSynthesizer =
    SpeechSynthesizerHelper.get configuration.Culture configuration.Rate

let text =
    ClipboardService.GetText()
    |> String.splitWithOptions
        Environment.NewLine
        (StringSplitOptions.RemoveEmptyEntries ||| StringSplitOptions.TrimEntries)

Console.Title <- "Clipboard2TTS"
Console.WindowWidth <- 80
Console.BufferWidth <- 80

let table =
    Table.init ()
    // |> Table.withWidth 80
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
