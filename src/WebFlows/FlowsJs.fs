namespace WebFlows

open System
open System.Text.Json
open System.Text.Encodings.Web

module FlowsJs =
    let flSerOpts = lazy(
        let opts = JsonSerializerOptions(JsonSerializerDefaults.Web, WriteIndented=true, AllowTrailingCommas=true)        
        opts.Encoder <- JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        opts)
            
    let toElement (clickable:ClickableElement) : ElemRef =
        {
            elementId = clickable.id
            inner_text = clickable.inner_text
            aria_label = clickable.aria_label
            css_classes = set clickable.classList
            tag = checkEmpty clickable.tag
            path = None
            xpath = None
        }
        
    let toParms (el: ElemRef) = JsonSerializer.Serialize(el, flSerOpts.Value)

    let findElementJs (el:ElemRef) = 
        let paramJson = toParms el
        let jsCallDbg = $"(function() {{ {Scripts.findElementRaw}; return findElement({paramJson}); }})()"
        let jsCall = $"(function() {{ {Scripts.findElement}; return findElement({Scripts.escapeSomeChars paramJson}); }})()"
        jsCall,jsCallDbg
        
    let findElementBoxJs (el: ElemRef) =
        let paramJson = toParms el
        let jsCallDbg = $"(function() {{ {Scripts.findElementRaw}; {Scripts.findBoundingBoxesRaw}; return findBoundingBoxes({paramJson}); }})()"
        let jsCall = $"(function() {{ {Scripts.findElement.Value}; {Scripts.findBoundingBoxes.Value}; return findBoundingBoxes({Scripts.escapeSomeChars paramJson}); }})()"
        jsCall,jsCallDbg

    let clickElementJs (el:ElemRef) =
        let paramJson = toParms el
        let jsCallDbg = $"(function() {{ {Scripts.findElementRaw}; {Scripts.clickElementRaw}; return clickElement({paramJson}); }})()" 
        let jsCall = $"(function() {{ {Scripts.findElement.Value}; {Scripts.clickElement.Value}; return clickElement({Scripts.escapeSomeChars paramJson}); }})()"
        jsCall,jsCallDbg
            
    let getValueJs (e:Extract) =
        let paramJson = toParms e.ElemRef
        let jsCallDbg = $"(function() {{ {Scripts.findElementRaw}; {Scripts.getElementValueRaw}; return getElementValue({paramJson}); }})()"
        let jsCall = $"(function() {{ {Scripts.findElement.Value}; {Scripts.getElementValue.Value}; return getElementValue({Scripts.escapeSomeChars paramJson}); }})()"
        jsCall,jsCallDbg
                    
