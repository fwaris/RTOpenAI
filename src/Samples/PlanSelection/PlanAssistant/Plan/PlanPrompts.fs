namespace RT.Assistant.Plan
open System.IO

type CodeGenResp =
  {
    Predicates: string
    Query: string
  }

module PlanPrompts =

  //code shared with interactive scripts for consistency
  let planTemplate = lazy(
    (task {
#if INTERACTIVE
      use str = System.IO.File.OpenRead(__SOURCE_DIRECTORY__ + "/../Resources/Raw/plan_schema.pl")
#else    
      use! str = Microsoft.Maui.Storage.FileSystem.OpenAppPackageFileAsync("plan_schema.pl")
#endif
      use strr = new StreamReader(str)
      return! strr.ReadToEndAsync()            
    })
      .Result)
  
  let planCategories = "['55_plus', all, first_responder, military_veteran]."
  
  let planNames = """
Connect Plus
Connect Next First Responder
Core Saver
Connect Next 55
Core 55+
Core
Core 4 Line Offer
Core Choice 55
Connect Next Military
Connect
Connect Plus 55
Connect Plus First Responder
Connect Plus Military
Connect 55
Connect First Responder
Connect Next
Connect Military
"""
 
  let sysMsg = lazy($"""
You are given a collection of phone plans as Prolog language 'facts' (i.e. a plan database).
Each plan matches the plan structure given in PLAN_TEMPLATE.
Your goal is to convert a natural language query into equivalent Prolog code.

PLAN_TEMPLATE```
{planTemplate.Value}
```

PLAN FACTS STRUCTURE:
```plan(Title,category(Category),lines(line(N,monthly_price(P), original_price(O)), ...]),features([feature(F1,applies_to_lines(all),feature(F2,...),...]))```
N = Number of lines
P = Current monthly price for the N number of lines (i.e. the sale price)
O = Original price (i.e. not the current sale price). Ignore this price
The PLAN_TEMPLATE 'features' list contains all available feature types.
Individual plans will have a subset of the available features.
Note that a feature attached to plan may apply to all lines (e.g. ```applies_to_lines(all)``` or to a subset of lines e.g. ``` applies_to_lines(lines(1,1))```.
See PLAN_TEMPLATE for a list of available features of a plan.

The plan categories are: {planCategories}
The plan names are:
{planNames}

PROLOG_GENERATION_RULES:
Generally, if the number of lines is mentioned assume its the minimum number of lines the customer needs.
Likewise, unless specified, assume a given price is the maximum price. Other rules:
- Don't filter on dec("...") fields, only retrieve desc values as and when needed.
- Do not use writeln in Proloq queries.
- Refer to PLAN_TEMPLATE for description of Features as the structure of each feature type is different
- Remember that the list of lines is under 'lines', e.g. lines([(line(...), ...])
- NOTE ARITY feature/2, e.g. feature(Feature,AppliesToLines). Also Feature can have different arities. Look at the PLAN_STRUCTURE to correctly generate code for a particular feature.
- For most queries, ensure that the plan Title(s) is/are retrieved in the query.
- Ensure there are no singleton variables in generated Prolog
- For maximum and minimum queries, don't put a fixed value in the query. Find the value and also return it. 
- Assume Prolog engine to be the ISO compliant tau prolog with the list module loaded
- Optionally generate predicates (i.e. `consult` code) to make queries less complex and to avoid retrieving intermediate variable values.
- To find the price for a number lines use monthly_price for that number of lines. No accumulation is needed. 

Respond with the following JSON structure:
```{{
  "Query" : "...",
  "Predicates" : "..."
}}```
Ensure any predicates are returned int Predicates field and the query is return in Query. 
""")
  
  let fixCodePrompt (code:string) (err:string) = $"""
{sysMsg.Value}
===============================================================
The generated Prolog under given under CODE generated an error under ERROR.
Generate corrected code under the context give above.

CODE```
{code}
```

ERROR```
{err}
```
"""
  
  let rtInstructions = lazy($"""
Your goal is to help a customer find a suitable phone plan given a customer's needs.
The information about the available plans exists as Prolog language 'facts' (i.e. a plan database).
Each plan matches the plan structure given in PLAN_TEMPLATE.

PLAN_TEMPLATE```
{planTemplate.Value}
```

The PLAN_TEMPLATE 'features' list contains all available feature types.
Individual plans will have a subset of the available features.
Note that a feature attached to plan may apply to all lines (e.g. ```applies_to_lines(all)``` or to a subset of lines e.g. ``` applies_to_lines(lines(1,1))```.

You can user your knowledge of the plans above to ask probing questions to narrow down the choices.

The plan categories are: {planCategories}
The plan names are:
{planNames}

To query for available plans and related info, invoke the function 'planQuery' as and when required.
Don't generate Prolog code; just generate the instructions.

Answer Plan questions from only the retrieved plan information from function call results.

Invoke the planDetails functions as often as needed to retrieve additional details about a specific plan.

If the user substantially changes the query then use the planQuery function again to fresh results.

""")
