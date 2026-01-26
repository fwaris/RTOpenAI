namespace FsPlaySamples.Cua

open System
open System.IO
open Fabulous
open FsPlan
open FsPlay
open FsPlaySamples.Cua
open FsPlaySamples.Cua.Agentic
open Microsoft.Maui.ApplicationModel
open Microsoft.Maui.Storage

module UiOps =
    let playIcon (m:Model) =
        m.flow
        |> Option.map(fun r -> Icons.fa_stop)
        |> Option.defaultValue Icons.fa_play
        
    let playMsg (m:Model) =
        m.flow
        |> Option.map(fun r -> StopFlow)
        |> Option.defaultValue CheckStartFlow        
        
    let postMsgDelayed model msg = async {
        do! Async.Sleep 1000
        model.mailbox.Writer.TryWrite msg |> ignore
    }
    
    let postAgentMessage model m=
        model.mailbox.Writer.TryWrite(FromRunningTask m) |> ignore
    
    let startStopFlow (model:Model) = async {
        match model.flow with
        | Some f -> f.Terminate()
                    return None
        | None when model.driver.IsSome ->
                  let previewActions = Settings.Environment.previewClicks()
                  let! iflow = Model.startPlan model.driver.Value previewActions (postAgentMessage model)
                  return Some iflow
        | None -> Log.error "driver not initialized"
                  return None
    }    
    
            
    let loadTask model (t,d) =
        // match t with
        // | Some (Target t) ->
        //     async {
        //         match model.driver with
        //         | Some d -> do! d.start t
        //         | None  -> ()                
        //     }
        //     |> Async.Start
        // | None -> ()
        {model with interactiveTask = Some d}, Cmd.none
        
    let doneTask model =
        model.flow |> Option.iter(fun f -> f.PostToAgent(Ag_Task_End))
        {model with interactiveTask = None}, Cmd.none                    
    
    let saveDom (dom:string) =
        let path = FileSystem.Current.AppDataDirectory @@ "dom.html"
        Utils.debug path
        File.WriteAllText(path,dom)
        
    let snapshot (model:Model) = task {
        let! (sn: byte[],_,_) = FsPlay.Service.capture Model.webviewCache.Value
        let path = FileSystem.Current.AppDataDirectory @@ "dom_image.jpeg"
        Utils.debug path
        File.WriteAllBytes(path, sn)
    }
    
    let toRunSteps (s:string) = match Double.TryParse s with true,v->v | _ -> 1.0
    
    let mergeValues (model:Model) (us:FsAICore.UsageMap)  =
        {model with usage = FsAICore.Usage.combineUsage model.usage us}           
    

    let installDriver (model:Model) =
        let driver = 
            Model.webviewCache.TryValue
            |> Option.map(fun wv ->   
                do FsPlay.MauiWebViewDriver.initialize Model.webviewWrapper.Value
                let driver =  FsPlay.MauiWebViewDriver.create()
                driver.driver)
        {model with driver = driver}
        
    let testSomething (model:Model) =
        let comp = 
            async {                              
                let js1 = """(function(){ return JSON.stringify({ ok: !!(true) }); })();"""
                let js2 = """(function(){ return JSON.stringify({ ok: !!(window.__fsDriver) }); })();"""
                //let js2 = """(function(){ return JSON.stringify({ ok: ("__fsDriver" in window) }); })();"""
                let js3 = """(function(){ return JSON.stringify({ ok: true }); })();"""
                let js4 = """(function () {
    return JSON.stringify({ ok: !!(window.__fsDriver && window.__fsDriver.clickAt(320, 26, 0)) });
  })();
"""                
                let! v = FsPlay.Service.evalJs (Model.webviewCache.Value) js4 |> Async.AwaitTask
                debug $"v = {v}"
                let! v2 =
                    match model.driver with
                    | Some d -> d.typeText("Neuro-symbolic AI")
                    | None -> async{return()}
                return ""
            }
        async {
            match! Async.Catch(comp) with
            | Choice1Of2 _ -> ()
            | Choice2Of2 ex -> debug (ex.Message)
        }
        |> Async.Start
        
        