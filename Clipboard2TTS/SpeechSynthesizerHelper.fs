namespace Clipboard2TTS

open System.Globalization
open System.Speech.Synthesis

[<RequireQualifiedAccess>]
module SpeechSynthesizerHelper =

    let private getVoice (culture: string) (s: SpeechSynthesizer) =
        let cultureInfo = CultureInfo(culture)
        let voices = s.GetInstalledVoices(cultureInfo) |> Seq.toList

        match voices with
        | [ voice ] -> voice
        | [] -> failwith $"no voice is matching culture {culture}"
        | _ -> failwith $"multiple voices are matching culture {culture}: {voices}"

    let get culture rate =
        let speechSynthesizer = new SpeechSynthesizer()
        let voice = speechSynthesizer |> getVoice culture
        speechSynthesizer.SelectVoice(voice.VoiceInfo.Name)
        speechSynthesizer.Rate <- 6
        speechSynthesizer
