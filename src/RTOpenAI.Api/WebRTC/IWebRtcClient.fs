namespace RTOpenAI.WebRTC
open System
open System.Threading.Tasks
open System.Text.Json


type State = Disconnected | Connected | Connecting
    
type IWebRtcClient =
    inherit IDisposable        
    abstract Connect : key:string*url:string -> Task<unit>
    abstract Send : string -> bool
    abstract OutputChannel : System.Threading.Channels.Channel<JsonDocument>
    abstract StateChanged : IEvent<State>
    abstract State : State

