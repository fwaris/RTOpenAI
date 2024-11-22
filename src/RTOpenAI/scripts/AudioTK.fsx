#r "nuget: OpenToolkit.OpenAL, 4.0.0-pre9.3"
open System
open System.Threading
open OpenToolkit.Audio.OpenAL
Environment.SetEnvironmentVariable("PATH", Environment.GetEnvironmentVariable("PATH") + ";" + @"E:\s\rtapi")
System.Runtime.InteropServices.NativeLibrary.Load("openal32.dll")
let devices = ALC.GetStringList(GetEnumerationStringList.DeviceSpecifier);
let mutable recording : int16[] = Array.zeroCreate (44100 * 4)
let captureDevice = ALC.CaptureOpenDevice(null, 44100, ALFormat.Mono16, 1024);
let mutable current = 0
ALC.CaptureStart(captureDevice);
while (current < recording.Length) do
    let samplesAvailable = ALC.GetInteger(captureDevice, AlcGetInteger.CaptureSamples);
    if (samplesAvailable > 512) then    
        let samplesToRead = Math.Min(samplesAvailable, recording.Length - current);
        ALC.CaptureSamples(captureDevice, ref recording[current], samplesToRead);
        current <- current + samplesToRead;
    Thread.Yield() |> ignore
ALC.CaptureStop(captureDevice);
