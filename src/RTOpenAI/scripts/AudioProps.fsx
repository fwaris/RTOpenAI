#r "nuget: MetadataExtractor, 2.9.0-rc2"
open MetadataExtractor

let w1 = @"C:\Users\Faisa\Music\PinkPanther30.wav"
let r1 = MetadataExtractor.Formats.Wav.WavMetadataReader.ReadMetadata(w1)
r1.[0].Tags |> Seq.iter (fun x -> printfn "%A" x)


22050 = (44100 / 2)

44100. * 10./ 8.;;

