namespace RT.Assistant.WorkFlow

module CodeGenPrompts =
    let planTemplate = lazy($"""
You are given a collection of phone plans as Prolog language 'facts' (i.e. a plan database).
Each plan matches the plan structure given in PLAN_TEMPLATE.
Your goal is to convert a natural language query - given in the field `query` - into equivalent Prolog code.

{RT.Assistant.PlanPrompts.planTemplateDesc.Value}

[PLAN CATEGORIES]
```
{RT.Assistant.PlanPrompts.planCategories}
```
[PLAN NAMES]
```
{RT.Assistant.PlanPrompts.planNames}
```
 """)    
        
    let sysMsg = lazy($"""
{planTemplate.Value}

PROLOG_GENERATION_RULES:
- For most quantities specified in the query e.g., number of lines; data XX GB; etc. assume its the minimum desired, unless very explicitly specified otherwise.
- Likewise, unless specified, assume a given price is the maximum price.
- Don't filter on dec("...") fields, only retrieve desc values as and when needed.
- Do not use writeln in Proloq queries.
- Refer to PLAN_TEMPLATE for description of Features as the structure of each feature type is different.
- NOTE ARITY feature/2, e.g. feature(Feature,AppliesToLines). Also each Feature type can have different arities. Look at the PLAN_STRUCTURE to correctly generate code for a particular feature.
- To extent possible, generate Prolog that retrieves only the relevant information. Avoid retrieving information that has not be requested, unless its relevant in some way.
- Always retrieve the plan Title(s).
- Also retrieve the quantities and features mentioned in the English query, e.g. if hotspot data is mentioned, ensure that the Prolog query returns XX in `high_speed_data_limit_gb(XX)`
- When feasible, generate predicates (i.e. `consult` code) to make queries less complex and to avoid retrieving intermediate variable values.
- Ensure there are no singleton variables in generated Prolog
- In general, abstract any constants (e.g. number of lines, data GB, etc.) out of any generated Predicates. Make Predicates parameterized. Put constant values in the query.
- Assume Prolog engine to be the ISO compliant Tau prolog with the list module loaded.
- To find the price for a number lines use monthly_price for that number of lines. No accumulation is needed. 

Respond with the following JSON structure:
```{{
  "query" : "...",
  "predicates" : "..."
}}```
Ensure any predicates are returned in the Predicates field and the query is return in Query. 
""" )
      
    let fixCodePrompt (code:string) (err:string) = $"""{sysMsg.Value}
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
    let summarizationPrompt = lazy($"""
{planTemplate.Value}
Given the fields `query`, `generatedCode` and `prologResults`, summarize the results in relation to the query.
`query` is a natural language query from with the Prolog code was generated and evaluated.
 """)
