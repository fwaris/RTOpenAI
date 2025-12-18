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
    - *F# Maui projects cannot be debugged currently with VS Code*. You will need either [Visual Studio](https://visualstudio.microsoft.com/) or [JetBrains Rider](https://www.jetbrains.com/rider/). Both have free editions. For MacOs, Rider is the only viable option.

> ### Important Note:
> You will have to update the `<TargetPlatforms>...</TargetPlatforms>` in the following `.fsproj` files, depending on which platform (MacOs,Windows) your are *building on* and what you are *targeting* (IOS,MacCatalyst,Android,Windows):
> - [`RTOpenAI.Api.fsproj`](/src/RTOpenAI.Api/RTOpenAI.Api.fsproj)
> - [`RTOpenAI.Sample.fsproj`](/src/Samples/Minimal/RTOpenAI.Sample/RTOpenAI.Sample.fsproj)
> - [`RT.Assistant.fsproj`](/src/Samples//PlanSelection/RT.Assistant/RT.Assistant.fsproj)
>
> For example, if you are on MacOs then set `<TargetPlatforms>` to:
> -  `net9.0-ios;net9.0-maccatalyst`
> - On Windows use `net9.0-windows10.0.19041.0`
> - Add `net9.0-android` to the list if you have Android Studio or Rider with the Android plugin installed.
> - The default is `net9.0-android` as it can work on both MacOs and Windows.
> - For convenience, commented-out versions of the `<TargetFrameworks>` are included in the `.fsproj` files.
- Build twice if necessary. Sometimes Maui projects have to be built twice for build errors to go away
- Note that `dotnet build` at the solution root level is not likely to succeed as there are too many variations possible. Instead set the `<TargetFrameworks>` and use targeted builds in Rider or Visual Studio.

## Acknowledgements

- Tim Lariviere and others for [Fabulous Maui](https://github.com/fabulous-dev/Fabulous.MauiControls)  
- [WebRTCme](https://github.com/melihercan/WebRTCme) - provided the base bindings for Maui WebRTC. These where modified (significantly for IOS) to make them work for RTOpenAI.
- Aaron Clauson and team for [SipSorcery](https://github.com/sipsorcery-org/sipsorcery) - WebRTC for the Windows platform.
- [Tau Prolog](http://tau-prolog.org/) - a javascript Prolog interpreter used in the RT.Assistant sample. 
- Loïc Denuzière and others for [FSharp.SystemTextJson](https://github.com/Tarmil/FSharp.SystemTextJson) - for F# types definitions
 and the accompanying serialization/de-serialization logic needed to handle OpenAI realtime protocol messages in a strongly-typed way.
- [SWI-Prolog](https://github.com/SWI-Prolog/swipl-devel) team for the base Prolog implementation that was instrumental in developing the Prolog-RAG approach used in RT.Assistant sample.
- Microsoft and [.Net Foundation](https://dotnetfoundation.org/) - for [dotnet](https://dotnet.microsoft.com/en-us/), [F#](https://fsharp.org/), [Maui](https://dotnet.microsoft.com/en-us/apps/maui) and [Microsoft.Extensions.AI](https://learn.microsoft.com/en-us/dotnet/ai/microsoft-extensions-ai)
