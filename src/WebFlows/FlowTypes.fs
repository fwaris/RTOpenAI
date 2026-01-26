namespace WebFlows
open System.Text.Json

type IMobileDriver =
    abstract evaluateJs : string -> Async<string>
    abstract goto : string -> Async<unit>
    abstract goBack : unit -> Async<unit>

type ElemRef =
        {
            elementId : string option
            aria_label: string option
            inner_text : string option
            css_classes: Set<string>
            tag : string option
            path : string option
            xpath : string option
        }
    with static member Default =
            {
                elementId = None
                aria_label = None
                inner_text = None
                css_classes = Set.empty
                tag = None
                path = None
                xpath = None
            }
            
type Extract =
    {
        Name: string
        ElemRef: ElemRef
    }
            
type FlowStep =
    | Page of string
    | Clicks of ElemRef list
    | Pause of {|label:string; altFlowId:string|}
    | Await of ElemRef list
    | Done of string

type Flow = { FlowId: string; Path:FlowStep list; Extractions: Extract list}
    with static member Default = { FlowId="new flow"; Path=[]; Extractions=[]}

type FlowRun = { ToDo:FlowStep list; Done:FlowStep list;  Values:Map<string,string>; Flow:Flow}
    with static member Default = { ToDo=[]; Done=[]; Values=Map.empty; Flow=Flow.Default}
         static member FromFlow flow = {FlowRun.Default with ToDo=flow.Path; Flow=flow}
 
type ClickableElement = {
    aria_label: string option
    inner_text : string option
    tag: string
    id: string option
    classList: string list
    role: string option
    path: string
    x: float
    y: float
    width: float
    height: float
    zIndex : float
}

type DomSnapshot = {
    zoom: float
    scrollX: float
    scrollY: float
    viewportWidth: float
    viewportHeight: float
    documentWidth: float
    documentHeight: float
    clickables: ClickableElement list
}

module FlowSer =     
    open System.Text.Json.Serialization
    open System.Text.Json
    let serOpts =
        let opts =
            JsonFSharpOptions.Default()
                //.WithSkippableOptionFields(true)
                .WithUnionInternalTag()
                .WithUnionTagName("type")
                .WithUnionUnwrapRecordCases()
                .WithUnionTagCaseInsensitive()
                .WithAllowNullFields()
                .WithAllowOverride()
                .WithUnionUnwrapFieldlessTags()
                .ToJsonSerializerOptions()
        opts.WriteIndented <- true
        opts

    let serFlow (flow:Flow) = JsonSerializer.Serialize(flow,serOpts)        
