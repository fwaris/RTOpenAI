namespace FsPlaySamples.Cua.Agentic

open FsAICore
open FsPlaySamples.Cua
open System
open System.ComponentModel
open Microsoft.SemanticKernel

type ArticleTools(poster: FromAgent -> unit) = 

    [<KernelFunction("save_summary")>]
    [<Description("Save the article summary")>]
    member this.save_summary(title:string, summary:string) = 
        let comp = async {
            try
                Log.info $"[ArticleTools] {nameof this.save_summary} {title}\n{summary}"
                poster (FromAgent.Summary (title,summary))
                return "summary saved"
            with ex ->
                Log.exn(ex, nameof this.save_summary)
                return $"error occurred while trying to save summary"
        }
        Async.StartAsTask comp
        
type TaskTools(poster: AgentMsg -> unit) = 

    [<KernelFunction("end_task")>]
    [<Description("Ends the current task")>]
    member this.end_task() = 
        let comp = async {
            try
                Log.info $"[TaskTools] ending current task"
                poster Ag_Task_End
                return "ended task"
            with ex ->
                Log.exn(ex, nameof this.end_task)
                return $"error occurred while trying to end task"
        }
        Async.StartAsTask comp

module Plans =    
    open FsPlan
    let getOutput (t:FsTask<Cu_Task>) (kernel:IServiceProvider) = async {
        return ""
    }
    
    let amazonTasks = [
        {   id = Tid "intro"
            task = Cu_Interactive (Some (Target "https://amazon.com"), "Login, then click Continue")
            description = "introduction"
            toolNames = []
        }
        {   id = Tid "find_camera_case"
            task = Cu_Cua None
            description = """Find at least 3, top rated, cases for the iphone 16 pro max where
each has a built-in screen protector and a stand.
Use the `save_summary` tool to save information about the case.
"""
            toolNames = [ToolName "save_summary"]                       
        }               
    ]
    
    
    let twitterTasks = [
        {   id = Tid "intro"
            task = Cu_Interactive (Some (Target "https://x.com"), "Login, then click Continue")
            description = "introduction"
            toolNames = []
        }
        {   id = Tid "find_posts"
            task = Cu_Cua None
            description = """"Search for posts related to `Neuro-Symbolic AI`.
If a post show engagement (more than 5 likes and at least 1 reply) then save the post using the `save_summary` tool.
Scroll down the search results to find interesting posts.
Use Alt-Left to go back a page, if needed.
Try to find about 5 summaries.
"""
            toolNames = [ToolName "save_summary"]                       
        }        
    ]   
    
    
    let redditTasks = [
        {   id = Tid "intro"
            task = Cu_Interactive (Some (Target "https://reddit.com"), "Login, then click Continue")
            description = "introduction"
            toolNames = []
        }
        {   id = Tid "find_posts"
            task = Cu_Cua None
            description = """Find posts related to 'neuro-symbolic ai'. 
Make sure to click the 'Ask' button after entering a search term.
Summarize each thread and use the `save_summary` tool to save the thread summary.
"""
            toolNames = [ToolName "save_summary"]                       
        }               
    ]          
    let testPlan =
        let tasks = twitterTasks //redditTasks //amazonTasks //twitterTasks
        {
            tasks = tasks |> List.map (fun x ->x.id,x) |> Map.ofList
            flow = FsPlanFlow.Sequential (tasks |> List.map _.id)
        }
