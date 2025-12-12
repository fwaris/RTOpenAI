# RT.Assistant Sample

This sample is a voice-enabled assistant that provides question-answering ability over a
'database' of mocked (but representative) phone plans that may be available from a
typical major telecom.

## This sample demonstrates: 
- Use of the RTFlow - a multi-agent framework for realtime applications - written in F#
- Integration with the OpenAI realtime voice API over the WebRTC protocol via the RTOpenAI framework - also written in F#
- Native mobile applications (IOS & Android) built with Microsoft Maui and Fabulous (a F# library for Maui controls)
- Use of the Maui HybridWebView to host a Javascript-based Prolog Engine in a native mobile app
- Integration with the Anthropic API for Prolog code generation

## Overview
There is a lot going on here: *generative AI*; *old-school symbolic AI*; *multi-agents*; *realtime voice*; *cross-platform native mobile apps*; to name some. The following explains how these are all stitched together into a comprehensive system.

- Firstly, this is a *voice-enabled* assistant so the user can talk to the assistant to get questions answered about phone plans.
  - Note that contemporary plans are complex offerings with bundled products and services. It requires some effort to ascertain the best plan for one's needs.
- The plan information is stored as set of Prolog *facts*.
- The assistant listens to the user's questions and responds with answers which are grounded in the asserted facts.
- Internally multiple agents work together to handle the user query:
  - **Voice Agent**: Connects to the OpenAI realtime API and handles the user queries given to it as tool calls from the voice model.
    - This agent also wires audio from the realtime API to the device's hardware using platform-native WebRTC libraries and the RTOpenAI wrapper framework.  
  - **CodeGen Agent**: Translates user queries into Prolog queries via the Anthropic API and then uses the *Tau* Prolog engine to 'solve' for answers against the 'facts' database.
  - **Query Agent**: Uses the Prolog engine to run simple 'canned' Prolog queries.
  - **App Agent**: Monitors the communications between other agents and reports them to the UI which is built with Maui+Fabulous.
- The agent hosting and inter-agent communication services are provided by the **RTFlow** framework.

The following diagram shows the RTFlow agent arrangement for the RT.Assistant sample:

![RTFlow](/src/Samples/PlanSelection/PlanAssistant/imgs/RTflow.png)

As there are multiple framework / technologies in play here, lets briefly delve into each one of them - in the order of perceived importance.

## 1. RTFlow
RTFlow is a framework for building realtime 'agentic' applications. There are three main parts: `Flow`, `Bus` and `Agents`.

The `Bus` connects the `Agents` and the `Flow` together. All agent messages are broadcast, i.e. all agents receive any agent-intent message thrown on the bus. The `Flow` has its own separate channel.

`Flow` and `Agents` each maintain their own internal states and communication via strongly-typed asynchronous messages. The message 'types' (for agents/flow) are F# discriminated unions (DU) which are custom designed for the flow implementation.

`Flow` is essentially an asynchronous state machine. The state transitions are triggered by messages arriving on the flow input channel. The transitions are deterministic and provide a way to control the overall system to the level desired.

Depending on the application, the `Flow` can be a minimal 3-state [`start`, `run`, `terminate`] machine where the agents mostly interact with each other - without involving the flow much. Contrarily, when more control is required, the agents mostly communicate to the `Flow` which then orchestrates the agents according to the states the flow is in.

LLMs are inherently non-deterministic. This approach offers a way to control non-determinism such that the overall system remains stable. As applications move from being human-centric to being more autonomous, we will need methods to better manage this non-determinism.

The F# language offers a clean way of modeling asynchronous state machines (or more precisely Mealy machines) where the states are functions and transitions happens via pattern matching over DUs or with 'active patterns'. In the snippet below `s_XXX` are functions as states and `M_xxx` are messages that arrive on the `Bus`. The structure `F` packages the next state along with any output messages for agents.

```F#
let rec s_start msg = async{
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
Given the relatively simple building blocks, we can construct rich agentic systems that can support many realtime needs. 


## 2. RTOpenAI
RTOpenAI wraps the OpenAI realtime voice API. Its two key features are a) support for the WebRTC protocol; and b) strongly-typed realtime protocol messages. These are discussed next.

### WebRTC
The OpenAI voice API can be used via Web Sockets and WebRTC (and now also SIP). Between Web Sockets and WebRTC, WebRTC has some key advantages:

- Firstly, WebRTC is meant for bidirectional realtime communication. It has built-in resiliency for minor network disconnects - which crucially Web Sockets does not.

- WebRTC has separate channels for voice and data. (Also video - which is not currently used). This means that typically the application only needs to  handle the data channel explicitly. The in/out audio channels are wired to the audio hardware by the underlying WebRTC libraries. In the case of Web Sockets, the application explicitly needs to handle in/out audio as well as the data.

- WebRTC transmits audio via the OPUS codec which offers high compression but retains good audio quality. For Web Sockets multiple choices exist. High quality audio is sent as uncompressed 24KHz PCM binary as base64 encoded strings. The bandwidth required is 10X that for OPUS. There other telephony formats available but the audio quality drops significantly.

### Strong Typing
The RTOpenAI.Events library attempts to define F# types for all OpenAI realtime voice protocol messages (that are currently documented). 

This makes is easier for consuming applications to handle incoming 'server' messages and to send out correctly formatted 'client' messages.

RTOpenAI currently works on IOS/MacOS and Android platfrom









The agent topology is relatively flat (although there is nothing stopping agents from having their own internal sub-agents). 


#### The types of questions that can be asked are:
- What categories of plans are available?
- What is the lowest cost plan for 4 lines?
  - Follow up: Does this plan include taxes and fees in the price?
- Are there any special plans for military veterans?
  - Follow up: What is the amount of mobile hotspot data available for this plan?

### Function Calling
The sample demonstrates the use of *function calling* with the realtime API. The basic function 
calling flow is as follows:

- The realtime API's 'session' object is updated (soon after its creation) to include
the description of available functions that the LLM may call.
  - Note that there are some other client event types where function descriptions may also be included (see realtime API documentation)
- User asks question
- Depending on the context, the server LLM may decide to call a function
  - If so, it will create any required parameters for the function to be called.
- Client receives a server event to actually call the function with the given parameters
- Client invokes the function and sends the function response to the server asynchronously
- Server sends the function results back to the client. The client knows that server has seen the response,
it may or may not do something with that (like update the UI).

Understand that the user can continue conversing with the model while the function call processing is occuring.

### Some Peculiarities
While we are lucky to be living in an era of unprecedented innovation, LLM voice-chat is very much an emergent
area; you can expect some quirkiness. Even the official API documentation is somewhat sparse. The patterns
for realtime API usage are still not fully clear (to me). In some cases the bot just stops and has to be prompted to
continue. In other cases, the voice jumps ahead discontinuously. Speech recognition may not be perfect,
the bot then has to be corrected.

### Prolog for 'RAG'
The predominant approach to question-answering involves prompting the LLM with natural 
language documentation alongside the user’s question. The LLM then derives the answer based on 
the provided context. While this is a highly simplified explanation, it remains conceptually accurate.
This method is known as Retrieval-Augmented Generation (RAG).

In RT.Assistant, the 'answer' is obtained from a Prolog query. The Prolog query results are handed to the
realtime API LLM, which then generates the speech response. 

How this exactly works and why it was done this way, is explained next.

#### RAG Flow
First the mechanics of what happens.

- User asks question "Find me the lowest cost plan for 3 lines".
- Realtime API LLM converts this query into a function call, after revising the query text based on
the chat history (if any).
- Client receives the function call message with query.
- Client converts the natural language query to a Prolog query by invoking another LLM (4o with fallback to o3-mini, for now) to
perform the code generation.
- Client executes the generated Prolog code via the Tau Prolog interpreter which runs in the javascript engine inside a Maui
HybridWebView control. The javascript code loads the plan 'database', which is just a set of Prolog 'facts' in a file, and then finds solutions to the query.
All of this happens locally in the mobile app.
- Client sends the query results to server which then generates a speech response from the results.

#### Why Prolog? 
Often product/pricing information can become complex. There can be many 'conditionalities'. For 
example a plan may include Netflix if 4 or more lines are purchased but not for lesser number of phone lines. 

Generally, pricing of bundled products and services can become complex as price analysts try to optimize the 
revenue / cost equations.

To enable a virtual assistant to answer product/pricing questions more precisely, the Prolog strategy
was adopted. To be clear, this is still experimental and unproven - more study is needed. The rationale here is
that it may be easier for an LLM to generate a precise formal query to answer the question than to answer the
same question directly from the text descriptions of products/pricing.

There are 17 plans with 1 to 5 lines possible for each and about 20 or so 'features'. While 
these number look small, the possible combinations can become quite large. And there
are other complexities, e.g. a feature (e.g. Apple TV) may be available for 6 months vs. for the 
duration of the purchased plan. In other words, features may have additional options, which increase the
number of possible combinations.

Modeling a structure this complex as a relational schema is generally impractical. 
Prolog, however, offers a more natural way to represent such intricate structures. 
With its ability to support nested structures, and recursive (graph) relations, 
Prolog allows for more intuitive modeling of information.
Therefore, using a combination of manual and LLM-based processing, the textual
descriptions of the plans were encoded as a 'database' of Prolog facts.

A prototype fact for a plan (used as a part of some prompts) can be seen [here](Resources/Raw/plan_schema.pl).

Prolog is a declarative language for first-order logic programming.
A Prolog 'database' consists of facts (e.g. plans and their features) and rules (to derive new facts
from existing ones). A Prolog implementation will find
any and all possible solutions that satisfy a query, given a set of facts and rules.

The language is over 50 years old so LLMs have likely ingested all openly available Prolog text and code. It is
expected that the selected LLM has enough comprehension of the subject area to be useful.
By useful, we mean that the LLM generates valid Prolog query code from a natual language query. The Prolog
query should capture the original query's intent in context of the plan database.

### Usage Experience
The app works as intended. The user can query for plans and ask follow up questions.

Still there are code generation issues that need to be further addressed. 

The generated Prolog code is logged (for introspection) and also posted to the UI.
The user can modify the code and test it from the UI and use this feedback to 
improve the code generation prompt.

In general, its better to fail than to produce an inaccurate answer. The Prolog-based approach
thus reduces hallucinations over 'normal' RAG.

















