# Repo Overview

This repo the following reusable library projects.

- [RTOpenAI.Api](/src//RTOpenAI.Api/) - A Maui library to interface with the OpenAI realtime voice API via the WebRTC protocol.
    - [RTOpenAI.Events](/src/RTOpenAI.Events/) - strongly typed wrappers for OpenAI realtime API events
    - [Bindings/FsWebRTC.Bindings.Maui.Android](/src/Bindings/FsWebRTC.Bindings.Maui.Android/) - Maui WebRTC bindings for Android
    - [Bindings/FsWebRTC.Bindings.Maui.iOS](/src/Bindings/FsWebRTC.Bindings.Maui.iOS/) - Maui WebRTC bindings for iOS
- [RTFlow](/src/RTFlow/) - A framework for building realtime multi-agents systems.
- [FsPlan](/src/FsPlan/) - A library for organizing a collection of tasks into a plan with either linear or graphical flow
- [FsPlay](/src/FsPlay/) - A Maui library for programmatically driving embedded mobile browsers, e.g. using computer-use-agents or CUA
    - [FsPlay.Abstractions](/src/FsPlay.Abstractions/) - Abstractions for browser automation
- [FsAICore](/src/FsAICore/) - Common AI utilities used by the other libraries here.

## Samples Included
The libraries are complementary i.e. they can be combined together to build various types of AI applications. The use of these libraries are showcased in the following sample projects:

### [RT.Assistant](/src/Samples/PlanSelection/RT.Assistant/)
Please read the [RT.Assistant sample write-up](/src/Samples/PlanSelection/RT.Assistant/docs/writeup.md) included in this repo. It showcases the use of these libraries in a realtime voice-assistant app. 

Two videos are also available: [Overview](https://youtu.be/bSMByJvYLoY); [Code Walkthrough](https://youtu.be/0ghPhQyzyaI).

### [RTOpenAI.Sample](/src/Samples//Minimal/RTOpenAI.Sample/)
A minimal sample showcasing RTOpenAI for interfacing with the OpenAI realtime voice API

### [CuaSample](/src/Samples/CuaSample/)
A computer-use-agent (CUA) sample for driving mobile embedded browser based website to accomplish a goal, e.g. extract some information from the site.

## Other Projects

In addition to the three projects mentioned above, the following projects are also included in this repo:

- [SwiplcsCore](/src/Samples/PlanSelection/SwiPlcsCore/): Revamped C# bindings for SWI-Prolog to run under dotnet core (the original bindings are for .Net Framework)


# Build and Run Notes
- System requirements:
    - .Net 10 SDK
    - `[sudo] dotnet workload install maui`
    - [XCode and related (matching Maui workload)](https://developer.apple.com/xcode/resources/) required on MacOS to target IOS and MacCatalyst app builds
    - [Android Studio](https://developer.android.com/studio) for Android app target
    - OpenAI API Key
    - Anthropic API Key (for RT.Assistant sample)
    - *F# Maui projects cannot be debugged currently with VS Code*. You will need either [Visual Studio](https://visualstudio.microsoft.com/) or [JetBrains Rider](https://www.jetbrains.com/rider/). Both have free editions. For MacOs, Rider is the only viable option. However there are VS Code 'tasks' that can launch MacOS `MacCatalyst` versions of the included samples.

> ### Important Note:
> You will have to update the `<TargetPlatforms>...</TargetPlatforms>` in the **Maui** `.fsproj` files, depending on which platform (MacOs,Windows) your are *building on* and what you are *targeting* (IOS,MacCatalyst,Android,Windows):
> - [`RTOpenAI.Api.fsproj`](/src/RTOpenAI.Api/RTOpenAI.Api.fsproj)
> - [`RTOpenAI.Sample.fsproj`](/src/Samples/Minimal/RTOpenAI.Sample/RTOpenAI.Sample.fsproj)
> - [`RT.Assistant.fsproj`](/src/Samples//PlanSelection/RT.Assistant/RT.Assistant.fsproj)
> - [`FSPlay`](/src/FsPlay/FsPlay.fsproj)
>
> For example, if you are on MacOs then set `<TargetPlatforms>` to:
> -  `net9.0-ios;net9.0-maccatalyst`
> - On Windows use `net9.0-windows10.0.19041.0`
> - Add `net9.0-android` to the list if you have Android Studio or Rider with the Android plugin installed.
> - The default is `net9.0-ios;net9.0-maccatalyst;net9.0-android`.
> - For convenience, commented-out versions of the `<TargetFrameworks>` are included in the `.fsproj` files.
- Build twice if necessary. Sometimes Maui projects have to be built twice for build errors to go away
- Note that `dotnet build` at the solution root level is not likely to succeed as there are too many variations possible. Instead set the `<TargetFrameworks>` and use targeted builds in Rider or Visual Studio.

## Package Build
The release builds of Maui *apps* (not libraries) can take a while so a separate solution is provided [RT_FS_NugetPackages.sln](/RT_FS_NugetPackages.sln) to build nuget packages.
Use the following command to build packages:

 `dotnet pack -c Release RT_FS_NugetPackages.sln` 
 
# Acknowledgements

- Tim Lariviere and others for [Fabulous Maui](https://github.com/fabulous-dev/Fabulous.MauiControls)  
- [WebRTCme](https://github.com/melihercan/WebRTCme) - provided the base bindings for Maui WebRTC. These where modified (significantly for IOS) to make them work for RTOpenAI.
- Aaron Clauson and team for [SipSorcery](https://github.com/sipsorcery-org/sipsorcery) - WebRTC for the Windows platform.
- [Tau Prolog](http://tau-prolog.org/) - a javascript Prolog interpreter used in the RT.Assistant sample. 
- Loïc Denuzière and others for [FSharp.SystemTextJson](https://github.com/Tarmil/FSharp.SystemTextJson) - for F# types definitions
 and the accompanying serialization/de-serialization logic needed to handle OpenAI realtime protocol messages in a strongly-typed way.
- [SWI-Prolog](https://github.com/SWI-Prolog/swipl-devel) team for the base Prolog implementation that was instrumental in developing the Prolog-RAG approach used in RT.Assistant sample.
- Microsoft and [.Net Foundation](https://dotnetfoundation.org/) - for [dotnet](https://dotnet.microsoft.com/en-us/), [F#](https://fsharp.org/), [Maui](https://dotnet.microsoft.com/en-us/apps/maui) and [Microsoft.Extensions.AI](https://learn.microsoft.com/en-us/dotnet/ai/microsoft-extensions-ai)
