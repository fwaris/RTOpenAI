//#r "nuget: Silk.NET.OpenAL"
open System.Runtime.InteropServices

#r "nuget: Silk.NET.OpenAL.Soft.Native"
#r "nuget: Silk.NET.OpenAL.Extensions.Soft"
#r "nuget: Silk.NET.OpenAL.Extensions.EXT"
#r "nuget: Silk.NET.OpenAL.Extensions.Enumeration"

open System
open System.IO
open Silk.NET.OpenAL
open System.Threading
open Silk.NET.OpenAL.Native.Extensions

let (@@) a b = Path.Combine(a,b)
let root = Environment.GetFolderPath Environment.SpecialFolder.UserProfile @@ "rt"
if Directory.Exists root |> not then Directory.CreateDirectory root |> ignore

let sampleRate = 22050
let format = BufferFormat.Mono16
let nullPtr = NativeInterop.NativePtr.nullPtr


let playPcmFile (pcm:byte[]) = 
    async {
        use alc = ALContext.GetApi();
        use al = AL.GetApi();
        let device = alc.OpenDevice("")
        let context = alc.CreateContext(device, nullPtr);
        let _ = alc.MakeContextCurrent(context);

        let err = al.GetError();

        if err <> AudioError.NoError then
            Console.WriteLine($"Error: {err}")

        let source = al.GenSource();
        let buffer = al.GenBuffer();
        al.SetSourceProperty(source, SourceBoolean.Looping, false)
        al.BufferData(buffer, format, pcm, sampleRate)

        al.SetSourceProperty(source, SourceInteger.Buffer, buffer);
        al.SourcePlay(source);

        let checkState() = 
            let mutable state = 0
            al.GetSourceProperty(source, GetSourceInteger.SourceState, &state)
            enum<SourceState> state

        async {            
            while(checkState() <> SourceState.Stopped) do
                do! Async.Sleep(1000)
                printfn "Playing..."
        } |> Async.RunSynchronously
        

        al.SourceStop(source);

        al.DeleteSource(source);
        al.DeleteBuffer(buffer);
        alc.DestroyContext(context);
        let deviceClosed = alc.CloseDevice(device)
        printfn $"Device closed: {deviceClosed}"
        al.Dispose();
        alc.Dispose();
    }
    |> Async.Start

(*
playPcmFile pp1
playPcmFile (File.ReadAllBytes(@"C:\Users\Faisa\Music\recording.pcm"))
*)

let listInputDevices() = 
    use alc = ALContext.GetApi();
    use al = AL.GetApi();

    use enumerations : Silk.NET.OpenAL.Extensions.Enumeration.Enumeration= 
        match alc.TryGetExtension(nullPtr) with
        | false, _ -> failwith "Failed to get extension"
        | true,extensions -> extensions    

    let defDvcSpc = Silk.NET.OpenAL.Extensions.EXT.Enumeration.GetCaptureEnumerationContextString.DefaultCaptureDeviceSpecifier
    let defDevice = enum<Silk.NET.OpenAL.Extensions.Enumeration.GetEnumerationContextString> (int defDvcSpc)
    let defaultInputName = enumerations.GetString(nullPtr, defDevice)
    printfn "Default input device: %s" defaultInputName

    printfn "Available input devices:"
    let inpDvcSpcs = Silk.NET.OpenAL.Extensions.EXT.Enumeration.GetCaptureContextStringList.CaptureDeviceSpecifiers
    let inpDvcEnm = enum<Silk.NET.OpenAL.Extensions.Enumeration.GetEnumerationContextStringList> (int inpDvcSpcs)
    let inputList = enumerations.GetStringList(inpDvcEnm)
    inputList |> Seq.iter (fun x -> printfn "%s" x)

(*
listInputDevices()
*)

type AudioFormat = {
    Frequency: int
    BitsPerSample: int
    Channels : int
}
    with
        member x.FrameSize = x.BitsPerSample * x.Channels / 8
        member x.ByteRate = x.Frequency * x.FrameSize
        member x.BufferFormat = 
            match x.Channels, x.BitsPerSample with
            | 1,8   -> BufferFormat.Mono8
            | 1,16  -> BufferFormat.Mono16
            | 2,8   -> BufferFormat.Stereo8
            | 2,16  -> BufferFormat.Stereo16
            | _     -> failwith "Unsupported audio format"

let capture (samples:int) (buffer: byref<int16[]>) (mic: Silk.NET.OpenAL.Extensions.EXT.AudioCapture<BufferFormat>) =
    use ptr  = fixed buffer
    printfn "ptr: %A" ptr
    let ptr2 = NativeInterop.NativePtr.toVoidPtr ptr
    mic.CaptureSamples(ptr2, samples)
    //let mutable buf : byte[] = Array.zeroCreate buffer.Length
    //mic.CaptureSamples(samples, &buf)
    //buf

let recordToFile (format:AudioFormat) (file:BinaryWriter) (token:System.Threading.CancellationToken) = 
    task{
        use alc = ALContext.GetApi();
        use al = AL.GetApi();

        let enumerations : Silk.NET.OpenAL.Extensions.Enumeration.Enumeration= 
            match alc.TryGetExtension(nullPtr) with
            | false, _ -> failwith "Failed to get extensions enumeration for audio input"
            | true,extensions -> extensions    

        let defDvcSpc = Silk.NET.OpenAL.Extensions.EXT.Enumeration.GetCaptureEnumerationContextString.DefaultCaptureDeviceSpecifier
        let defDevice = enum<Silk.NET.OpenAL.Extensions.Enumeration.GetEnumerationContextString> (int defDvcSpc)
        let defaultInputName = enumerations.GetString(nullPtr, defDevice)
        printfn "Input device: %s" defaultInputName

        let bufferSize = format.ByteRate * 2 // 2 seconds

        let audioBuffer : byte[] = Array.zeroCreate (bufferSize)

        use cap = new Silk.NET.OpenAL.Extensions.EXT.Capture(alc.Context)
        use mic = cap.CreateCapture(defaultInputName, uint format.Frequency, Nullable format.BufferFormat, format.Frequency * 2 )
        mic.Start()
        while not token.IsCancellationRequested  do
            do! Async.Sleep(1000)
            let samples = mic.AvailableSamples
            let sz = samples * format.FrameSize
            if samples > 0 then 
                let ptr = fixed audioBuffer
                let ptr2 = NativeInterop.NativePtr.toVoidPtr ptr
                mic.CaptureSamples(ptr2, samples)
                printfn $"{samples} bytes {sz} %A{audioBuffer.[0..20]}" 
                file.Write(audioBuffer, 0, sz)
        mic.Stop() 
        mic.Dispose()
        cap.Dispose()
        al.Dispose()
        alc.Dispose()
        printfn "Recording stopped"
    }

let record seconds (file:string) = 
    let comp = 
        async {
            let cts = new System.Threading.CancellationTokenSource(seconds * 1000)
            let audioFormat = { Frequency = 22050; BitsPerSample = 16; Channels = 1 }
            use str = File.Create(file)
            use file = new BinaryWriter(str)
            do! recordToFile audioFormat file cts.Token |> Async.AwaitTask
        }
    async {
        match! Async.Catch comp with
        | Choice1Of2 _ -> printfn "Recording completed"
        | Choice2Of2 ex -> printfn "Recording failed: %A" (ex.Message,ex.StackTrace)
    }
    |> Async.Start
    


(*
let file = root @@ "recording.pcm"
record 5 file
playPcmFile (File.ReadAllBytes(file))
*)

