open System.Globalization
open System.Speech.Synthesis
open TextCopy

let getVoice (culture: string) (s: SpeechSynthesizer) =
    let cultureInfo = CultureInfo(culture)
    let voices = s.GetInstalledVoices(cultureInfo) |> Seq.toList

    match voices with
    | [ voice ] -> voice
    | [] -> failwith $"no voice is matching culture {culture}"
    | _ -> failwith $"multiple voices are matching culture {culture}: {voices}"

task {
    let! text = ClipboardService.GetTextAsync()
    let speechSynthesizer = new SpeechSynthesizer()
    let voice = speechSynthesizer |> getVoice "fr-FR"
    speechSynthesizer.SelectVoice(voice.VoiceInfo.Name)
    speechSynthesizer.Rate <- 6
    speechSynthesizer.Speak(text)
}
|> Async.AwaitTask
|> Async.RunSynchronously
