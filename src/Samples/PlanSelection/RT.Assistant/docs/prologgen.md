# A Comparison of Model Performance for Prolog Code Generation

## Overview
The RT.Assistant sample is a voice bot for question-answering over phone plan information. Phone plans today are somewhat complex, *bundled* products and services, where price-feature combinations are governed by a *set of rules*. 

RT.Assistant represents phone plan data as Prolog [*facts*](/src/Samples/PlanSelection/RT.Assistant/Resources/Raw/wwwroot/plan_clauses.pl) so as to authentically represent the rules and remove any ambiguity (which may arise from describing the rules in plain English).

The voice bot - *with the help of the OpenAI realtime API* - receives (rephrased) user queries as tool calls. It then converts them into equivalent Prolog code with the help of another LLM and runs that code to generate the response.

See: [Background](/src/Samples/PlanSelection/RT.Assistant/docs/writeup.md) for additional context.

Currently `GPT-5.1` and Claude `Sonnet 4.5` are the configured options for code generation. Codex was also tried but ist not suited for realtime use due to latency.



## About Prolog
Before comparing model performance, we need to understand some specifics about Prolog code, namely 'free' variables and `predicates` vs.  `query`.

### 'Free' Variables
In Prolog, uppercase character starting names - e.g. 'Title' - are 'free' variables. 

When a Prolog query runs and solutions are found, the solutions values are bound to the free variables in the query and become part of the output.

Here is a sample query to retrieve the title of each plan:

```prolog
plan(Title,_,_,_).
```
It produces the following output:
```
Title = Connect Plus
Title = Core Saver
 ...
```

### Predicates vs Query
Generated Prolog code can have two parts: `predicates` and `query` where:
- **`predicates`** are helper functions to support a complex query.
- **`query`** is top-level code from which the output is generated.

Example: `find military veteran plans for 4 lines for less than $120 / mth`:

#### Predicate:
```prolog
valid_plan_for_military_veteran(Title, Prices, Features) :-
    plan(Title, category(military_veteran), prices(Prices), features(Features)),
    member(line(4, monthly_price(MonthPrice), _), Prices),
    MonthPrice =< 120.
```

#### Query:
```prolog
valid_plan_for_military_veteran(Title, Prices, Features).
```

In the above example, the query *calls* the predicate `valid_plan_for_military_veteran`,  however the output will contain values for only the free variables in the query; not from any in the predicate. Thus, the output contains mostly relevant content.

**An LLM generating Prolog code should divide complex queries into `predicate` and `query` parts to suppress irrelevant content that might bloat the context for downstream tasks.**


## Model Comparison
First off, both `GPT-5.1` and `Sonnet 4.5` generate syntactically correct Prolog code 99% of the time! This was not the case about year ago (late 2024) when I first started testing this idea.

Beyond that there is a clear difference between the performance of the two models. `GPT-5.1` clearly outperforms `Sonnet 4.5` as illustrated by the following examples:

### Example: "Find all plans that offer 4 lines for around $100 per month."
---
#### GPT-5.1 Generated Code


- Predicates:
```prolog
% Helper: check if a price is within a tolerance of a target price
within_tolerance(Price, Target, Tolerance) :-
    Diff is abs(Price - Target),
    Diff =< Tolerance.

% Helper: get the monthly price for a given number of lines in a plan
plan_price_for_lines(Title, NumLines, Price) :-
    plan(Title, _Category, prices(PriceList), _Features),
    member(line(NumLines, monthly_price(Price), _OriginalPrice), PriceList).

% Main predicate: plans that have NumLines lines for about TargetPrice per month
plans_around_price_for_lines(NumLines, TargetPrice, Tolerance, Title, Price) :-
    plan_price_for_lines(Title, NumLines, Price),
    within_tolerance(Price, TargetPrice, Tolerance).

```

- Query:
```prolog
plans_around_price_for_lines(4, 100, 20, Title, Price).
```
- Output:
```
Title = Core, Price = 105
Title = Core 4 Line Offer, Price = 100
Title = Connect First Responder, Price = 120
Title = Connect Military, Price = 120

```
---
### Sonnet 4.5 Generated Code
- Predicates:
```prolog
```
- Query:
```prolog
plan(Title, _, prices(Prices), _), member(line(4, monthly_price(Price), _), Prices), Price =< 100.
```
- Output:
```
Title = Core 4 Line Offer, Prices =
[line(4,monthly_price(100),original_price(120)),line(5,monthly_price(125),original_price(150))],
Price = 100
```

