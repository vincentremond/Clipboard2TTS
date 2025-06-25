open System
open Clipboard2TTS
open Fargo
open Fargo.Parsers
open Pinicola.FSharp
open Pinicola.FSharp.Fargo
open Pinicola.FSharp.SpectreConsole
open Spectre.Console
open TextCopy

let commandLineParser =
    let tryParseInt32 error =
        optParse (Int32.tryParse >> (Result.ofOption error))

    fargo {
        let! culture = opt "culture" "c" "en-US" "The culture to use for TTS" |> defaultValue "en-US"

        let! rate =
            opt "rate" "r" "8" "The speech rate (-10 to 10)"
            |> tryParseInt32 "Invalid rate, must be an integer between -10 and 10"
            |> defaultValue 8

        return {
            Culture = culture
            Rate = rate
        }
    }

FargoCmdLine.run
    "Clipboard2TTS"
    commandLineParser
    (fun configuration ->

        let speechSynthesizer =
            SpeechSynthesizerHelper.get configuration.Culture configuration.Rate

        let text =
            ClipboardService.GetText()
            |> String.splitWithOptions Environment.NewLine (StringSplitOptions.RemoveEmptyEntries ||| StringSplitOptions.TrimEntries)

        Console.Title <- "Clipboard2TTS"

        let table =
            Table.init ()
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
        |> LiveDisplay.withOverflow VerticalOverflow.Ellipsis
        |> LiveDisplay.withCropping VerticalOverflowCropping.Bottom
        |> LiveDisplay.start (fun ctx ->

            text
            |> Seq.iter (fun line ->
                table
                |> Table.addRow [
                    " "
                    Markup.escape line
                ]
                |> ignore
            )

            LiveDisplayContext.refresh ctx

            text
            |> Array.iteri'
                (fun rowIndex removedRows line ->

                    let fixedRowIndex = rowIndex - removedRows

                    let fixedRowIndex, removedRows =
                        if fixedRowIndex > 3 then
                            table |> Table.removeRow 0 |> ignore
                            fixedRowIndex - 1, (removedRows + 1)

                        else
                            fixedRowIndex, removedRows

                    (Markup.fromString "➤", table) ||> Table.updateCell fixedRowIndex 0

                    (Markup.fromInterpolated $"[bold black on white]{line}[/]", table)
                    ||> Table.updateCell fixedRowIndex 1

                    LiveDisplayContext.refresh ctx

                    speechSynthesizer.Speak(line)

                    (Markup.fromString " ", table) ||> Table.updateCell fixedRowIndex 0

                    (Markup.fromInterpolated $"[default on default]{line}[/]", table)
                    ||> Table.updateCell fixedRowIndex 1

                    LiveDisplayContext.refresh ctx

                    removedRows
                )
                0

        )
    )
