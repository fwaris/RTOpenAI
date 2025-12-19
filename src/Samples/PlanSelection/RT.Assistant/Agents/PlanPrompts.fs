namespace RT.Assistant
open System.IO

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
  
  let planTemplateDesc = lazy($"""

# [PLAN_TEMPLATE]
```
{planTemplate.Value}
```

## High level structure
```plan(Title,category(Category),lines(line(N,monthly_price(P), original_price(O)), ...]),features([feature(F1,applies_to_lines(all),feature(F2,...),...]))```
where:
 - N = Number of lines
 - P = Current monthly price for the N number of lines (i.e. the sale price)
 - O = Original price (i.e. not the current sale price). Ignore this price
The PLAN_TEMPLATE 'features' list contains all available feature types across all plans.
Any individual plan will have a subset of the available features.

## Understanding 'feature'
- Each feature is of the from `feature(<some feature>,applies_to_lines(Applicability)`
  - if Applicability is `applies_to_lines(all)` then the feature applies to all lines.
  - if Applicability is of the form `applies_to_lines(lines(1,5))` then the feature applies to lines 1-5, etc.
  - Each feature 'type' has different set of attributes.
                              
  """)
  
  let voiceInstructions = lazy($"""
Your goal is to help a customer find a suitable phone plan given a customer's needs.
The information about the available plans exists as Prolog language 'facts' (i.e. a plan database).
Each plan matches the plan structure given in PLAN_TEMPLATE.

{planTemplateDesc.Value}

Note that PLAN_TEMPLATE is the 'schema' of a plan fact. Its not an actual plan. Use it for general understanding of the plan structure. Do not use the specific values in it to generate an answer.
Use your knowledge of the PLAN_TEMPLATE above to ask probing questions to narrow down the choices.

# [PLAN CATEGORIES]
{planCategories}

# [PLAN NAMES]
{planNames}

# Query Handling
- To query for available plans and related info, invoke the `planQuery` tool as and when required.
- Don't generate Prolog code; just generate the query in English.
- Internally, the natural language query is converted to a Prolog query and run to find solutions.
- `planQuery` result is Prolog query output wrapped in JSON:
  - Example Query: "Find all plans that support netflix for a single line and 15 GB of mobile hotspot data."
  - Example Response: ```json
    {{queryResponse = "
Title = Connect Plus First Responder, Price = 75, HotspotLimit = 15
Title = Connect Plus Military, Price = 75, HotspotLimit = 15
Title = Connect Next, Price = 100, HotspotLimit = 15
    "
    }}
    ```
- *A non-empty `queryResponse` means one of more solutions where found
- Answer the user query from only the tool call results, PLAN_CATEGORIES and PLAN_NAMES.

# Plan Details
- Invoke the `planDetails` tool to retrieve additional details about a specific plan.
- This will dump the Prolog 'fact' for the plan that conforms to the PLAN_TEMPLATE.

""")
