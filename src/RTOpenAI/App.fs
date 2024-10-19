namespace RTOpenAI
open System
open Fabulous
open Fabulous.Maui
open Microsoft.Maui
open Microsoft.Maui.Graphics
open Microsoft.Maui.Accessibility
open Microsoft.Maui.Primitives

open type Fabulous.Maui.View

module App =
    let inline debug (s:'a) = System.Diagnostics.Debug.WriteLine(s)
    type Measure = {Date: DateTime; Weight: float}
    let testData = [ {Date = DateTime.Now; Weight = 100.}; {Date = DateTime.Now; Weight = 200.}]
    type Model = 
        { 
            weights  : Measure list        
        }

    type Msg = | Export | Clicked  | SetWeight of string

    type CmdMsg = SemanticAnnounce of string 

    let semanticAnnounce text =
        Cmd.ofSub(fun _ -> SemanticScreenReader.Announce(text))

    let mapCmd cmdMsg =
        match cmdMsg with
        | SemanticAnnounce text -> semanticAnnounce text

    let init () = { weights=testData }, []

    let update msg model =
        match msg with
        | Export -> model, []
        | SetWeight s -> debug s; model,[]
        | Clicked -> model, [SemanticAnnounce "You clicked the button!"]

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
                        Image("dotnet_bot.png")
                            .semantics(description = "Cute dotnet bot waving hi to you!")
                            .height(200.)
                            .centerHorizontal()

                        Label("Hello, World!")
                            .semantics(SemanticHeadingLevel.Level1)
                            .font(size = 32.)
                            .centerTextHorizontal()

                        Label("Welcome to .NET Multi-platform App UI powered by Fabulous")
                            .semantics(SemanticHeadingLevel.Level2, "Welcome to dot net Multi platform App U I powered by Fabulous")
                            .font(size = 18.)
                            .centerTextHorizontal()

                    })
                        .padding(30., 0., 30., 0.)
                        .centerVertical()
                )
                    .width(300)
            )
        )

    let program = Program.statefulWithCmdMsg init update view mapCmd
