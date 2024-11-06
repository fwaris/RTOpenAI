#r "nuget: Silk.NET.OpenAL"
#r "nuget: Silk.NET.OpenAL.Soft.Native"
#r "nuget: Silk.NET.OpenAL.Extensions.Soft"
open System
open Microsoft.FSharp.NativeInterop
open Silk.NET.OpenAL
open System.IO

let wav = File.ReadAllBytes(@"/Users/faisalwaris/Downloads/PinkPanther30.wav").[44..]
let sampleRate = 22050
let format = BufferFormat.Mono16

let alc = ALContext.GetApi();
let al = AL.GetApi();
let device = alc.OpenDevice("")
let nullPtr = NativeInterop.NativePtr.nullPtr
let context = alc.CreateContext(device, nullPtr);
alc.MakeContextCurrent(context);

al.GetError();

let source = al.GenSource();
let buffer = al.GenBuffer();
al.SetSourceProperty(source, SourceBoolean.Looping, true)
al.BufferData(buffer, format, wav, sampleRate)


al.SetSourceProperty(source, SourceInteger.Buffer, buffer);
al.SourcePlay(source);

Console.WriteLine("Press Enter to Exit...");
Console.ReadLine();

al.SourceStop(source);

al.DeleteSource(source);
al.DeleteBuffer(buffer);
alc.DestroyContext(context);
alc.CloseDevice(device);
al.Dispose();
alc.Dispose();

