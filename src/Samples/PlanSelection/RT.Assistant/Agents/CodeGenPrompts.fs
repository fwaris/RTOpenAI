namespace RT.Assistant.WorkFlow

module CodeGenPrompts =
    let sysMsg = lazy($"""
You are given a collection of phone plans as Prolog language 'facts' (i.e. a plan database).
Each plan matches the plan structure given in PLAN_TEMPLATE.
Your goal is to convert a natural language query - given in the field `query` - into equivalent Prolog code.

PLAN_TEMPLATE```
{RT.Assistant.Plan.PlanPrompts.planTemplate.Value}
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

The plan categories are: {RT.Assistant.Plan.PlanPrompts.planCategories}
The plan names are:
{RT.Assistant.Plan.PlanPrompts.planNames}

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
- Instead of accumulating all solutions explicitly, rely on the Prolog system to get all solutions. Generate the query/predicates that can a solution.

Respond with the following JSON structure:
```{{
  "Query" : "...",
  "Predicates" : "..."
}}```
Ensure any predicates are returned int Predicates field and the query is return in Query. 
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
      
