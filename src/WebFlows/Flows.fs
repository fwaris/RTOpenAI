namespace WebFlows

open System
open System.Text.Json
open System.Threading
open FSharp.Control

module Flows =
    open FlowsJs
    type EvalJs = string -> Async<string>

    let filterHiddenClickables (domS:DomSnapshot) =
        let cs =
            domS.clickables
//            |> List.filter (fun c -> c.x > 0 && c.y > 0. && c.width > 0. && c.height > 0.)            
            |> List.filter (fun c -> c.x <> 0 && c.y <> 0. && c.width > 0. && c.height > 0.)            
        { domS with clickables = cs }
                
    let findElementBox (driver:IMobileDriver) (el: ElemRef) =
        async {
            let! rslt = findElementBoxJs el |> fst |> driver.evaluateJs
            let rslt = Scripts.quoteWrap rslt 
            let rslt = JsonSerializer.Deserialize<string>(rslt, flSerOpts.Value)
            let domS = JsonSerializer.Deserialize<DomSnapshot>(rslt,flSerOpts.Value)
            let domS = filterHiddenClickables domS
            return Some domS            
        }
    
    let clickElement (driver:IMobileDriver) (el:ElemRef) =
        clickElementJs el |> fst |> driver.evaluateJs |> Async.Ignore
        
    let getValue (driver:IMobileDriver) acc (e:Extract) = async {
        let js,dbgJs = getValueJs e
        let! value = driver.evaluateJs js
        if value <> null then
            return acc |> Map.add e.Name value
        else
            return acc                
    }        
                
    let matchElement proto candidate =
        let common = Set.intersect proto.css_classes candidate.css_classes
        proto.elementId |> Option.map (fun x -> proto.elementId ==== candidate.elementId) |> Option.defaultValue true &&
        proto.aria_label |> Option.map (fun x -> proto.aria_label ==== candidate.aria_label) |> Option.defaultValue true &&
        proto.tag |> Option.map (fun x -> proto.tag ==== candidate.tag) |> Option.defaultValue true &&
        proto.css_classes.IsEmpty || common.Count > 0
    
    let mergeValues newMap prevMap =
        (prevMap,newMap)
        ||> Map.fold (fun acc k v ->
            match acc |> Map.tryFind k with
            | Some _ -> acc
            | None   -> acc |> Map.add k v            
        )
        
    let getValues (driver:IMobileDriver,extractions:Extract list) = task {
        let acc =  
            extractions
            |> AsyncSeq.ofSeq
            |> AsyncSeq.foldAsync (getValue driver) Map.empty
        return! acc 
    }
    
    let checkBox (driver:IMobileDriver) el = task {
        let! domS = findElementBox driver el
        return domS
        |> Option.bind(fun d->d.clickables |> List.tryHead)
        |> Option.map (fun c -> el,c)
    }
    
    let lastVisibleRef (driver:IMobileDriver) (elemRefs:ElemRef list)  = 
      elemRefs
      |> AsyncSeq.ofSeq
      |> AsyncSeq.mapAsync (checkBox driver >> Async.AwaitTask)
      |> AsyncSeq.choose id
      |> AsyncSeq.tryLast
      |> Async.StartAsTask

    let clickLastVisibleElement evalJs (elemRefs:ElemRef list)= task {        
          match! lastVisibleRef evalJs elemRefs with
          | Some (e,c) -> do! clickElement evalJs e
          | None -> ()
    }

    let findElements (driver:IMobileDriver) (elemRefs : ElemRef list) = async {
        // Check if any of the elemRefs exist and are visible in the DOM
        let! results = 
            elemRefs
            |> AsyncSeq.ofSeq
            |> AsyncSeq.mapAsync (checkBox driver >> Async.AwaitTask)
            |> AsyncSeq.toListAsync
        
        // Return true if at least one element was found
        return results |> List.exists Option.isSome
    }

    let rec awaitElements attempts (driver:IMobileDriver) (elemRefs : ElemRef list) = async {
        let! r = findElements driver elemRefs
        if r then 
            return()
        else
            if attempts > 0 then 
                do! Async.Sleep 1000
                return! awaitElements (attempts - 1) driver elemRefs
            else
                return failwith $"Unable to find elements"
    }

    let doStep (driver:IMobileDriver) step extractions = task {
        match step with
        | Page url    -> do! driver.goto url
        | Pause _     -> do! Async.Sleep 1000                     
        | Clicks refs -> do! awaitElements 10 driver refs
                         do! clickLastVisibleElement driver refs
        | Await refs  -> do! awaitElements 10 driver refs
        | Done _      -> do! Async.Sleep 1000
    }
    
    let undoStep (driver:IMobileDriver) step  = task {
        match step with
        | Pause _ | Done _ -> return ()
        | _       -> return! driver.goBack()
    }
        
    let step (driver:IMobileDriver) (flowRun:FlowRun) = task {
        match flowRun.ToDo with
        | step::rest -> do! doStep driver step flowRun.Flow.Extractions
                        do! Async.Sleep 1000
                        let! values = getValues (driver,flowRun.Flow.Extractions)
                        let values = mergeValues values flowRun.Values
                        return {flowRun with ToDo=rest; Done=step::flowRun.Done; Values=values}
        | _ -> return flowRun                             
    }
    let stepBack (driver:IMobileDriver) (flowRun:FlowRun) = task {
        match flowRun.Done with
        | step::rest -> do! undoStep driver step 
                        do! Async.Sleep 1000
                        return {flowRun with ToDo=step::flowRun.ToDo; Done=rest}
        | _ -> return flowRun                             
    }
