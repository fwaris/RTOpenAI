#r "nuget: MetadataExtractor, 2.9.0-rc2"
open MetadataExtractor

let w1 = @"C:\Users\Faisa\Music\PinkPanther30.wav"
let r1 = MetadataExtractor.Formats.Wav.WavMetadataReader.ReadMetadata(w1)
r1.[0].Tags |> Seq.iter (fun x -> printfn "%A" x)


22050 = (44100 / 2)

44100. * 10./ 8.;;

#r "nuget: Silk.NET.Core, 2.22.0"
#r "nuget: Silk.NET.OpenAL, 2.22.0"
#r "nuget: Silk.NET.OpenAL.Soft.Native, 1.23.1"
#r "nuget: Silk.NET.OpenAL.Extensions.Soft, 2.22.0"
open Silk.NET.OpenAL
open Silk.NET.Core.Loader

let sampleRate = 22050
let byteRate = sampleRate * 2
let format = BufferFormat.Mono16
let blockAlign = 2
let bits = System.IO.File.ReadAllBytes(@"C:\Users\Faisa\Music\PinkPanther30 - Copy.pcm")

let alc = ALContext.GetApi()
let al = AL.GetApi()
let device = alc.OpenDevice("")
let _ =  context.CreateContext(device, NativeInterop.NativePtr.nullPtr)

let buffer = al.GenBuffer()
let source = al.GenSource()






