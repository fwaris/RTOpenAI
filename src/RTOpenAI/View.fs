namespace RTOpenAI
open System.Collections.ObjectModel
open Fabulous.Maui
open Microsoft.Maui.Controls
open Microsoft.Maui
open Microsoft.Maui.Graphics
open type Fabulous.Maui.View

module View =
    let logView (model:Model) =
        ((CollectionView model.log)
            (fun item -> Label($"{item}").lineBreakMode(LineBreakMode.WordWrap)))
            .gridColumn(1)

    let controlsView (model:Model) =
         (VStack(spacing = 25.) {
            
            Button(Connect.stateIcon model,Session_StartStop)
                .isEnabled(not (Connect.connecting model))
                .font(size=48.0, fontFamily="MaterialSymbols")
                .background(Colors.Transparent)
                .textColor(Colors.Magenta)
                .centerHorizontal()            

            Button((if model.player.IsNone then Icons.play else Icons.stop),Play_StartStop)
                .font(size=48.0, fontFamily="MaterialSymbols")
                .background(Colors.Transparent)
                .textColor(Colors.Lime)
                .centerHorizontal()
                
            Button((if model.recorder.IsSome then Icons.stop else Icons.mic),Recorder_StartStop)
                .font(size=48.0, fontFamily="MaterialSymbols")
                .background(Colors.Transparent)
                .textColor(Colors.Lime)
                .centerHorizontal()                             
        })
            .padding(30., 0., 30., 0.)
            .centerVertical()
            .gridColumn(0)
        
    let root model =
        Application() {
            Window(
                ContentPage(
                        Grid([Dimension.Star; Dimension.Star],[Dimension.Star]) {
                            controlsView model
                            logView model
                        }
                  )
                 .padding(0., 0., 0., 0.)
                 .width(300)
                )            
            }

