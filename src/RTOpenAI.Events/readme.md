# RTOpenAI.Events
Defines strongly-typed wrappers for OpenAI realtime API protocol messages.

All currently documented (12/16/2025) messages have been accounted for.

However, (de)serialization has not been fully tested across all scenarios.

### Audio and Data Channels
In the case of WebRTC, there two channels of communication with the OpenAI realtime API - audio and data. The two are connected but also work independently. 

The audio conversation and data exchange happen concurrently. The audio conversation will likely continue even if there is an error on the data channel. But tool calls and other events from the server may not arrive on the client side as expected.

### Error handling
The following are **ServerEvents** union cases related to error handling:

- **Error** - is an error message sent by the server in relation to *some* error. It could be related to a past message sent from the client. Pay particular attention to this message as it will likely affect how the server responds from this point forward.
- **UnknownEvent (msgType,JsonDocument)** - is generated on the client side when the JSON message sent by the server cannot be mapped to a known type. The raw JSON and its 'type' string (if any) are included in this message 
- **EventHandlingError (err,msgType,JsonDocument)** - is generated on the client side when the JSON message sent by server cannot be parsed/deserialized. The message includes the error message as well as the msgType and raw JSON.

### Possible Serialization Issues
OpenAI realtime API protocol messages are described in detail but there is an ambiguity related to optional fields. Assume `data` is an optional field then there are three distinct possibilities:
- `data` value is present
```json
{
  "id": "xyz",
  "data": {...}
}
```
- `data` is null
```json
{
  "id": "xyz",
  "data" : null
}
```
- `data` tag is missing
```json
{
  "id": "xyz"
}

```

To account for this, the F# record type is defined as:
```fsharp
type Data = {..}
type R = {
    id:string
    data:Skippable<Data option>
}
```
Skippable<'t> is a very useful type defined by FSharp.SystemTextJson.
The above type definition accounts for all three cases. However, that complicates the type definition and its use, as the Skippable + Option cases need to be handled explicitly.

For ease of use, the option tag is dropped sometimes e.g. `data:Skippable<Data>`. This will handle 2 of the above 3 cases. It will not handle the `data:null` case. 

The bottom line is that there may be cases were we are assuming that the optional field will not be sent but is actually sent as null. In that case serialization will fail. The library will have to be updated accordingly.
