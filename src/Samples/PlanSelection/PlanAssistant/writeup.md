# RT.Assistant Sample

This sample is a voice-enabled assistant that allows one to determine the best phone plan for ones needs from a set of available plans. The assistant can be conversed with via voice to discover the types of plans and features available and their prices, to make a selection.

### Plan Offerings

Phone plans (like many other offerings these days) are bundled product and services which makes it non-trivial to ascertain which plan will work best for ones needs. Consider the following components of a typical contemporary plan:

- *Base plan* with voice, text and data rates & limits
- *Mobile hotspot* data rates & limits (20gb, 50gb, 100gb?)
- *Premium data* rates & limits (with premium, ones data gets preference in a crowded/overloaded network)
- *Streaming services* e.g. Netflix, Hulu, Apple TV, etc.
- *Taxes and fees* may or may not be included in the plan price
- Special discounts for *Military Veterans*, *First Responders*, *Seniors*, etc.
- *In-flight* data rates & limits
- Then there may be seasonal or campaign *promotions*

Additionally, how many lines one gets affects what is included. For example, NetFlix may not be included for a single line (one phone number) but is included for two or more lines. 

### Technologies Showcased
Internally the system maintains a 'database' of mocked (but representative) phone plans that may be available from a
typical major telecom as set of Prolog ['facts'](https://www.cs.trincoll.edu/~ram/cpsc352/notes/prolog/factsrules.html). Staring from voice input to how the database is queried and results produced involves several components. This sample is a demonstration of the following integrated frameworks and technologies:

- **RTFlow**: Multi-agent framework for realtime GenAI applications - written in F#
- **RTOpenAI**: F# library for interfacing with the OpenAI realtime API via the WebRTC protocol
- **Fabulous** for Maui: F# library for building native mobile applications (IOS, Android, +) with Microsoft Maui
- **Tau**: JavaScript-based Prolog engine and its use in a native mobile app via the Maui HybridWebView control.
- Integration with the Anthropic API for Prolog code generation with the **Microsoft.Extensions.AI** library

## Overview
There is a lot going on here: *generative AI*; *old-school symbolic AI*; *multi-agents*; *realtime voice*; *cross-platform native mobile apps*; to name some. The following explains how these are all stitched together into a comprehensive system.

- Firstly, this is a *voice-enabled* assistant so the user can talk to the assistant to get questions answered about phone plans.
- The plan information is stored as set of logically consistent Prolog facts that remove any ambiguity about features and prices.
- The assistant listens to the user's questions and responds with answers that are grounded in the asserted facts.
- Internally multiple 'agents' work together to handle the user query:
  - **Voice Agent**: Connects to the OpenAI realtime API. This enables the user to have a conversation about phone plans with the voice model. From this conversation, the voice model generates natural language queries and sends them to the Voice Agent as tool calls. Voice Agent forwards the queries to other agents. Tool call results (when obtained from other agents) are sent back to the voice model which then conveys them to the user via audio.
  - **CodeGen Agent**: Translates natural language queries into Prolog queries via the Anthropic API and then uses the *Tau* Prolog engine to 'solve' for answers against the facts.
  - **Query Agent**: Uses the Prolog engine to run simple 'canned' Prolog queries.
  - **App Agent**: Monitors the communications between other agents and reports them to the UI.

The **RTFlow** framework provides the agent hosting and communication services. The following diagram shows the RTFlow agent arrangement for the RT.Assistant sample:

![RTFlow](/src/Samples/PlanSelection/PlanAssistant/imgs/RTflow.png)

As there are multiple frameworks / technologies in play here. Lets briefly delve into each one of them - in the order of perceived importance.

## 1. RTFlow
RTFlow is a framework for building realtime 'agentic' applications. There are three main parts: `Flow`, `Bus` and `Agents`.

The `Bus` connects the `Agents` and the `Flow` together. All agent messages are broadcast, i.e. all agents receive any agent-intent message thrown on the bus. The `Flow` has its own separate channel.

`Flow` and `Agents` each maintain their own internal states and communicate via strongly-typed asynchronous messages. The message 'types' (for agents/flow) are F# [discriminated unions (DUs)](https://learn.microsoft.com/en-us/dotnet/fsharp/language-reference/discriminated-unions) which are custom designed for the flow implementation.

`Flow` is essentially an asynchronous state machine. The state transitions are triggered by messages arriving on the flow input channel. The transitions are deterministic and provide a way to control the overall system to the level desired.

Depending on the application, the `Flow` can be a minimal 3-state [`start`, `run`, `terminate`] machine where the agents mostly interact with each other - without involving the flow much. Contrarily, when more control is required, the agents mostly communicate to the `Flow` which then orchestrates the agents according to the states the flow is in. In terms of [topologies used to describe agent connections](https://vuir.vu.edu.au/652/1/Zhang_H-TopologicalClassfication.pdf), RTFlow could be considered a hybrid 'bus-star'.

The F# language offers a clean way to model asynchronous state machines (or more precisely [Mealy machines](https://www.tutorialspoint.com/automata_theory/moore_and_mealy_machines.htm)) where the states are functions and transitions happen via pattern matching over messages (DUs) or with ['active patterns'](https://zetcode.com/fsharp/active-patterns/). In the snippet below `s_XXX` are functions as states and `M_xxx` are messages that arrive on the `Bus`. The structure [`F`](/src/RTFlow/Workflow.fs#L76) packages the next state along with any output messages to be sent to agents.

```F#
let rec s_start msg = async {
  match msg with 
  | M_Start -> return F(s_run,[M_Started]) //transition to run
  | _       -> return F(s_start,[]) //stay in start state
  }

and s_run msg = async {
  match msg with 
  | M_DoSomething -> do! doSomething()
                     return F(s_run,[M_DidSomething])                     
  | M_Terminate   -> return F(s_terminate,[])
  | _             -> return F(s_run,[])
}

and s_terminate msg = async {
...
```

LLMs are inherently non-deterministic. RTFlow offers a way to control non-determinism to keep the overall system stable. As applications move from being human-centric to being more autonomous, we will need increasingly sophisticated methods to manage non-determinism. RTFlow's approach is to inject a deterministic state machine in the mix to effect such control.

Given the relatively simple building blocks of RTFlow, we can construct rich agentic systems that can support many realtime needs with the ability to dial-in the desired degree of control.

## 2. RTOpenAI
RTOpenAI wraps the OpenAI realtime voice API for native mobile(+) apps. Its two key features are a) support for the [WebRTC](https://webrtc.org/) protocol; and b) **strongly-typed realtime protocol messages**. These are discussed next.

### WebRTC
The OpenAI voice API can be used via Web Sockets or WebRTC, where WebRTC has some key advantages over the other;

- Firstly, WebRTC was designed for bidirectional, realtime communication. It has built-in resiliency for minor network disconnects - which crucially Web Sockets does not.

- WebRTC has separate channels for voice and data. (Also video - which is not currently used). This means that typically the application only needs to handle the data channel explicitly. The in/out audio channels are wired to the audio hardware by the underlying WebRTC libraries. In the case of Web Sockets, the application explicitly needs to handle in/out audio as well as the data.

- WebRTC transmits audio via the OPUS codec that has excellent compression but also retains good audio quality. For Web Sockets multiple choices exist. High quality audio is sent as uncompressed 24KHz [PCM](https://www.tutorialspoint.com/digital_communication/digital_communication_pulse_code_modulation.htm) binary as base64 encoded strings. The bandwidth required is 10X that for OPUS. There other telephony formats available but the audio quality drops significantly.

### Strongly-Typed Event Handling
The [RTOpenAI.Events](/src/RTOpenAI.Events/Events.fs#L938) library attempts to define F# types for all OpenAI realtime API protocol messages (that are currently documented). 

Additionally, the server (and client) messages are wrapped in DUs, which is convenient for consuming applications; incoming events can be handled with simple pattern matching. After the realtime connection is established, there is a steady flow of incoming events from the server that the application needs to accept and handle. The following snippet is an impressionistic version of how the [Voice Agent](/src/Samples/PlanSelection/PlanAssistant/Agents/VoiceAgent.fs#L123) handles server events:

```F#          
let handleEvent (ev:ServerEvent) = async {
  match ev with
  | SessionCreated                                   -> ...
  | ResponseOutputItemDone ev when isFunctionCall ev -> ...
  | _                                                -> ... //choose to ignore
}
```

The RTOpenAI library is a cross-platform *Maui* (see next) library and as such supports realtime voice applications for IOS, MacOS, Android and Windows.

## 3. Fabulous Maui Controls
[*Microsoft Maui*](https://dotnet.microsoft.com/en-us/apps/maui) is a technology for building cross-platform native apps. The F# library [*Fabulous.MauiControls*](https://github.com/fabulous-dev/Fabulous.MauiControls) is a way to build Maui apps with the F# language.

Fabulous is a [functional-reactive](https://reactiveweb.org/what-is-functional-reactive-programming-an-overview/) UI framework (influenced by [Elm](https://elm-lang.org/) and [React](https://react.dev/)). 

Fabulous is a joy to use. UI's can be defined declaratively in simple and understandable F#. UI 'events' are messages, which again are F# DU types that are 'handled' with pattern matching. In the simplest case, *events* update the application state, which is then rendered by Fabulous on to the screen.

Fabulous for Maui has a rich feature set, which cannot be fully covered here but the *Counter App* sample is replicated below to provide some sense of the library is used:

```F#
/// A simple Counter app

type Model =         //application state
    { Count: int }

type Msg =           //DU message types
    | Increment
    | Decrement

let init () =
    { Count = 0 }

let update msg model = //function to handle UI events/messages
    match msg with
    | Increment -> { model with Count = model.Count + 1 }
    | Decrement -> { model with Count = model.Count - 1 }

let view model =  
    Application(
        ContentPage(
            VStack(spacing = 16.) {                     //view
                Image("fabulous.png")

                Label($"Count is {model.Count}")

                Button("Increment", Increment)
                Button("Decrement", Decrement)
            }
        )
    )
```

The RT.Assistant is a *Maui* application and so the project structure is defined by Maui. Its a single project that targets multiple platforms. Components specific to each target platform are under the *Platforms* folder:
```
/RT.Assistant
  /Platforms 
    /Android
    /IOS
    /MacCatalyst
    /Windows
```

The platform specific folders contain the native-app required components (plists, app manifests, etc.). For example, here is the IOS [plist](/src/Samples/PlanSelection/PlanAssistant/Platforms/iOS/Info.plist).

RT.Assistant application code is 90% shared across platforms. However platform-specific libraries are required when interfacing with hardware that Maui does not cover. For WebRTC, RTOpenAI uses platform-native libraries with [Native Library Interop](https://devblogs.microsoft.com/dotnet/native-library-interop-dotnet-maui/). The [IOS WebRtc binding library](/src/WebCme/WebRTCme.Bindings.Maui.iOS/WebRTCme.Bindings.Maui.iOS.csproj) wraps the WebRTC.xcframework written in C++. And [for Android](/src/WebCme/WebRTCme.Bindings.Maui.Android/WebRTCme.Bindings.Maui.Android.csproj) the native libwebrtc.arr Android Archive is wrapped.

Since most mobile apps have both IOS and Android versions, as such, Maui makes a lot of sense. Instead of maintaining multiple code bases and dev teams, with Maui one can maintain a single code base with 90% shared code across platforms. And unlike other mobile platforms (e.g. React Native), Maui apps are proper native apps. For example, it would be problematic to host a realtime multi-agent systems like RTFlow in a JavaScript-based system like React Native.

## 4. Prolog for RAG

To make the sample somewhat fun and interesting, I decided to use Prolog-based ['RAG'](https://aws.amazon.com/what-is/retrieval-augmented-generation/). Generative AI meets Symbolic AI.

Prolog is a language for logic programming that was created almost 50 years ago. It has endured well even till today. The best known open source implementation is [SWI- Prolog](https://github.com/SWI-Prolog/swipl). However here I am using the much lighter weight [Tau](https://github.com/tau-prolog/tau-prolog) Prolog engine that runs in the browser. 

Fortunately web content can be easily hosted in Maui apps via the [HybridWebViewControl](https://learn.microsoft.com/en-us/dotnet/maui/user-interface/controls/hybridwebview?view=net-maui-10.0). In RT.Assistant there is a hidden web view that loads the Tau engine and the ['plan' clauses or facts](/src/Samples/PlanSelection/PlanAssistant/Resources/Raw/wwwroot/plan_clauses.pl).

### Prolog Representation

The richness of plan offerings make it sound like we need a rules engine to make sense of the plans/pricing/promotions data - which is exactly where Prolog comes in. The valid combination of plans, features and pricing can be expressed as logical facts where any ambiguity is eliminated. 

Prolog is a declarative language for first-order logic programming.
A Prolog 'database' consists of facts (e.g. plans and their features) and rules (to derive new facts from existing ones). A Prolog implementation will find any and all possible solutions that satisfy a query, given a set of facts and rules.

The general schema for the plan and its features is in [plan_schema.pl](/src/Samples/PlanSelection/PlanAssistant/Resources/Raw/plan_schema.pl):
```prolog
plan(title,category,lines,features)
% where each feature may have its own attribute set
```
An example fact for the 'Connect' plan is given below:

```Prolog
plan(
    "Connect",
    category("all"),
    [
        line(1, monthly_price(75),  original_price(80)),
        line(2, monthly_price(130), original_price(140)),
        ...
    ],
    features([

        feature(
            netflix(
                desc("Netflix Standard with Ads On Us"),
                included(yes)
            ),
            applies_to_lines(lines(2, 2))
        ),

        feature(
            autopay_monthly_discount(
                desc("$5 disc. per line up to 8 lines w/AutoPay & eligible payment method."),
                discount_per_line(5),
                lines_up_to(8),
                included_in_monthly_price(yes)
            ),
            applies_to_lines(all)
        ),
        ...

    ])
).

```

> While the above Prolog fact may seem complex, the same rules expressed in a relational database schema would be far more complex to understand and query. The *metadata* (columns,tables,relations) required to represent the rules and facts will be far greater than what is required for Prolog.


### Query Processing

While we can obtain an answer by prompting the LLM with text descriptions of the plans along with the query, there is a sound reason for not doing so. LLMs are not perfect and can make mistakes. And here we desire a more precise answer. So, instead we transform the natural language user query into an equivalent Prolog query - with the help of an LLM. It is surmised that the reformulation of the question is easier for the LLM, i.e. the LLM is less likely to hallucinate compared to the case of generating the answer directly. For direct answer generation, the LLM will need to sift through a much larger context - the entire plan database as plain text. For query generation, the LLM need only look at the database 'schema' - which is much more compact in the case of Prolog.

If query transformation goes awry then the Prolog query may fail entirely or produce strange results. Either way the user will be alerted and will not rely on the results to make a decision. If on the other hand, the answer is generated directly, a hallucination may subtly alter or miss facts. The user is likely to accept it without questioning because the answer looks plausible. This is a more egregious error. 

### Example Queries

Below are some typical questions that can be asked:
- What categories of plans are available?
- What is the lowest cost plan for 4 lines?
  - Follow up: Does this plan include taxes and fees in the price?
- Are there any special plans for military veterans?
  - Follow up: What is the amount of mobile hotspot data available for this plan?

The RT.Assistant application shows the natural language query; the generated Prolog; and the Prolog query results on the UI in realtime. 

Example:

- #### Natural language query generated by voice model from conversation: 
```
Find the plans in the category 'military_veteran' for 2 lines and list their costs.
```
- #### Prolog query:
```Prolog
plan(Title,
     category(military_veteran),
     lines(Lines),
     _),
member(line(2, monthly_price(Price), _), Lines).
```
> Note: In Prolog, uppercase-starting variables are 'free' variables that can be bound to values. For example, 'Title' above will bind to each plan title, respecting other constraints. One obvious constraint is 'category=military_veteran' so only Military Veteran plans will be considered.
- #### Results:
```
Title = Connect Next Military, Lines =
[line(1,monthly_price(85),original_price(90)),
 line(2,monthly_price(130),original_price(140)),
 line(3,monthly_price(165),original_price(180)),
 line(4,monthly_price(200),original_price(220)),
 line(5,monthly_price(235),original_price(260))],
Price = 130
[...]
  ```

 If a Prolog error occurs, the system regenerates the Prolog query but this time includes the Prolog error message along with the original query. This cycle may be repeated up to a limit.

### Prolog Code Generation and Results
With current models - Claude *Sonnet 4.5* for Prolog code generation and *gpt-realtime* for realtime voice, the results are quite satisfactory. This was not the case about a year. This approach was not workable when I first started testing it in late 2024. But now (late 2025) the models have advanced enough for this approach to be viable.















