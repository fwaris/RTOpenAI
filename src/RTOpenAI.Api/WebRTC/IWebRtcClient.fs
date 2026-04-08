namespace RTOpenAI.WebRTC
open System
open System.Net
open System.Threading.Tasks
open System.Text.Json


type State = Disconnected | Connected | Connecting

type WebRtcClientConfig =
    { BindAddress: IPAddress option
      IceServerUrls: string list
      IceGatherTimeoutMs: int }
    static member Default =
        { BindAddress = None
          IceServerUrls = [ "stun:stun.l.google.com:19302" ]
          IceGatherTimeoutMs = 4000 }

[<RequireQualifiedAccess>]
module internal WebRtcClientConfigHelpers =
    let normalize (config: WebRtcClientConfig) =
        { config with
            IceServerUrls =
                config.IceServerUrls
                |> List.filter (String.IsNullOrWhiteSpace >> not)
            IceGatherTimeoutMs =
                if config.IceGatherTimeoutMs > 0 then
                    config.IceGatherTimeoutMs
                else
                    WebRtcClientConfig.Default.IceGatherTimeoutMs }
    
type IWebRtcClient =
    inherit IDisposable        
    abstract Connect : key:string*url:string*config:WebRtcClientConfig -> Task<unit>
    abstract Send : string -> bool
    abstract OutputChannel : System.Threading.Channels.Channel<JsonDocument>
    abstract StateChanged : IEvent<State>
    abstract State : State
