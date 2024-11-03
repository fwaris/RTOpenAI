namespace RTOpenAI
open Fabulous.Maui
open Microsoft.Maui
open Microsoft.Maui.Graphics
open type Fabulous.Maui.View

module View = 

    let logView (model:Model) =
        (ListView(model.log) (fun item -> TextCell($"{item}")))
            .gridColumn(1)
            .header(Label("Log"))
            .horizontalScrollBarVisibility(ScrollBarVisibility.Never)

    let controlsView (model:Model) = 
        (VStack(spacing = 25.) {
            Ellipse()
                .size(10., 10.)
                .background(Colors.Green)
            Button(Icons.play, Play_Start)
                .font(48,fontFamily = "MaterialIconsTwoTone")
                .semantics(hint = "Counts the number of times you click")
                .centerHorizontal()
            Button(Icons.cancel, Play_Stop)
                .font(48,fontFamily = "MaterialIconsTwoTone")
                .semantics(hint = "Counts the number of times you click")
                .centerHorizontal()
            Button((if model.recorder.IsNone then Icons.mic else Icons.stop) , Recorder_StartStop)
                .font(48,fontFamily = "MaterialIconsTwoTone")
                .semantics(hint = "Counts the number of times you click")
                .centerHorizontal()
        })
            .padding(30., 0., 30., 0.)
            .centerVertical()
            .gridColumn(0)
        

    let view model =
        Application(
            ContentPage(
                ScrollView(
                    Grid([Dimension.Star; Dimension.Star],[Dimension.Star]) {
                        controlsView model
                        logView model
                    }
                )
                    .width(300)
            )
        )

