# Repo Overview

This repo has two main reusable library projects.

- [RTFlow](/src/RTFlow/) - A framework for building realtime multi-agents systems.
- [RTOpenAI](/src//RTOpenAI.Api/) - A library to interface with the OpenAI realtime voice API via the WebRTC protocol.

The two are complementary. Please read the [RT.Assistant sample write-up](/src/Samples/PlanSelection/RT.Assistant/writeup.md) included in this repo first. It showcases the use of these libraries in a realtime voice-assistant app.

The libraries are available in source form for now. The plan is to distribute them as nuget packages after the source starts to stabilize.

## Other Projects

In addition to the three projects mentioned above, the following projects are also included in this repo:

- [WebRTCme.Bindings.Maui.Android](/src/Bindings/WebRTCme.Bindings.Maui.Android/): Maui bindings for WebRTC Android
- [WebRTCme.Bindings.Maui.iOS](/src/Bindings/WebRTCme.Bindings.Maui.iOS/): Maui bindings for WebRTC IOS and MacCatalyst
- [OpenAI.Events](/src/RTOpenAI.Events/): Strongly-typed message wrappers for the OpenAI realtime API messages
- [RTOpenAI.Sample](/src/Samples/Minimal/RTOpenAI.Sample/): Minimal sample for realtime voice applications
- [SwiplcsCore](/src/Samples/PlanSelection/SwiPlcsCore/): Revamped C# bindings for SWI-Prolog to run under dotnet core (the original bindings are for .Net Framework)

## Build and Run Notes
- System requirements:
    - .Net 9 SDK
    - `[sudo] dotnet workload install maui`
    - [XCode and related](https://developer.apple.com/xcode/resources/) required on MacOS to target IOS and MacCatalyst app builds
    - [Android Studio](https://developer.android.com/studio) for Android app target
    - OpenAI API Key
    - Anthropic API Key (for RT.Assistant sample)
- F# Maui projects cannot be debugged currently with VS Code. You will need either [Visual Studio](https://visualstudio.microsoft.com/) or [JetBrains Rider](https://www.jetbrains.com/rider/). For MacOs, Rider is the only viable option.
- Although you cannot debug in VS Code, you can launch the included samples apps using VS Code tasks included with solution.
- Build twice if necessary. Sometimes Maui projects have to be built twice for build errors to go away
- The solution cannot be built for all platforms as the same time so `dotnet build` at the solution root will not work. There are windows-only and IOS/MacCatalyst-specific projects that can only be built on their respective platforms.

- When launching from the command line (or running in Visual Studio or Rider
) specify the target framework. Specifically, the MacOS/IOS components cannot be built in Windows and Windows components cannot be built in MacOS. So on Windows you can build bwith 

## Acknowledgements

- Tim Lariviere and others for [Fabulous Maui](https://github.com/fabulous-dev/Fabulous.MauiControls)  
- [WebRTCme](https://github.com/melihercan/WebRTCme) - provided the base bindings for Maui WebRTC. These where modified (significantly for IOS) to make them work for RTOpenAI.
- [Tau Prolog](http://tau-prolog.org/) - a javascript Prolog interpreter used in the RT.Assistant sample. 
- Loïc Denuzière and others for [FSharp.SystemTextJson](https://github.com/Tarmil/FSharp.SystemTextJson) - for F# types definitions
 and the accompanying serialization/de-serialization logic needed to handle OpenAI realtime protocol messages in  strongly-typed way.
- [SWI-Prolog](https://github.com/SWI-Prolog/swipl-devel) team for the base Prolog implementation that was instrumental in developing the Prolog-RAG approach used in RT.Assistant sample.

