namespace RTOpenAI
open System
open System.IO
open Fabulous
open Fabulous.Maui
open Fabulous.Maui.MediaElement
open Microsoft.Maui
open Microsoft.Maui.Graphics
open Microsoft.Maui.Accessibility
open Microsoft.Maui.Primitives
open type Fabulous.Maui.View
open Plugin.Maui.Audio
open Microsoft.Maui.Storage
open Microsoft.Maui.ApplicationModel

module App =
    let inline debug (s:'a) = System.Diagnostics.Debug.WriteLine(s)

    let semanticAnnounce text =
        Cmd.ofMsg(SemanticScreenReader.Announce(text))

    let mapCmd cmdMsg =
        match cmdMsg with
        | SemanticAnnounce text -> semanticAnnounce text

    let testData = [
        { Date = DateTime.Now; Weight = 100.0 }
        { Date = DateTime.Now.AddDays(-1.); Weight = 99.0 }
    ]

    let testFiles() =       
        let fileNames = ["mysound.wav";"short-beep-tone-47916.mp3"]
        async {
            try 
                for fileName in fileNames do
                    let! exists = FileSystem.AppPackageFileExistsAsync(fileName) |> Async.AwaitTask
                    let! size =
                        async {
                            if exists then
                                use! str = FileSystem.OpenAppPackageFileAsync(fileName) |> Async.AwaitTask
                                return str.Length
                                
                            else
                                return 0
                        }
                    debug (sprintf "File %s exists: %b with size %d" fileName exists size)
            with ex -> 
                debug ex.Message
        }

    let playSound (model:Model) =
        task {
            do! testFiles()
            try
                use! str = FileSystem.OpenAppPackageFileAsync("mysound.wav") 
                let player = model.audioManager.Value.CreatePlayer(str)
                debug $"Can seek: {player.CanSeek}"
                debug $"Can set speed: {player.CanSetSpeed}"
                debug $"Is playing: {player.IsPlaying}"
                debug $"Volume: {player.Volume}"                
                debug $"Curren position: {player.CurrentPosition}"
                debug $"Durtion {player.Duration}"
                MainThread.BeginInvokeOnMainThread(fun () ->
                    try player.Play() with ex -> debug ex.Message
                    player.Dispose()
                )
                ()
            with ex -> 
                debug ex.Message
            ()
        }
        |> ignore


    let init () = 
        let audioManager = lazy( IPlatformApplication.Current.Services.GetService(typeof<IAudioManager>) :?> IAudioManager)
        { weights=testData; audioManager=audioManager },[]

    let update msg model =
        match msg with
        | Export -> model, []
        | SetWeight s -> debug s; model,[]
        | Clicked -> playSound model; model,[]

    let view model =
        Application(
            ContentPage(
                ScrollView(
                    (VStack(spacing = 25.) {                        
                        (CollectionView(model.weights) (fun c -> 
                            HStack(spacing = 10.) {
                                Label(c.Date.ToString("yyyy-MM-dd"))
                                    .font(size = 16.)
                                    .centerHorizontal()
                                Label(c.Weight.ToString())
                                    .font(size = 16.)
                                    .centerHorizontal()
                            }))
                            .width(200)
                            .centerHorizontal()
                        (ListView(model.weights) (fun m -> EntryCell(null,string m.Weight,SetWeight)))
                            .width(200)
                            .centerHorizontal()
                            .header(Label("Weights"))
                        Button("\ue029", Clicked)
                            .font(48,fontFamily = "MaterialIconsTwoTone")
                            .semantics(hint = "Counts the number of times you click")
                            .centerHorizontal()
                        MediaElement()
                            .source("https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4")
                            .centerHorizontal()
                            .height(200.)
                        //Image("dotnet_bot.png")
                        //    .semantics(description = "Cute dotnet bot waving hi to you!")
                        //    .height(200.)
                        //    .centerHorizontal()

                    })
                        .padding(30., 0., 30., 0.)
                        .centerVertical()
                )
                    .width(300)
            )
        )

    let program  = 
        Program.statefulWithCmd init update
        |> Program.withView view
   
