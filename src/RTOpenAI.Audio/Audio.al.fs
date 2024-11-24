namespace RTOpenAI.Audio.Al
open System
open System.IO
open System.Diagnostics
open System.Threading
open System.Threading.Channels
open FSharp.Control
open RTOpenAI.Audio
open Silk.NET.OpenAL
open FSharp.NativeInterop
open Silk.NET.OpenAL.Extensions.EXT
#nowarn "9" //suppress native interop warning

module Audio =
    let _checkError(al:AL,src:string) =
        let err = al.GetError();
        if err <> AudioError.NoError then
            let msg = $"Audio processing error in {src}: %A{err}"
            Log.error msg                
            Utils.debug msg
            //failwith msg
    
    let listInputDevices() = 
        use alc = ALContext.GetApi();
        use al = AL.GetApi()
        
        use enumerations : Silk.NET.OpenAL.Extensions.Enumeration.Enumeration= 
            match alc.TryGetExtension(NativePtr.nullPtr) with
            | false, _ -> failwith "Failed to get extension"
            | true,extensions -> extensions    

        let defDvcSpc = Silk.NET.OpenAL.Extensions.EXT.Enumeration.GetCaptureEnumerationContextString.DefaultCaptureDeviceSpecifier
        let defDevice = enum<Silk.NET.OpenAL.Extensions.Enumeration.GetEnumerationContextString> (int defDvcSpc)
        let defaultInputName = enumerations.GetString(NativePtr.nullPtr, defDevice)
        let inpDvcSpcs = Silk.NET.OpenAL.Extensions.EXT.Enumeration.GetCaptureContextStringList.CaptureDeviceSpecifiers
        let inpDvcEnm = enum<Silk.NET.OpenAL.Extensions.Enumeration.GetEnumerationContextStringList> (int inpDvcSpcs)
        let inputList = enumerations.GetStringList(inpDvcEnm)
        inputList
        |> Seq.map (fun x -> x, (x=defaultInputName))
        |> Seq.toList    


type internal PlayState =
        {
            alc : ALContext
            al : AL
            device : nativeptr<Device>
            context : nativeptr<Context>
            buffers : uint[]
            source : uint
            mutable disposed : bool
        }
    with                
        member this.CheckError(src:string) = Audio._checkError(this.al,src)
            
        static member Create() =
            let alc = ALContext.GetApi()
            let al = AL.GetApi()
            Audio._checkError(al,"init")
            let device = alc.OpenDevice(String.Empty)
            Audio._checkError(al,"open device")            
            let context = alc.CreateContext(device, NativePtr.nullPtr)
            Audio._checkError(al,"create context")            
            let _ = alc.MakeContextCurrent(context)
            Audio._checkError(al,"make context concurrent")            
            let buffers = al.GenBuffers(2)
            Audio._checkError(al,"gen buffers")            
            let source = al.GenSource()
            Audio._checkError(al,"gen source")            
            let state = 
                {
                    alc = alc
                    al = al
                    device = device
                    context = context
                    buffers = buffers
                    source = source
                    disposed = false
                }
            al.SetSourceProperty(source, SourceBoolean.SourceRelative, true)
            state.CheckError("relative source")
            al.SetSourceProperty(source,  SourceFloat.Gain, 1.0f )
            state.CheckError("gain")
            al.SetSourceProperty(source, SourceVector3.Position, 0.f,0.f,0.f)
            state.CheckError("position")
            state
            
        interface IDisposable with
            member this.Dispose() =
                if not this.disposed then
                    this.disposed <- true
                    this.al.DeleteSource(this.source)
                    this.al.DeleteBuffers(this.buffers)
                    this.alc.DestroyContext(this.context)
                    this.alc.CloseDevice(this.device) |> ignore
                    this.alc.Dispose()
                    this.al.Dispose()
             
type internal RecordState =
    {
        alc : ALContext
        al : AL
        capture : Extensions.EXT.Capture
        mic : Extensions.EXT.AudioCapture<BufferFormat>
        buffer : byte[]
        mutable disposed : bool
    }
    with
        interface IDisposable with
            member this.Dispose() =
                if not this.disposed then
                    this.mic.Stop()
                    this.mic.Dispose()
                    this.capture.Dispose()
                    this.alc.Dispose()
                    this.al.Dispose()
            
        member this.CheckError(src) = Audio._checkError(this.al,src)
        
        static member Create(format:AudioFormat,activate:(unit->unit) option) =
            activate |> Option.iter (fun f -> f())
            let nullPtr = NativePtr.nullPtr
            let alc = ALContext.GetApi()
            let al = AL.GetApi()
            Audio._checkError(al,"init")
           
            let enumerations : Extensions.Enumeration.Enumeration= 
                match alc.TryGetExtension(nullPtr) with
                | false, _ -> failwith "Failed to get extensions enumeration for audio input"
                | true,extensions -> extensions
                
            let defDvcSpc = Extensions.EXT.Enumeration.GetCaptureEnumerationContextString.DefaultCaptureDeviceSpecifier
            let defDevice = enum<Extensions.Enumeration.GetEnumerationContextString> (int defDvcSpc)
            let defaultInputName = enumerations.GetString(nullPtr, defDevice)
           
            let bufferSize = format.ByteRate * 2 // 2 seconds
            let audioBuffer : byte[] = Array.zeroCreate (bufferSize)
            let cap = new Extensions.EXT.Capture(alc.Context)
            Audio._checkError(al,"create capture")
            let cap = new Capture(alc.Context)
            Thread.Sleep(1000)
            let audio = new AudioCapture<BufferFormat>(cap,defaultInputName, uint format.Frequency, Nullable format.BufferFormat, format.Frequency * 2 )
            
            //let mic = cap.CreateCapture(defaultInputName, uint format.Frequency, Nullable format.BufferFormat, format.Frequency * 2 )
            Audio._checkError(al,"mic")            
            {
                al = al
                alc = alc
                capture = cap
                mic = audio
                buffer = audioBuffer
                disposed = false
            }
            