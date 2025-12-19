namespace RT.Assistant.WorkFlow

module CodeGenPrompts =
    let planTemplate = lazy($"""
You are given a collection of phone plans as Prolog language 'facts' (i.e. a plan database).
Each plan matches the plan structure given in PLAN_TEMPLATE.
Your goal is to convert a natural language query - given in the field `query` - into equivalent Prolog code.

{RT.Assistant.PlanPrompts.planTemplateDesc.Value}

[PLAN_TEMPLATE]: ```
{RT.Assistant.PlanPrompts.planTemplate.Value}
```

[PLAN FACTS STRUCTURE]:
```plan(Title,category(Category),lines(line(N,monthly_price(P), original_price(O)), ...]),features([feature(F1,applies_to_lines(all),feature(F2,...),...]))```
where:
 - N = Number of lines
 - P = Current monthly price for the N number of lines (i.e. the sale price)
 - O = Original price (i.e. not the current sale price). Ignore this price
The PLAN_TEMPLATE 'features' list contains all available feature types.
Individual plans will have a subset of the available features.

Note that a feature attached to plan may apply to all lines (e.g. `applies_to_lines(all)` or to a subset of lines e.g. ` applies_to_lines(lines(1,5))`, i.e. `applies_to_lines(LOWER_BOUND,UPPER_BOUND)`.

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
- Refer to PLAN_TEMPLATE for description of Features as the structure of each feature type is different
- Remember that the list of lines is under 'lines', e.g. lines([(line(...), ...])
- NOTE ARITY feature/2, e.g. feature(Feature,AppliesToLines). Also each Feature type can have different arities. Look at the PLAN_STRUCTURE to correctly generate code for a particular feature.
- To extent possible, generate Prolog that retrieves only the relevant information. Avoid retrieving information that has not be requested, unless its relevant in some way.
- Always retrieve the minimum the plan Title(s). And retrieve the quantities and features mentioned in the query.
- When feasible, generate predicates (i.e. `consult` code) to make queries less complex and to avoid retrieving intermediate variable values.
- Ensure there are no singleton variables in generated Prolog
- For maximum and minimum queries, don't put a fixed value in the query. Find the value and also return it. 
- Assume Prolog engine to be the ISO compliant Tau prolog with the list module loaded
- To find the price for a number lines use monthly_price for that number of lines. No accumulation is needed. 

Respond with the following JSON structure:
```{{
  "Query" : "...",
  "Predicates" : "..."
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
