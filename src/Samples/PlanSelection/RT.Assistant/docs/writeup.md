# RT.Assistant - a realtime, multi-agent voice bot

This sample is a voice-enabled assistant that allows one to determine the best phone plan for ones needs from a set of available plans. The assistant can be conversed with via voice to discover the types of plans & features available and their prices, to make a selection. 

---
> [YouTube: Overview](https://youtu.be/bSMByJvYLoY) | [YouTube: Code Walkthrough](https://youtu.be/0ghPhQyzyaI) | [root readme.md](/readme.md) | [LinkedIn](https://www.linkedin.com/posts/activity-7410082754836725761-lquf?utm_source=share&utm_medium=member_desktop&rcm=ACoAAAAbaagBCG-0LlGBjghxmo7KKzbEXRHmiZ0)
---


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

Additionally, the available features may be dependent on the number of lines (distinct phone numbers). For example, Netflix may be *excluded* for a single line but *included* for two or more lines. 

### Technologies Showcased
The system internally maintains a mocked—but representative—set of phone plans, modeled as Prolog facts, simulating offerings from a typical major telecom provider. From (a) capturing the voice input, to (b) querying the Prolog knowledge base, and finally (c) generating the results, multiple components work together seamlessly.

This sample highlights the integration of the following frameworks and technologies:
- **RTFlow**: Multi-agent framework for realtime GenAI applications - written in F#
- **RTOpenAI**: F# library for interfacing with the OpenAI realtime API via the WebRTC protocol
- **Fabulous** for Maui: F# library for building native mobile applications (IOS, Android, +) with Microsoft Maui
- **Tau**: JavaScript-based Prolog engine and its use in a native mobile app via the Maui HybridWebView control.
- Integration with the OpenAI & Anthropic 'chat' APIs for Prolog code generation with the **Microsoft.Extensions.AI** library

## Overview
There is a lot going on here: *generative AI*; *old-school symbolic AI*; *multi-agents*; *realtime voice*; *cross-platform native mobile apps*; to name some. The following explains how these are all stitched together into a comprehensive system.

- Voice-enabled interaction: The assistant allows users to ask questions about phone plans through natural speech, making the experience conversational and intuitive.

- Structured knowledge base: Plan details are represented as logically consistent Prolog facts, ensuring clarity and eliminating ambiguity in feature–price combinations.

- Internally, multiple specialized agents work together to process and respond to user queries:

    - **Voice Agent** – Maintains a connection to the OpenAI realtime 'voice' API, enabling natural voice conversations about phone plans. It handles the steady stream of messages from the API, include tool calls containing natural language queries. These queries are then routed to other agents, and the resulting answers are returned to the voice model, which conveys them back to the user in audio form.

    - **CodeGen Agent** – Converts natural language queries into Prolog statements using another LLM API, then leverages the Tau Prolog engine to evaluate those statements against the knowledge base of facts.

    - **Query Agent** – Executes predefined (“canned”) Prolog queries directly against the Prolog engine for quick, structured lookups.

    - **App Agent** – Oversees communication among agents and reports activity back to the user interface for transparency and monitoring.

All agents are orchestrated by the **RTFlow** framework, which provides hosting and communication services. The diagram below illustrates the RTFlow agent arrangement for the RT.Assistant sample:

![RTFlow](/src/Samples/PlanSelection/RT.Assistant/imgs/RTflow.png)

As there are multiple frameworks / technologies in play here. Lets briefly delve into each one of them - in the order of perceived importance.



## 1. RTFlow

RTFlow is a framework for building real-time, *agentic* applications. It is composed of three primary elements: **Flow**, **Bus**, and **Agents**.

### Bus

The **Bus** provides the communication substrate that connects **Agents** to one another and to the **Flow**. It exposes two distinct logical channels:

- **Agent broadcast channel**  
  All agent-intent messages published to this channel are broadcast to *all* agents. 

- **Flow input channel**  
  Messages published to this channel are delivered exclusively to the **Flow**. Agents do not receive these messages.

This separation allows agent collaboration to occur independently of system-level orchestration, while still enabling agents to explicitly signal the Flow when required.

### Messages and State

Both **Flow** and **Agents** maintain private internal state and communicate exclusively via strongly typed, asynchronous messages. Message 'schemas' are defined as F#  [*discriminated unions (DUs)*](https://learn.microsoft.com/en-us/dotnet/fsharp/language-reference/discriminated-unions) types and are fixed at implementation time, providing:

- Compile-time exhaustiveness checking  
- Explicit modeling of intent and system events  
- Clear separation between agent-level and flow-level concerns

### Flow

The **Flow** is an asynchronous, deterministic state machine. Its state transitions are triggered solely by messages arriving on the Flow input channel.

Depending on application requirements, the Flow can range from minimal to highly directive:

- **Minimal control**  
  A simple lifecycle state machine (e.g., `Start → Run → Terminate`), where agents primarily interact with each other via the broadcast channel and the Flow plays a supervisory role.

- **Orchestrated control**  
  A more granular state machine in which agents primarily communicate with the Flow, and the Flow explicitly coordinates agent behavior based on its current state.

This design allows system-level determinism and control to be introduced incrementally, without constraining agent autonomy where it is unnecessary.

### Topology

From a multi-agent systems perspective, RTFlow employs a hybrid **bus–star** [topology](https://vuir.vu.edu.au/652/1/Zhang_H-TopologicalClassfication.pdf):

- The **Bus** enables broadcast-based, peer-style agent communication.
- The **Flow** acts as a central coordinating node when orchestration is required.

This hybrid model balances scalability and decoupling with deterministic system control.

The F# language offers a clean way to model asynchronous state machines (or more precisely [Mealy machines](https://www.tutorialspoint.com/automata_theory/moore_and_mealy_machines.htm)) where the states are functions and transitions happen via pattern matching over messages (DUs) or with ['active patterns'](https://zetcode.com/fsharp/active-patterns/). In the snippet below `s_XXX` are functions as states and `M_xxx` are messages that arrive on the `Bus`. The structure [`F`](/src/RTFlow/Workflow.fs#L35) packages the next state along with any output messages to be sent to agents.

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

Given the relatively simple building blocks of RTFlow, we can construct rich agentic systems that can support many realtime needs with the ability to dial-in the desired degree of control, when needed.

## 2. RTOpenAI
RTOpenAI wraps the OpenAI realtime voice API for native mobile(+) apps. Its two key features are a) support for the [WebRTC](https://webrtc.org/) protocol; and b) **strongly-typed realtime protocol messages**. These are discussed next.

### WebRTC
The OpenAI voice API can be used via Web Sockets or WebRTC, where WebRTC has some key advantages over the other;

- Firstly, WebRTC was designed for bidirectional, realtime communication. It has built-in resiliency for minor network disconnects - which crucially Web Sockets does not.

- WebRTC has separate channels for voice and data. (Also video - which is not currently used). This means that typically the application only needs to handle the data channel explicitly. The in/out audio channels are wired to the audio hardware by the underlying WebRTC libraries. In the case of Web Sockets, the application explicitly needs to handle in/out audio as well as the data.

- WebRTC transmits audio via the OPUS codec that has excellent compression but also retains good audio quality. For Web Sockets multiple choices exist. High quality audio is sent as uncompressed 24KHz [PCM](https://www.tutorialspoint.com/digital_communication/digital_communication_pulse_code_modulation.htm) binary as base64 encoded strings. The bandwidth required is 10X that for OPUS. There other telephony formats available but the audio quality drops significantly.

### Strongly-Typed Event Handling
The [RTOpenAI.Events](/src/RTOpenAI.Events/Events.fs#L925) library attempts to define F# types for all OpenAI realtime API protocol messages (that are currently documented). 

Additionally, the server (and client) messages are wrapped in DUs, which is convenient for consuming applications; incoming events can be handled with simple pattern matching. After the realtime connection is established, there is a steady flow of incoming events from the server that the application needs to accept and handle. The following snippet is an impressionistic version of how the [Voice Agent](/src/Samples/PlanSelection/RT.Assistant/Agents/VoiceAgent.fs#L155) handles server events:

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
[*Microsoft Maui*](https://dotnet.microsoft.com/en-us/apps/maui) is a technology for building cross-platform native apps. The F# library [*Fabulous.MauiControls*](https://github.com/fabulous-dev/Fabulous.MauiControls) enables building of Maui apps in F#.

Fabulous is a [functional-reactive](https://reactiveweb.org/what-is-functional-reactive-programming-an-overview/) UI framework (influenced by [Elm](https://elm-lang.org/) and [React](https://react.dev/)). 

Fabulous is a joy to use. UI's can be defined declaratively in simple and understandable F#. UI 'events' are messages, which again are F# DU types that are 'handled' with pattern matching. In the simplest case, *events* update the application state, which is then rendered by Fabulous on to the screen.

Fabulous for Maui has a rich feature set, which cannot be fully covered here but the *Counter App* sample is replicated below to provide some sense of how the library works:

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

The platform specific folders contain the native-app required components (plists, app manifests, etc.). For example, here is the IOS [plist](/src/Samples/PlanSelection/RT.Assistant/Platforms/iOS/Info.plist).

RT.Assistant application code is 90% shared across platforms. However platform-specific libraries are required when interfacing with hardware that Maui does not cover. For WebRTC, RTOpenAI uses platform-native libraries with [Native Library Interop](https://devblogs.microsoft.com/dotnet/native-library-interop-dotnet-maui/). The [IOS WebRtc binding library](/src/Bindings/FsWebRTC.Bindings.Maui.iOS/FsWebRTC.Bindings.Maui.iOS.csproj) wraps the `WebRTC.xcframework` written in C++. And [for Android](/src/Bindings/FsWebRTC.Bindings.Maui.Android/FsWebRTC.Bindings.Maui.Android.csproj) the native `libwebrtc.arr` Android Archive is wrapped.

Since most mobile apps have both IOS and Android versions, as such, Maui makes a lot of sense. Instead of maintaining multiple code bases and dev teams, with Maui one can maintain a single code base with 90% shared code across platforms. And unlike other mobile platforms (e.g. React Native), Maui apps are proper native apps. For example, it would be problematic to host a realtime multi-agent systems like RTFlow in a JavaScript-based system like React Native.

## 4. Prolog for RAG

To make the sample somewhat fun and interesting, I decided to use Prolog-based ['RAG'](https://aws.amazon.com/what-is/retrieval-augmented-generation/). Generative AI meets Symbolic AI.

Prolog is a language for logic programming that was created almost 50 years ago. It has endured well even till today. The best known open source implementation is [SWI- Prolog](https://github.com/SWI-Prolog/swipl). However here I am using the much lighter weight [Tau](https://github.com/tau-prolog/tau-prolog) Prolog engine that runs in the browser. 

Fortunately web content can be easily hosted in Maui apps via the [HybridWebViewControl](https://learn.microsoft.com/en-us/dotnet/maui/user-interface/controls/hybridwebview?view=net-maui-10.0). In RT.Assistant there is a [hidden web view](/src/Samples/PlanSelection/RT.Assistant/Views/Chat.fs#L105) that loads the Tau engine and the [*plan facts*](/src/Samples/PlanSelection/RT.Assistant/Resources/Raw/wwwroot/plan_clauses.pl).

### Prolog Representation

The typical phone plans from the major telecoms are 'rich' offerings. The interplay of base plans, number of lines, features and promotions suggest a rules engine based approach. This is precisely where Prolog excels. By representing valid combinations of plans, features, and pricing as logical facts, Prolog ensures consistency and removes ambiguity.

Prolog is a declarative language for first-order logic programming.
A Prolog 'database' consists of facts (e.g. plans and their features) and rules (to derive new facts from existing ones). A Prolog implementation will find any and all possible solutions that satisfy a query, given a set of facts and rules.

The 'schema' for the plan and its features is in [plan_schema.pl](/src/Samples/PlanSelection/RT.Assistant/Resources/Raw/plan_schema.pl). The skeletal form is:
```prolog
plan(title,category,prices,features)
% where each feature may have a different attribute set
```

An partial fact for the 'Connect' plan is given below:

```Prolog
plan(
    "Connect",
    category("all"),
    prices([
      line(1, monthly_price(20), original_price(25)),
      line(2, monthly_price(40), original_price(26)),
      ...
    ]),
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

> The [full Prolog fact](/src/Samples/PlanSelection/RT.Assistant/Resources/Raw/plan_schema.pl) may seem complex, however the same rules expressed in a relational database schema would be far more complex to understand and query. The *metadata* (columns,tables,relations) required to represent the rules and facts will be far greater than what is required under Prolog.


### Query Processing

While we can obtain an answer by prompting the LLM with text descriptions of the plans along with the query, there is a sound reason for not doing so. LLMs are not perfect and can make mistakes. And here we desire a more precise answer. So, instead we transform the natural language user query into an equivalent Prolog query - with the help of an LLM. It is surmised that the reformulation of the question is easier for the LLM, i.e. the LLM is less likely to hallucinate compared to the case of generating the answer directly. For direct answer generation, the LLM will need to sift through a much larger context - the entire plan database as plain text. For query generation, the LLM need only look at the database 'schema' - which is much more compact, especially in the case of Prolog.

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
     prices(Lines),
     _),
member(price(2, monthly_price(Price), _), Lines).
```
> Note: In Prolog, uppercase-starting names are 'free' variables that can be bound to values. For example, 'Title' above will bind to each of the plan titles for the found solutions. A solutions satisfies all constraints. One obvious constraint is 'category=military_veteran' so only Military Veteran plans will be considered.
- #### Results:
```
Title = Connect Next Military, Lines =
[line(1,monthly_price(85),original_price(90)),
 line(2,monthly_price(130),original_price(140)),
 line(3,monthly_price(165),original_price(180)),
 line(4,monthly_price(200),original_price(220)),
 line(5,monthly_price(235),original_price(260))],
Price = 130

Title = Core, Lines = ...
  ```

 If a Prolog error occurs, the system regenerates the Prolog query but this time includes the Prolog error message along with the original query. This cycle may be repeated up to a limit.

### Prolog Code Generation and Results
For code generation, the application allows for a choice between Claud Sonnet 4.5 and GPT 5.1 (via the app Settings). The GPT Codex model was also tested but there the latency is too high for realtime needs.

For this particular task, GPT-5.1 has the clear edge, generating code that produces concise and relevant output. See [this analysis](/src/Samples/PlanSelection/RT.Assistant/docs/prologgen.md) for more details.

FTW, both models generate syntactically correct Prolog 99% of the time. (A retry loop corrects generated errors, if any.)

For question-answering, the OpenAI realtime model generates satisfactory answers to user queries from the generated Prolog output. Note that for any real production system there should be a well-crafted 'eval' suite to truly gauge the performance.

