### Example: "Find all plans that support Netflix for four lines and have a price of $200 or less per month for those four lines."
---
#### GPT-5.1 Generated Code
- Predicates:
```prolog
% Helper predicate: check if a feature applies to a given line count
feature_applies_to_lines(applies_to_lines(all), _Lines).
feature_applies_to_lines(applies_to_lines(lines(Low,High)), Lines) :-
    Lines >= Low,
    Lines =< High.

% Helper predicate: get the monthly price for a given number of lines
plan_price_for_lines(PlanTitle, Lines, Price, Desc) :-
    plan(PlanTitle, _Category, prices(PriceList), features(FeaturesList)),
    member(line(Lines, monthly_price(Price), _OriginalPrice), PriceList),
    member(feature(netflix(desc(Desc), included(yes)), Applies), FeaturesList),
    feature_applies_to_lines(Applies, Lines).

```
- Query:
```prolog
plan_price_for_lines(Title, 4, Price, NetflixDesc), Price =< 200.
```
- Output:
```
Title = Connect Plus, Price = 185, NetflixDesc = Netflix Standard with Ads On Us; 

Title = Connect Next First Responder, Price = 200, NetflixDesc = Netflix Standard with Ads On Us
```
---
#### Sonnet 4.5 Generated Code
- Predicates:
```prolog
```
- Query:
```prolog
plan(Title, _, prices(Prices), features(Features)), member(line(4, monthly_price(Price), _), Prices), Price =< 200, member(feature(netflix(desc(NetflixDesc), included(yes)), applies_to_lines(Applicability)), Features), (Applicability = all ; Applicability = lines(Lower, Upper), Lower =< 4, Upper >= 4).
```
- Output:
```
Title = Connect Plus, Prices = [line(1,monthly_price(90),original_price(95)),line(2,monthly_price(150),original_price(160)),line(3,monthly_price(150),original_price(200)),line(4,monthly_price(185),original_price(240)),line(5,monthly_price(220),original_price(280))], Features = [feature(taxes_and_fees(desc(Taxes and fees are included in the monthly price),included_in_monthly_price(yes)),applies_to_lines(all)),feature(autopay_monthly_discount(desc($5 disc. per line up to 8 lines w/AutoPay & eligible payment method.),discount_per_line(5),lines_up_to(8),included_in_monthly_price(yes)),applies_to_lines(all)),feature(no_annual_service_contract(desc(Included)),applies_to_lines(all)),feature(phone_upgrades(desc(Upgrade-ready every two years),frequency_year(2)),applies_to_lines(all)),feature(wi_fi_calling(desc(Included)),applies_to_lines(all)),feature(voicemail_to_text(desc(Included)),applies_to_lines(all)),feature(scam_shield_premium(desc(Scam Shield Premium)),applies_to_lines(all)),feature(high_speed_data(desc(Unlimited 5G & 4G LTE with Unlimited Premium Data),unlimited(yes),premium_data_limited(yes,limit_gb(50))),applies_to_lines(all)),feature(access_5g_at_no_extra_cost(desc(Included)),applies_to_lines(lines(1,1))),feature(canada_and_mexico_included(desc(Unl. talk, text, & up to 15GB of high-speed data),high_speed_data_gb(15)),applies_to_lines(all)),feature(unlimited_talk_and_text(desc(Unlimited)),applies_to_lines(all)),feature(unlimited_international_texting_from_home(desc(Unlimited international text from home)),applies_to_lines(all)),feature(data_and_texting_while_abroad(desc(Unlimited text & up to 5GB of high-speed data, then unlimited data at up to 256Kbps in 215+ countries & destinations),high_speed_data_limit_gb(5),high_speed_data_country_limit(yes,countries(215))),applies_to_lines(all)),feature(low_flat_rate_calling_while_abroad(desc($0.25/minute in 215+ countries & destinations)),applies_to_lines(all)),feature(mobile_hotspot(desc(50GB of high-speed mobile hotspot data included. Unlimited mobile hotspot data at max 3G speeds after 50GB.),high_speed_data_limit_gb(50)),applies_to_lines(all)),feature(video_streaming_quality(desc(Up to 4K UHD video),quality_480p(yes),quality_720p(yes),quality_4k_uhd(yes)),applies_to_lines(all)),feature(in_flight_connection(desc(Unlimited in-flight texting & streaming where available),wifi_hours(unlimited),streaming_included(yes)),applies_to_lines(all)),feature(apple_tv_plus(desc(Included),included(yes)),applies_to_lines(all)),feature(hulu(desc(Not included),included(no)),applies_to_lines(all)),feature(netflix(desc(Netflix Standard with Ads On Us),included(yes)),applies_to_lines(lines(1,5))),feature(one_year_aaa_membership_on_us(desc(Included)),applies_to_lines(all)),feature(connect_telco_travel(desc(No information available)),applies_to_lines(all))], Price = 185, NetflixDesc = Netflix Standard with Ads On Us, Applicability = lines(1,5), Lower = 1, Upper = 5; 

Title = Connect Next First Responder, Prices = [line(1,monthly_price(85),original_price(90)),line(2,monthly_price(130),original_price(140)),line(3,monthly_price(165),original_price(180)),line(4,monthly_price(200),original_price(220)),line(5,monthly_price(235),original_price(260))], Features = [feature(taxes_and_fees(desc(Taxes and fees are included in the monthly price.),included_in_monthly_price(yes)),applies_to_lines(all)),feature(autopay_monthly_discount(desc($5 disc. per line up to 8 lines w/AutoPay & eligible payment method.),discount_per_line(5),lines_up_to(8),included_in_monthly_price(yes)),applies_to_lines(all)),feature(no_annual_service_contract(desc(Included)),applies_to_lines(all)),feature(phone_upgrades(desc(Upgrade-ready every year),frequency_year(1)),applies_to_lines(all)),feature(wi_fi_calling(desc(Included)),applies_to_lines(all)),feature(voicemail_to_text(desc(Included)),applies_to_lines(all)),feature(scam_shield_premium(desc(Scam Shield Premium)),applies_to_lines(all)),feature(high_speed_data(desc(Unlimited 5G & 4G LTE with Unlimited Premium Data),unlimited(yes),premium_data_limited(yes,limit_gb(50))),applies_to_lines(all)),feature(access_5g_at_no_extra_cost(desc(Included)),applies_to_lines(all)),feature(canada_and_mexico_included(desc(Unl. talk, text, & up to 15GB of high-speed data),high_speed_data_gb(15)),applies_to_lines(all)),feature(unlimited_talk_and_text(desc(Unlimited)),applies_to_lines(all)),feature(unlimited_international_texting_from_home(desc(Unlimited international text from home)),applies_to_lines(all)),feature(data_and_texting_while_abroad(desc(Unlimited text & up to 5GB of high-speed data, then unlimited data at up to 256Kbps in 215+ countries & destinations),high_speed_data_limit_gb(5),high_speed_data_country_limit(yes,countries(11))),applies_to_lines(all)),feature(low_flat_rate_calling_while_abroad(desc($0.25/minute in 215+ countries & destinations)),applies_to_lines(all)),feature(mobile_hotspot(desc(50GB of high-speed mobile hotspot data included. Unlimited mobile hotspot data at max 3G speeds after 50GB.),high_speed_data_limit_gb(50)),applies_to_lines(all)),feature(video_streaming_quality(desc(Up to 4K UHD video),quality_480p(yes),quality_720p(yes),quality_4k_uhd(yes)),applies_to_lines(all)),feature(in_flight_connection(desc(Unlimited in-flight texting & streaming where available),wifi_hours(unlimited),streaming_included(yes)),applies_to_lines(all)),feature(apple_tv_plus(desc(Included),included(yes)),applies_to_lines(all)),feature(hulu(desc(Hulu with ads ON US),included(yes)),applies_to_lines(all)),feature(netflix(desc(Netflix Standard with Ads On Us),included(yes)),applies_to_lines(all)),feature(one_year_aaa_membership_on_us(desc(Included)),applies_to_lines(all)),feature(connect_telco_travel(desc(No additional details available for Connect-Telco Travel)),applies_to_lines(all))], Price = 200, NetflixDesc = Netflix Standard with Ads On Us, Applicability = all

```
---

## Analysis of Model Outputs
Looking at the `predicates`, `query` and `outputs` for each of the two models `GPT-5.1` and `Sonnet 4.5`, for the two examples given above, it is clear that `GPT-5.1`produces much better quality output.

Even though both models generate technically correct Prolog code, `Sonnet 4.5` does not generate any predicates (ever) [despite explicit instructions](/src/Samples/PlanSelection/RT.Assistant/Agents/CodeGenPrompts.fs#L36). As a result, `Sonnet 4.5` generated code output contains unnecessary and irrelevant content.

The results of these (limited) tests suggest that `GPT-5.1` is much better suited to the task of Prolog code generation. A fact that is borne out by anecdotal evidence when using RT.Assistant with each of the two models.


