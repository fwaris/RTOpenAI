#r "nuget: Jint"
open System
open Jint
(*
test to see if tau prolog could work in the standalone javascript engine jint
* it does not 
*)
let engine =
    (new Engine(fun opts ->
        opts
            .EnableModules(@"/Users/faisalwaris/repos/RTOpenAI/src/Samples/PlanSelection/PlanAssistant/Resources/Raw/wwwroot/")
            .Strict(false)
             |> ignore))
        .SetValue("log", new Action<obj>(Console.WriteLine))
    
engine.Execute("""
    import "core.js", "lists.js"
    function hello() { 
        log('Hello World');
    };
 
    hello();
""")

engine.Modules.Import("./core.js")

let coreJs = @"/Users/faisalwaris/repos/RTOpenAI/src/Samples/PlanSelection/PlanAssistant/Resources/Raw/wwwroot/core.js"
let listJs = @"/Users/faisalwaris/repos/RTOpenAI/src/Samples/PlanSelection/PlanAssistant/Resources/Raw/wwwroot/lists.js"

engine.Modules.Import(coreJs)