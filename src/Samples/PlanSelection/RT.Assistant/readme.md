### RT.Assistant Sample

This sample is a  voice-enabled assistant that performs question-answering over a
'database' of mocked (but representative) phone plans (that may be available from a
typical major telecom company).

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

















