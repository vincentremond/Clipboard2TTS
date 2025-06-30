namespace Clipboard2TTS

open System.Globalization
open System.Speech.Synthesis

[<RequireQualifiedAccess>]
module SpeechSynthesizerHelper =

    let private getVoice (culture: string) (s: SpeechSynthesizer) =
        let cultureInfo = CultureInfo(culture)
        let voices = s.GetInstalledVoices(cultureInfo) |> Seq.toList

        match voices |> List.tryHead with
        | Some voice -> voice
        | None -> failwith $"no voice is matching culture {culture}"

    let get culture rate =
        let speechSynthesizer = new SpeechSynthesizer()
        let voice = speechSynthesizer |> getVoice culture
        speechSynthesizer.SelectVoice(voice.VoiceInfo.Name)
        speechSynthesizer.Rate <- rate
        speechSynthesizer
