module PlanInfo
let planInfo = """
plan(
    title("Essentials Saver"),
    unique_plan_id("M0006945950PPI000000000076"),
    category("CONSUMER"),
    lines_allowed(min_lines(1), max_lines(3)),
    monthly_prices([
        lines_price(1, 55),
        lines_price(2, 90),
        lines_price(3, 115)
    ]),
    taxes_and_fees_included(no),
    auto_pay_discount(eligible),
    disclaimers([
        "This monthly price is not an introductory rate and does not require a yearly contract",
        "Does not include AutoPay or other discounts",
        "No annual service contract required"
    ]),
    data_allowance(
        unlimited_data(yes),
        premium_data_gb(50),
        hotspot_data_gb(0),
        after_limit_speed(throttled)
    ),
    typical_speeds(
        download_mbps_range(79, 357),
        upload_mbps_range(6, 30),
        latency_ms_range(17, 32)
    ),
    included_benefits([
        "Unlimited talk and text",
        "50GB premium data",
        "5G access"
    ]),
    additional_charges_and_terms([
        device_connection_charge(35),
        early_termination_fee(0),
        government_taxes(varies_by_location)
    ]),
    notes("AutoPay discount with eligible payment method is not reflected in these prices")
).
plan(
    title("Go5G Next"),
    unique_plan_id("M0006945950PPI000000000081"),
    category("CONSUMER"),
    lines_allowed(min_lines(1), max_lines(12)),
    monthly_prices([
        /* Base prices for 1–5 lines */
        lines_price(1, 105),
        lines_price(2, 180),
        lines_price(3, 230),
        lines_price(4, 280),
        lines_price(5, 330),
        /* Each additional line 6–8 is +$50/line */
        lines_price(6, 380),
        lines_price(7, 430),
        lines_price(8, 480),
        /* Each additional line 9–12 is +$55/line */
        lines_price(9, 535),
        lines_price(10, 590),
        lines_price(11, 645),
        lines_price(12, 700)
    ]),
    taxes_and_fees_included(yes),
    auto_pay_discount(eligible),
    disclaimers([
        "This monthly price is not an introductory rate and does not require a yearly contract",
        "Does not include AutoPay or other discounts",
        "One-time device connection charge: $35/line"
    ]),
    data_allowance(
        unlimited_data(yes),
        premium_data_gb(unlimited),
        hotspot_data_gb(50),
        after_limit_speed(128)
    ),
    typical_speeds(
        download_mbps_range(89, 418),
        upload_mbps_range(6, 31),
        latency_ms_range(17, 32)
    ),
    included_benefits([
        "Upgrade-ready every year",
        "Unlimited premium data",
        "50GB high-speed mobile hotspot",
        "Unlimited talk and text",
        "Taxes & fees included",
        "Hulu with ads ON US",
        "Apple TV+ ON US",
        "Netflix Standard with ads ON US"
    ]),
    additional_charges_and_terms([
        device_connection_charge(35),
        early_termination_fee(0),
        government_taxes(included_in_monthly_price)
    ]),
    notes("AutoPay discount with eligible payment method is not reflected in these prices")
).
plan(
      title("Go5G Next First Responder"),
      unique_plan_id("M0006945950PPI000000000083"),
      category("FIRST_RESPONDER"),
      lines_allowed(min_lines(1), max_lines(12)),
      monthly_prices([
        lines_price(1, 90),
        lines_price(2, 140),
        lines_price(3, 180),
        lines_price(4, 220),
        lines_price(5, 260),
        lines_price(6, 300),
        % Each additional line from 7 to 12 is +$50/line
        lines_price(7, 350),
        lines_price(8, 400),
        lines_price(9, 450),
        lines_price(10, 500),
        lines_price(11, 550),
        lines_price(12, 600)
      ]),
      taxes_and_fees_included(yes),
      auto_pay_discount(eligible),
      disclaimers([
        "This monthly price is not an introductory rate and does not require a yearly contract",
        "Does not include AutoPay or other discounts",
        "One-time device connection charge: $35/line"
      ]),
      data_allowance(
        unlimited_data(yes),
        premium_data_gb(unlimited),
        hotspot_data_gb(50),
        after_limit_speed(600)
      ),
      typical_speeds(
        download_mbps_range(89, 418),
        upload_mbps_range(6, 31),
        latency_ms_range(17, 32)
      ),
      included_benefits([
        "Unlimited talk and text",
        "Unlimited premium data",
        "Upgrade-ready every year",
        "Hulu (with ads) ON US",
        "Apple TV+ ON US",
        "Netflix Standard with ads ON US",
        "Taxes & fees included"
      ]),
      additional_charges_and_terms([
        device_connection_charge(35),
        early_termination_fee(0),
        government_taxes(included_in_monthly_price)
      ]),
      notes("AutoPay discount with eligible payment method is not reflected in these prices")
).
plan(
    title("Go5G Next Military"),
    unique_plan_id("M0006945950PPI000000000084"),
    category("MILITARY"),
    lines_allowed(min_lines(1), max_lines(12)),
    monthly_prices([
        lines_price(1, 90),
        lines_price(2, 140),
        lines_price(3, 180),
        lines_price(4, 220),
        lines_price(5, 260),
        lines_price(6, 300),
        % Each additional line from 7 to 12 is +$50/line
        lines_price(7, 350),
        lines_price(8, 400),
        lines_price(9, 450),
        lines_price(10, 500),
        lines_price(11, 550),
        lines_price(12, 600)
    ]),
    taxes_and_fees_included(yes),
    auto_pay_discount(eligible),
    disclaimers([
        "This monthly price is not an introductory rate and does not require a yearly contract",
        "Does not include AutoPay or other discounts",
        "One-time device connection charge: $35/line"
    ]),
    data_allowance(
        unlimited_data(yes),
        premium_data_gb(unlimited),
        hotspot_data_gb(50),
        after_limit_speed(600)
    ),
    typical_speeds(
        download_mbps_range(89, 418),
        upload_mbps_range(6, 31),
        latency_ms_range(17, 32)
    ),
    included_benefits([
        "Unlimited talk and text",
        "Unlimited premium data",
        "Taxes & fees included",
        "Apple TV+ ON US",
        "Netflix Standard with ads ON US",
        "Hulu with ads ON US",
        "Upgrade-ready every year"
    ]),
    additional_charges_and_terms([
        device_connection_charge(35),
        early_termination_fee(0),
        government_taxes(included_in_monthly_price)
    ]),
    notes("AutoPay discount with eligible payment method is not reflected in these prices")
).
plan(
    title("Go5G Next 55"),
    unique_plan_id("M0006945950PPI000000000119"),
    category("55+"),
    lines_allowed(
        % The plan group includes a 1-line version and a 2–4-line version
        min_lines(1),
        max_lines(4)
    ),
    monthly_prices([
        lines_price(1, 90),
        lines_price(2, 140),
        lines_price(3, 210),
        lines_price(4, 280)
    ]),
    taxes_and_fees_included(yes),
    auto_pay_discount(eligible),
    disclaimers([
        "This monthly price is not an introductory rate and does not require a yearly contract",
        "Does not include AutoPay or other discounts",
        "One-time device connection charge: $35/line"
    ]),
    data_allowance(
        unlimited_data(yes),
        premium_data_gb(unlimited),
        % From plan description: “50GB high-speed mobile hotspot”
        hotspot_data_gb(50),
        after_limit_speed(128)
    ),
    typical_speeds(
        download_mbps_range(89, 418),
        upload_mbps_range(6, 31),
        latency_ms_range(17, 32)
    ),
    included_benefits([
        "Upgrade-ready every year",
        "Unlimited premium data",
        "Taxes & fees included",
        "Hulu with ads ON US",
        "Apple TV+ ON US",
        "Netflix Standard with ads ON US"
    ]),
    additional_charges_and_terms([
        device_connection_charge(35),
        early_termination_fee(0),
        government_taxes(included_in_monthly_price)
    ]),
    notes("AutoPay discount with eligible payment method is not reflected in these prices")
).
plan(
    title("Go5G Plus"),
    unique_plan_id("M0006945950PPI000000000085"),
    category("Consumer"),
    lines_allowed(min_lines(1), max_lines(12)),
    monthly_prices([
        lines_price(1, 95),
        lines_price(2, 160),
        lines_price(3, 200),
        lines_price(4, 240),
        lines_price(5, 280),
        lines_price(6, 320),
        lines_price(7, 360),
        lines_price(8, 400),
        lines_price(9, 445),
        lines_price(10, 490),
        lines_price(11, 535),
        lines_price(12, 580)
    ]),
    taxes_and_fees_included(yes),
    auto_pay_discount(eligible),
    disclaimers([
        "This monthly price is not an introductory rate and does not require a yearly contract",
        "Does not include AutoPay or other discounts",
        "One-time device connection charge: $35/line"
    ]),
    data_allowance(
        unlimited_data(yes),
        premium_data_gb(unlimited),
        hotspot_data_gb(50),
        after_limit_speed(not_specified)
    ),
    typical_speeds(
        download_mbps_range(89, 418),
        upload_mbps_range(6, 31),
        latency_ms_range(17, 32)
    ),
    included_benefits([
        "Unlimited talk and text",
        "Unlimited premium data",
        "Taxes & fees included",
        "Apple TV+ ON US",
        "Netflix Standard with ads ON US",
        "50GB high-speed mobile hotspot"
    ]),
    additional_charges_and_terms([
        device_connection_charge(35),
        early_termination_fee(0),
        government_taxes(included_in_monthly_price)
    ]),
    notes("AutoPay discount with eligible payment method is not reflected in these prices")
).
plan(
    title("Go5G Plus Military"),
    unique_plan_id("M0006945950PPI000000000088"),
    category("MILITARY"),
    lines_allowed(min_lines(1), max_lines(12)),
    monthly_prices([
        lines_price(1, 80),
        lines_price(2, 120),
        lines_price(3, 150),
        lines_price(4, 180),
        lines_price(5, 210),
        lines_price(6, 240),
        % Each additional line from 7 to 12 is +$40/line
        lines_price(7, 280),
        lines_price(8, 320),
        lines_price(9, 360),
        lines_price(10, 400),
        lines_price(11, 440),
        lines_price(12, 480)
    ]),
    taxes_and_fees_included(yes),
    auto_pay_discount(eligible),
    disclaimers([
        "This monthly price is not an introductory rate and does not require a yearly contract",
        "Does not include AutoPay or other discounts"
    ]),
    data_allowance(
        unlimited_data(yes),
        premium_data_gb(unlimited),
        hotspot_data_gb(50),
        after_limit_speed(128)
    ),
    typical_speeds(
        download_mbps_range(89, 418),
        upload_mbps_range(6, 31),
        latency_ms_range(17, 32)
    ),
    included_benefits([
        "Unlimited talk and text",
        "Unlimited data",
        "50GB high-speed mobile hotspot",
        "Taxes & fees included",
        "Apple TV+ ON US",
        "Netflix Standard with ads ON US"
    ]),
    additional_charges_and_terms([
        device_connection_charge(35),
        early_termination_fee(0),
        government_taxes(included_in_monthly_price)
    ]),
    notes("AutoPay discount with eligible payment method is not reflected in these prices")
).
plan(
      title("Go5G Plus First Responder"),
      unique_plan_id("M0006945950PPI000000000087"),
      category("FIRST_RESPONDER"),
      lines_allowed(min_lines(1), max_lines(12)),
      monthly_prices([
        lines_price(1, 80),
        lines_price(2, 120),
        lines_price(3, 150),
        lines_price(4, 180),
        lines_price(5, 210),
        lines_price(6, 240),
        % Each additional line from 7 to 12 is +$40/line
        lines_price(7, 280),
        lines_price(8, 320),
        lines_price(9, 360),
        lines_price(10, 400),
        lines_price(11, 440),
        lines_price(12, 480)
      ]),
      taxes_and_fees_included(yes),
      auto_pay_discount(eligible),
      disclaimers([
        "This monthly price is not an introductory rate and does not require a yearly contract",
        "Does not include AutoPay or other discounts",
        "One-time device connection charge: $35/line"
      ]),
      data_allowance(
        unlimited_data(yes),
        premium_data_gb(unlimited),
        hotspot_data_gb(50),
        after_limit_speed(128)
      ),
      typical_speeds(
        download_mbps_range(89, 418),
        upload_mbps_range(6, 31),
        latency_ms_range(17, 32)
      ),
      included_benefits([
        "Unlimited talk and text",
        "Unlimited premium data",
        "Taxes & fees included",
        "Apple TV+ ON US",
        "Netflix Standard with ads ON US",
        "50GB high-speed mobile hotspot"
      ]),
      additional_charges_and_terms([
        device_connection_charge(35),
        early_termination_fee(0),
        government_taxes(included_in_monthly_price)
      ]),
      notes("AutoPay discount with eligible payment method is not reflected in these prices")
).
plan(
    title("Essentials 4 Line Offer"),
    unique_plan_id("M0006945950PPI000000000074"),
    category("CONSUMER"),
    lines_allowed(min_lines(4), max_lines(6)),
    monthly_prices([
        lines_price(4, 120),
        lines_price(5, 150),
        lines_price(6, 180)
    ]),
    taxes_and_fees_included(no),
    auto_pay_discount(eligible),
    disclaimers([
        "This monthly price is not an introductory rate and does not require a yearly contract",
        "Does not include AutoPay or other discounts"
    ]),
    data_allowance(
        unlimited_data(yes),
        premium_data_gb(50),
        hotspot_data_gb(0),
        after_limit_speed(128)
    ),
    typical_speeds(
        download_mbps_range(79, 357),
        upload_mbps_range(6, 30),
        latency_ms_range(17, 32)
    ),
    included_benefits([
        "Unlimited talk and text",
        "50GB premium data",
        "5G access"
    ]),
    additional_charges_and_terms([
        device_connection_charge(35),
        early_termination_fee(0),
        government_taxes(varies_by_location)
    ]),
    notes("AutoPay discount with eligible payment method is not reflected in these prices")
).
plan(
    title("Essentials"),
    unique_plan_id("M0006945950PPI000000000073"),
    category("CONSUMER"),
    lines_allowed(min_lines(1), max_lines(6)),
    monthly_prices([
        lines_price(1, 65),
        lines_price(2, 100),
        lines_price(3, 120),
        lines_price(4, 140),
        lines_price(5, 160),
        lines_price(6, 180)
    ]),
    taxes_and_fees_included(no),
    auto_pay_discount(eligible),
    disclaimers([
        "This monthly price is not an introductory rate and does not require a yearly contract",
        "Does not include AutoPay or other discounts"
    ]),
    data_allowance(
        unlimited_data(yes),
        premium_data_gb(50),
        hotspot_data_gb(0)
        % No explicit after-limit speed is stated; omitted or set to after_limit_speed(unspecified)
    ),
    typical_speeds(
        download_mbps_range(79, 357),
        upload_mbps_range(6, 30),
        latency_ms_range(17, 32)
    ),
    included_benefits([
        "Unlimited talk and text",
        "Unlimited data (with 50GB premium data)",
        "5G access"
    ]),
    additional_charges_and_terms([
        device_connection_charge(35),
        early_termination_fee(0),
        government_taxes(varies_by_location)
        % Provider regulatory fee of $3.49/line and other surcharges also apply, but not explicitly enumerated here
    ]),
    notes("AutoPay discount with eligible payment method is not reflected in these prices")
).
plan(
      title("Essentials Choice 55"),
      unique_plan_id("M0006945950PPI000000000106"),
      category("55+"),
      lines_allowed(min_lines(1), max_lines(2)),
      monthly_prices([
        lines_price(1, 50),
        lines_price(2, 70)
      ]),
      taxes_and_fees_included(no),
      auto_pay_discount(eligible),
      disclaimers([
        "This monthly price is not an introductory rate and does not require a yearly contract",
        "Does not include AutoPay or other discounts",
        "One-time device connection charge: $35/line"
      ]),
      data_allowance(
        unlimited_data(yes),
        premium_data_gb(50),
        hotspot_data_gb(0),       % Unlimited hotspot at 3G speeds
        after_limit_speed(512)    % 3G roughly ~512 Kbps
      ),
      typical_speeds(
        download_mbps_range(79, 357),
        upload_mbps_range(6, 30),
        latency_ms_range(17, 32)
      ),
      included_benefits([
        "Unlimited talk and text",
        "50GB premium data",
        "Unlimited 3G mobile hotspot",
        "Scam Shield Premium"
      ]),
      additional_charges_and_terms([
        device_connection_charge(35),
        early_termination_fee(0),
        government_taxes(varies_by_location),
        regulatory_program_fee(3.49), % Fee per line
        federal_and_local_surcharges_range(0.20, 4.25)
      ]),
      notes("AutoPay discount with eligible payment method is not reflected in these prices.")
).
plan(
    title("Go5G Plus 55"),
    unique_plan_id("M0006945950PPI000000000118"),
    category("55+"),
    lines_allowed(min_lines(1), max_lines(4)),
    monthly_prices([
        lines_price(1, 80),
        lines_price(2, 120),
        lines_price(3, 180),
        lines_price(4, 240)
    ]),
    taxes_and_fees_included(yes),
    auto_pay_discount(eligible),
    disclaimers([
        "This monthly price is not an introductory rate and does not require a yearly contract",
        "Does not include AutoPay or other discounts",
        "One-time device connection charge: $35/line"
    ]),
    data_allowance(
        unlimited_data(yes),
        premium_data_gb(unlimited),
        hotspot_data_gb(50),
        after_limit_speed(128)
    ),
    typical_speeds(
        download_mbps_range(89, 418),
        upload_mbps_range(6, 31),
        latency_ms_range(17, 32)
    ),
    included_benefits([
        "Taxes & fees included",
        "Apple TV+ ON US",
        "Netflix Standard with ads ON US",
        "50GB high-speed mobile hotspot"
    ]),
    additional_charges_and_terms([
        device_connection_charge(35),
        early_termination_fee(0),
        government_taxes(included_in_monthly_price)
    ]),
    notes("AutoPay discount with eligible payment method is not reflected in these prices")
).
plan(
    title("Go5G 55"),
    unique_plan_id("M0006945950PPI000000000117"),
    category("55+"),
    lines_allowed(min_lines(1), max_lines(4)),
    monthly_prices([
        lines_price(1, 65),
        lines_price(2, 100),
        lines_price(3, 150),
        lines_price(4, 200)
    ]),
    taxes_and_fees_included(yes),
    auto_pay_discount(eligible),
    disclaimers([
        "This monthly price is not an introductory rate and does not require a yearly contract",
        "Does not include AutoPay or other discounts",
        "One-time device connection charge: $35/line"
    ]),
    data_allowance(
        unlimited_data(yes),
        premium_data_gb(100),
        hotspot_data_gb(0),
        after_limit_speed(128)
    ),
    typical_speeds(
        download_mbps_range(89, 418),
        upload_mbps_range(6, 31),
        latency_ms_range(17, 32)
    ),
    included_benefits([
        "Unlimited talk and text",
        "Unlimited premium data",
        "Taxes & fees included",
        "Apple TV+ ON US for 6 months",
        "Netflix Standard with ads ON US"
    ]),
    additional_charges_and_terms([
        device_connection_charge(35),
        early_termination_fee(0),
        government_taxes(included_in_monthly_price)
    ]),
    notes("AutoPay discount with eligible payment method is not reflected in these prices")
).
plan(
    title("Go5G"),
    unique_plan_id("M0006945950PPI000000000077"),
    category("CONSUMER"),
    lines_allowed(min_lines(1), max_lines(12)),
    monthly_prices([
       lines_price(1, 80),
       lines_price(2, 140),
       lines_price(3, 170),
       lines_price(4, 200),
       lines_price(5, 230),
       % 6–8 lines: $30 per additional line
       lines_price(6, 30),
       lines_price(7, 30),
       lines_price(8, 30),
       % 9–12 lines: $35 per additional line
       lines_price(9, 35),
       lines_price(10, 35),
       lines_price(11, 35),
       lines_price(12, 35)
    ]),
    taxes_and_fees_included(yes),
    auto_pay_discount(eligible),
    disclaimers([
        "This monthly price is not an introductory rate and does not require a yearly contract",
        "Does not include AutoPay or other discounts",
        "One-time device connection charge: $35/line"
    ]),
    data_allowance(
        unlimited_data(yes),
        premium_data_gb(100),
        hotspot_data_gb(0),
        after_limit_speed(128)
    ),
    typical_speeds(
        download_mbps_range(89, 418),
        upload_mbps_range(6, 31),
        latency_ms_range(17, 32)
    ),
    included_benefits([
        "Unlimited talk, text, and data",
        "100GB premium data",
        "Taxes & fees included",
        "Apple TV+ ON US for 6 months",
        "Netflix Standard with ads ON US"
    ]),
    additional_charges_and_terms([
        device_connection_charge(35),
        early_termination_fee(0),
        government_taxes(included_in_monthly_price)
    ]),
    notes("AutoPay discount with eligible payment method is not reflected in these prices")
).
plan(
      title("Go5G Military"),
      unique_plan_id("M0006945950PPI000000000080"),
      category("MILITARY"),
      lines_allowed(min_lines(1), max_lines(12)),
      monthly_prices([
        lines_price(1, 65),
        lines_price(2, 100),
        lines_price(3, 120),
        lines_price(4, 140),
        lines_price(5, 160),
        lines_price(6, 180),
        % Each additional line from 7 to 12 is +$30/line
        lines_price(7, 210),
        lines_price(8, 240),
        lines_price(9, 270),
        lines_price(10, 300),
        lines_price(11, 330),
        lines_price(12, 360)
      ]),
      taxes_and_fees_included(yes),
      auto_pay_discount(eligible),
      disclaimers([
        "This monthly price is not an introductory rate and does not require a yearly contract",
        "Does not include AutoPay or other discounts",
        "One-time device connection charge: $35/line"
      ]),
      data_allowance(
        unlimited_data(yes),
        premium_data_gb(100),
        hotspot_data_gb(0),
        after_limit_speed(128)
      ),
      typical_speeds(
        download_mbps_range(89, 418),
        upload_mbps_range(6, 31),
        latency_ms_range(17, 32)
      ),
      included_benefits([
        "Unlimited talk and text",
        "100GB of premium data",
        "Taxes & fees included",
        "Apple TV+ ON US for 6 months",
        "Netflix Standard with ads ON US"
      ]),
      additional_charges_and_terms([
        device_connection_charge(35),
        early_termination_fee(0),
        government_taxes(included_in_monthly_price)
      ]),
      notes("AutoPay discount with eligible payment method is not reflected in these prices")
).
plan(
      title("Go5G First Responder"),
      unique_plan_id("M0006945950PPI000000000079"),
      category("FIRST_RESPONDER"),
      lines_allowed(min_lines(1), max_lines(12)),
      monthly_prices([
        lines_price(1, 65),
        lines_price(2, 100),
        lines_price(3, 120),
        lines_price(4, 140),
        lines_price(5, 160),
        lines_price(6, 180),
        % Each additional line from 7 to 12 is +$30/line
        lines_price(7, 210),
        lines_price(8, 240),
        lines_price(9, 270),
        lines_price(10, 300),
        lines_price(11, 330),
        lines_price(12, 360)
      ]),
      taxes_and_fees_included(yes),
      auto_pay_discount(eligible),
      disclaimers([
        "This monthly price is not an introductory rate and does not require a yearly contract",
        "Does not include AutoPay or other discounts",
        "One-time device connection charge: $35/line"
      ]),
      data_allowance(
        unlimited_data(yes),
        premium_data_gb(100),
        hotspot_data_gb(0),
        after_limit_speed(128)
      ),
      typical_speeds(
        download_mbps_range(89, 418),
        upload_mbps_range(6, 31),
        latency_ms_range(17, 32)
      ),
      included_benefits([
        "Unlimited talk and text",
        "100GB premium data",
        "Taxes & fees included",
        "Apple TV+ ON US for 6 months",
        "Netflix Standard with ads ON US"
      ]),
      additional_charges_and_terms([
        device_connection_charge(35),
        early_termination_fee(0),
        government_taxes(included_in_monthly_price)
      ]),
      notes("AutoPay discount with an eligible payment method is not reflected in these prices. There may be additional billing discounts available.")
).
plan(
    title("Go5G Puerto Rico"),
    unique_plan_id("M0006945950PPI000000000097"),
    category("CONSUMER"),
    lines_allowed(min_lines(1), max_lines(3)),
    monthly_prices([
        lines_price(1, 70),
        lines_price(2, 120),
        lines_price(3, 170)
    ]),
    taxes_and_fees_included(yes),
    auto_pay_discount(eligible),
    disclaimers([
        "This monthly price is not an introductory rate and does not require a yearly contract",
        "Does not include AutoPay or other discounts",
        "One-time device connection charge: $35/line"
    ]),
    data_allowance(
        unlimited_data(yes),
        premium_data_gb(100),
        hotspot_data_gb(0),
        after_limit_speed(128)
    ),
    typical_speeds(
        download_mbps_range(89, 418),
        upload_mbps_range(6, 31),
        latency_ms_range(17, 32)
    ),
    included_benefits([
        "Unlimited talk and text",
        "100GB premium data",
        "Taxes & fees included",
        "Apple TV+ ON US (for 6 months)"
    ]),
    additional_charges_and_terms([
        device_connection_charge(35),
        early_termination_fee(0),
        government_taxes(included_in_monthly_price)
    ]),
    notes("AutoPay discount with eligible payment method is not reflected in these prices")
).
plan(
    title("Magenta® MAX"),
    unique_plan_id("M0006945950PPI000000000092"),
    category("CONSUMER"),
    lines_allowed(min_lines(1), max_lines(12)),
    monthly_prices([
        lines_price(1, 90),
        lines_price(2, 150),
        lines_price(3, 185),
        lines_price(4, 220),
        lines_price(5, 255),
        % Each additional line for lines 6–8 is +$35/line
        lines_price(6, 290),
        lines_price(7, 325),
        lines_price(8, 360),
        % Each additional line for lines 9–12 is +$40/line
        lines_price(9, 400),
        lines_price(10, 440),
        lines_price(11, 480),
        lines_price(12, 520)
    ]),
    taxes_and_fees_included(yes),
    auto_pay_discount(eligible),
    disclaimers([
        "This monthly price is not an introductory rate and does not require a yearly contract",
        "Does not include AutoPay or other discounts",
        "One-time device connection charge: $35/line"
    ]),
    data_allowance(
        unlimited_data(yes),
        premium_data_gb(unlimited),
        hotspot_data_gb(40),
        after_limit_speed(128)
    ),
    typical_speeds(
        download_mbps_range(89, 418),
        upload_mbps_range(6, 31),
        latency_ms_range(17, 32)
    ),
    included_benefits([
        "Unlimited talk and text",
        "Unlimited premium data",
        "Taxes & fees included",
        "Apple TV+ ON US",
        "Netflix Standard with ads ON US"
    ]),
    additional_charges_and_terms([
        device_connection_charge(35),
        early_termination_fee(0),
        government_taxes(included_in_monthly_price)
    ]),
    notes("AutoPay discount with eligible payment method is not reflected in these prices")
).
plan(
    title("Magenta® MAX Military"),
    unique_plan_id("M0006945950PPI000000000095"),
    category("MILITARY"),
    /*
       Combining PLAN_GROUP entries:
         - The lowest MinLines is 1
         - The highest MaxLines is 12
         - Monthly prices for lines 1–6 are as given; each line from 7 to 12 adds $35
    */
    lines_allowed(min_lines(1), max_lines(12)),
    monthly_prices([
        lines_price(1, 75),
        lines_price(2, 110),
        lines_price(3, 135),
        lines_price(4, 160),
        lines_price(5, 185),
        lines_price(6, 210),
        % Each additional line for lines 7–12 is +$35/line
        lines_price(7, 245),
        lines_price(8, 280),
        lines_price(9, 315),
        lines_price(10, 350),
        lines_price(11, 385),
        lines_price(12, 420)
    ]),
    taxes_and_fees_included(yes),
    auto_pay_discount(eligible),
    /*
       Disclaimers and included benefits derived from plan details:
         - Both entries note “taxes & fees included” and “does not include AutoPay”
         - The one-time device connection charge is $35/line
    */
    disclaimers([
        "This monthly price is not an introductory rate and does not require a yearly contract",
        "Does not include AutoPay or other discounts",
        "One-time device connection charge: $35/line"
    ]),
    /*
       Data and speed information common to both entries:
         - Unlimited premium data (cannot be slowed down)
         - Typical 5G speeds are 89-418 Mbps down, 6-31 Mbps up, 17-32 ms latency
    */ 
    data_allowance(
        unlimited_data(yes),
        premium_data_gb(unlimited),
        hotspot_data_gb(0),
        after_limit_speed(128)  % fallback for any throttling scenario
    ),
    typical_speeds(
        download_mbps_range(89, 418),
        upload_mbps_range(6, 31),
        latency_ms_range(17, 32)
    ),
    included_benefits([
        "Unlimited talk and text",
        "Unlimited premium data",
        "Taxes & fees included",
        "Apple TV+ ON US",
        "Netflix Standard with ads ON US"
    ]),
    additional_charges_and_terms([
        device_connection_charge(35),
        early_termination_fee(0),
        government_taxes(included_in_monthly_price)
    ]),
    notes("AutoPay discount with eligible payment method is not reflected in these prices")
).
plan(
      title("Magenta® MAX First Responder"),
      unique_plan_id("M0006945950PPI000000000094"),
      category("FIRST_RESPONDER"),
      lines_allowed(min_lines(1), max_lines(12)),
      monthly_prices([
        lines_price(1, 75),
        lines_price(2, 110),
        lines_price(3, 135),
        lines_price(4, 160),
        lines_price(5, 185),
        lines_price(6, 210),
        % Each additional line from 7 to 12 is +$35/line
        lines_price(7, 35),
        lines_price(8, 35),
        lines_price(9, 35),
        lines_price(10, 35),
        lines_price(11, 35),
        lines_price(12, 35)
      ]),
      taxes_and_fees_included(yes),
      auto_pay_discount(eligible),
      disclaimers([
        "This monthly price is not an introductory rate and does not require a yearly contract",
        "Does not include AutoPay or other discounts",
        "One-time device connection charge: $35/line"
      ]),
      data_allowance(
        unlimited_data(yes),
        premium_data_gb(unlimited),
        hotspot_data_gb(0),
        after_limit_speed(128)
      ),
      typical_speeds(
        download_mbps_range(89, 418),
        upload_mbps_range(6, 31),
        latency_ms_range(17, 32)
      ),
      included_benefits([
        "Unlimited talk and text",
        "Unlimited premium data",
        "Taxes & fees included",
        "Apple TV+ ON US",
        "Netflix Standard with ads ON US"
      ]),
      additional_charges_and_terms([
        device_connection_charge(35),
        early_termination_fee(0),
        government_taxes(included_in_monthly_price)
      ]),
      notes("AutoPay discount with eligible payment method is not reflected in these prices")
).
plan(
    title("Magenta® MAX 55+"),
    unique_plan_id("M0006945950PPI000000000093"),
    category("55+"),
    lines_allowed(min_lines(1), max_lines(4)),
    monthly_prices([
        lines_price(1, 70),
        lines_price(2, 100),
        lines_price(3, 150),
        lines_price(4, 200)
    ]),
    taxes_and_fees_included(yes),
    auto_pay_discount(eligible),
    disclaimers([
        "This monthly price is not an introductory rate and does not require a yearly contract",
        "Does not include AutoPay or other discounts",
        "One-time device connection charge: $35/line"
    ]),
    data_allowance(
        unlimited_data(yes),
        premium_data_gb(unlimited),
        hotspot_data_gb(40),
        after_limit_speed(128)
    ),
    typical_speeds(
        download_mbps_range(89, 418),
        upload_mbps_range(6, 31),
        latency_ms_range(17, 32)
    ),
    included_benefits([
        "Unlimited talk and text",
        "Unlimited premium data",
        "Taxes & fees included",
        "Apple TV+ ON US",
        "Netflix Standard with ads ON US"
    ]),
    additional_charges_and_terms([
        device_connection_charge(35),
        early_termination_fee(0),
        government_taxes(included_in_monthly_price)
    ]),
    notes("AutoPay discount with eligible payment method is not reflected in these prices")
).
plan(
    title("Magenta®"),
    unique_plan_id("M0006945950PPI000000000089"),
    category("Consumer"),
    lines_allowed(min_lines(1), max_lines(12)),
    monthly_prices([
        lines_price(1, 75),
        lines_price(2, 130),
        lines_price(3, 155),
        lines_price(4, 180),
        lines_price(5, 205),
        % Additional lines from 6 to 8 cost $25/line
        lines_price(6, 230),
        lines_price(7, 255),
        lines_price(8, 280),
        % Additional lines from 9 to 12 cost $30/line
        lines_price(9, 310),
        lines_price(10, 340),
        lines_price(11, 370),
        lines_price(12, 400)
    ]),
    taxes_and_fees_included(yes),
    auto_pay_discount(eligible),
    disclaimers([
        "This monthly price is not an introductory rate and does not require a yearly contract",
        "Does not include AutoPay or other discounts",
        "One-time device connection charge: $35/line"
    ]),
    data_allowance(
        unlimited_data(yes),
        premium_data_gb(100),
        hotspot_data_gb(5),
        after_limit_speed(128)
    ),
    typical_speeds(
        download_mbps_range(89, 418),
        upload_mbps_range(6, 31),
        latency_ms_range(17, 32)
    ),
    included_benefits([
        "Unlimited talk and text",
        "Unlimited data",
        "Taxes & fees included",
        "Apple TV+ on us for 6 months",
        "Netflix Standard with ads ON US"
    ]),
    additional_charges_and_terms([
        device_connection_charge(35),
        early_termination_fee(0),
        government_taxes(included_in_monthly_price)
    ]),
    notes("AutoPay discount with eligible payment method is not reflected in these prices")
).
plan(
    title("Magenta® Military"),
    unique_plan_id("M0006945950PPI000000000096"),
    category("MILITARY"),
    lines_allowed(min_lines(1), max_lines(12)),
    monthly_prices([
        lines_price(1, 60),
        lines_price(2, 90),
        lines_price(3, 105),
        lines_price(4, 120),
        lines_price(5, 135),
        lines_price(6, 150),
        % Each additional line from 7 to 12 is +$25/line
        lines_price(7, 175),
        lines_price(8, 200),
        lines_price(9, 225),
        lines_price(10, 250),
        lines_price(11, 275),
        lines_price(12, 300)
    ]),
    taxes_and_fees_included(yes),
    auto_pay_discount(eligible),
    disclaimers([
        "This monthly price is not an introductory rate and does not require a yearly contract",
        "Does not include AutoPay or other discounts",
        "One-time device connection charge: $35/line"
    ]),
    data_allowance(
        unlimited_data(yes),
        premium_data_gb(100),
        hotspot_data_gb(5),
        after_limit_speed(128)
    ),
    typical_speeds(
        download_mbps_range(89, 418),
        upload_mbps_range(6, 31),
        latency_ms_range(17, 32)
    ),
    included_benefits([
        "Unlimited talk and text",
        "Unlimited 5G & 4G LTE with 100GB Premium Data",
        "5GB of high-speed mobile hotspot data",
        "Taxes & fees included",
        "Apple TV+ ON US for 6 months",
        "Netflix Standard with ads ON US"
    ]),
    additional_charges_and_terms([
        device_connection_charge(35),
        early_termination_fee(0),
        government_taxes(included_in_monthly_price)
    ]),
    notes("AutoPay discount with eligible payment method is not reflected in these prices")
).
plan(
      title("Magenta® First Responder"),
      unique_plan_id("M0006945950PPI000000000091"),
      category("FIRST_RESPONDER"),
      lines_allowed(min_lines(1), max_lines(12)),
      monthly_prices([
        lines_price(1, 60),
        lines_price(2, 90),
        lines_price(3, 105),
        lines_price(4, 120),
        lines_price(5, 135),
        lines_price(6, 150),
        % Each additional line from 7 to 12 is +$25/line
        lines_price(7, 175),
        lines_price(8, 200),
        lines_price(9, 225),
        lines_price(10, 250),
        lines_price(11, 275),
        lines_price(12, 300)
      ]),
      taxes_and_fees_included(yes),
      auto_pay_discount(eligible),
      disclaimers([
        "This monthly price is not an introductory rate and does not require a yearly contract",
        "Does not include AutoPay or other discounts",
        "One-time device connection charge: $35/line"
      ]),
      data_allowance(
        unlimited_data(yes),
        premium_data_gb(100),
        hotspot_data_gb(0),
        after_limit_speed(128)
      ),
      typical_speeds(
        download_mbps_range(89, 418),
        upload_mbps_range(6, 31),
        latency_ms_range(17, 32)
      ),
      included_benefits([
        "Unlimited talk and text",
        "Unlimited data",
        "Taxes & fees included",
        "Apple TV+ for 6 months ON US",
        "Netflix Standard with ads ON US"
      ]),
      additional_charges_and_terms([
        device_connection_charge(35),
        early_termination_fee(0),
        government_taxes(included_in_monthly_price)
      ]),
      notes("AutoPay discount with eligible payment method is not reflected in these prices")
).
plan(
    title("Magenta® 55+"),
    unique_plan_id("M0006945950PPI000000000090"),
    category("55+"),
    lines_allowed(min_lines(1), max_lines(4)),
    monthly_prices([
        lines_price(1, 55),
        lines_price(2, 80),
        lines_price(3, 120),
        lines_price(4, 160)
    ]),
    taxes_and_fees_included(yes),
    auto_pay_discount(eligible),
    disclaimers([
        "This monthly price is not an introductory rate and does not require a yearly contract",
        "Does not include AutoPay or other discounts",
        "One-time device connection charge: $35/line"
    ]),
    data_allowance(
        unlimited_data(yes),
        premium_data_gb(100),
        hotspot_data_gb(5),
        after_limit_speed(128)
    ),
    typical_speeds(
        download_mbps_range(89, 418),
        upload_mbps_range(6, 31),
        latency_ms_range(17, 32)
    ),
    included_benefits([
        "Unlimited talk and text",
        "Unlimited premium data (100GB included)",
        "Taxes & fees included",
        "5GB high-speed mobile hotspot",
        "Apple TV+ ON US for 6 months",
        "Netflix Standard with ads ON US"
    ]),
    additional_charges_and_terms([
        device_connection_charge(35),
        early_termination_fee(0),
        government_taxes(included_in_monthly_price)
    ]),
    notes("AutoPay discount with eligible payment method is not reflected in these prices")
).
plan(
    title("Essentials 55+"),
    unique_plan_id("M0006945950PPI000000000075"),
    category("55+"),
    lines_allowed(min_lines(1), max_lines(2)),
    monthly_prices([
        lines_price(1, 45),
        lines_price(2, 65)
    ]),
    taxes_and_fees_included(no),
    auto_pay_discount(eligible),
    disclaimers([
        "This monthly price is not an introductory rate and does not require a yearly contract",
        "Does not include AutoPay or other discounts",
        "Provider monthly fees (Regulatory programs / Telco recovery fee $3.49/line, Federal & Local Surcharges typically $0.20–$4.25/line)",
        "One-time device connection charge: $35/line"
    ]),
    data_allowance(
        unlimited_data(yes),
        premium_data_gb(50),
        hotspot_data_gb(0),
        after_limit_speed(128)
    ),
    typical_speeds(
        download_mbps_range(79, 357),
        upload_mbps_range(6, 30),
        latency_ms_range(17, 32)
    ),
    included_benefits([
        "Unlimited talk and text",
        "50GB of Premium Data",
        "No annual service contract"
    ]),
    additional_charges_and_terms([
        device_connection_charge(35),
        early_termination_fee(0),
        government_taxes(varies_by_location)
    ]),
    notes("AutoPay discount with eligible payment method is not reflected in these prices")
).
plan(
    title("Magenta® Puerto Rico"),
    unique_plan_id("M0006945950PPI000000000099"),
    category("CONSUMER"),
    lines_allowed(min_lines(1), max_lines(3)),
    monthly_prices([
        lines_price(1, 65),
        lines_price(2, 110),
        lines_price(3, 155)
    ]),
    taxes_and_fees_included(yes),
    auto_pay_discount(eligible),
    disclaimers([
        "This monthly price is not an introductory rate and does not require a yearly contract",
        "Does not include AutoPay or other discounts",
        "One-time device connection charge: $35/line"
    ]),
    data_allowance(
        unlimited_data(yes),
        premium_data_gb(100),
        hotspot_data_gb(0),
        after_limit_speed(not_specified)
    ),
    typical_speeds(
        download_mbps_range(89, 418),
        upload_mbps_range(6, 31),
        latency_ms_range(17, 32)
    ),
    included_benefits([
        "Unlimited talk and text",
        "5G & 4G LTE with 100GB Premium Data",
        "Taxes & fees included",
        "Apple TV+ ON US for 6 months"
    ]),
    additional_charges_and_terms([
        device_connection_charge(35),
        early_termination_fee(0),
        government_taxes(included_in_monthly_price)
    ]),
    notes("AutoPay discount with eligible payment method is not reflected in these prices")
).
plan(
    title("Simple Choice Family local Unlimited Talk, Text and Data"),
    unique_plan_id("M0006945950PPI000000000100"),
    category(""),
    lines_allowed(min_lines(2), max_lines(12)),
    monthly_prices([
        % Per the plan’s own table, 2–5 lines have fixed prices;
        lines_price(2, 70),
        lines_price(3, 90),
        lines_price(4, 110),
        lines_price(5, 130),
        % lines 6–12 cost an extra $20 each
        lines_price(6, 150),
        lines_price(7, 170),
        lines_price(8, 190),
        lines_price(9, 210),
        lines_price(10, 230),
        lines_price(11, 250),
        lines_price(12, 270)
    ]),
    taxes_and_fees_included(no),
    auto_pay_discount(not_included),
    disclaimers([
        "This monthly price is not an introductory rate and does not require a yearly contract",
        "Does not include AutoPay or other discounts",
        "Taxes and fees not included"
    ]),
    data_allowance(
        % 6GB high-speed data, then unlimited at 128kbps
        unlimited_data(yes),
        premium_data_gb(6),
        hotspot_data_gb(6),
        after_limit_speed(128)
    ),
    typical_speeds(
        download_mbps_range(89, 418),
        upload_mbps_range(6, 31),
        latency_ms_range(17, 32)
    ),
    included_benefits([
        "Unlimited talk and text"
    ]),
    additional_charges_and_terms([
        device_connection_charge(35),
        early_termination_fee(0),
        government_taxes(varies_by_location)
    ]),
    notes("Charges for additional data usage are $0.")
).
plan(
      title("Puerto Rico Simple Choice Local Unlimited Talk, Text and Data"),
      unique_plan_id("M0006945950PPI000000000100"),
      category(""),
      lines_allowed(min_lines(1), max_lines(12)),
      monthly_prices([
        lines_price(1, 40),
        lines_price(2, 70),
        lines_price(3, 90),
        lines_price(4, 110),
        lines_price(5, 130),
        % Each additional line from 6 to 12 is +$20/line
        lines_price(6, 150),
        lines_price(7, 170),
        lines_price(8, 190),
        lines_price(9, 210),
        lines_price(10, 230),
        lines_price(11, 250),
        lines_price(12, 270)
      ]),
      taxes_and_fees_included(no),
      auto_pay_discount(eligible),
      disclaimers([
        "This monthly price is not an introductory rate and does not require a yearly contract",
        "Does not include AutoPay or other discounts",
        "One-time device connection charge: $35/line"
      ]),
      data_allowance(
        unlimited_data(yes),
        premium_data_gb(6),
        hotspot_data_gb(0),
        after_limit_speed(128)
      ),
      typical_speeds(
        download_mbps_range(89, 418),
        upload_mbps_range(6, 31),
        latency_ms_range(17, 32)
      ),
      included_benefits([
        "Unlimited talk and text",
        "High-speed mobile hotspot data"
      ]),
      additional_charges_and_terms([
        device_connection_charge(35),
        early_termination_fee(0),
        government_taxes(varies_by_location)
      ]),
      notes("There may be additional billing discounts available.")
).
"""

let sysMsg = """
You are a Prolog programmer. A  Prolog prototype class for a phone plan is give in TEMPLATE below.
You are to convert a natural language query and other supporting information into a
Prolog query to answer the posed natural language question based on the Prolog database that conforms to the TEMPLATE.
TEMPLATE```
plan(
  plan_info(
    title("Go5G Next Military"),
    unique_plan_id("M0006945950PPI000000000084"),
    category("MILITARY"),
    lines_allowed(min_lines(1), max_lines(6)),
    monthly_prices([
      lines_price(1, 90),
      lines_price(2, 140),
      lines_price(3, 180),
      lines_price(4, 220),
      lines_price(5, 260),
      lines_price(6, 300)
      % Additional lines (7–12) mentioned as $50/line not explicitly listed here
    ]),
    taxes_and_fees_included(yes),
    auto_pay_discount(eligible),  % price shown excludes AutoPay discount
    disclaimers([
      "This monthly price is not an introductory rate and does not require a yearly contract",
      "Does not include AutoPay or other discounts",
      "One-time device connection charge: $35/line"
    ]),
    data_allowance(
      unlimited_data(yes),
      premium_data_gb(unlimited),  % plan explicitly mentions unlimited premium data
      hotspot_data_gb(50),         % plan mentions 50GB of high-speed hotspot
      after_limit_speed(not_specified)
    ),
    typical_speeds(
      download_mbps_range(89, 418),
      upload_mbps_range(6, 31),
      latency_ms_range(17, 32)
    ),
    included_benefits([
      "5G access",
      "Unlimited talk and text",
      "Unlimited premium data",
      "50GB high-speed mobile hotspot",
      "Upgrade-ready every year",
      "Hulu with ads ON US",
      "Apple TV+ ON US",
      "Netflix Standard with ads ON US"
    ]),
    additional_charges_and_terms([
      device_connection_charge(35),
      early_termination_fee(0),
      government_taxes(included_in_monthly_price)
    ]),
    notes("AutoPay discount with eligible payment method is not reflected in the listed price")
  )
).
Just answer with Prolog code.
```prolog
"""

let listModule = """
pragma(optimize,true).


/// append (ResultList, List1, List2)
/// ResultList = List1 + List2
/// List1 + List2 = ResultList
/// 
append(List,[],List).
append([Item|Result],[Item|List1],List2) :-
    append(Result,List1,List2).


/// appendItem (ResultList, List, Item)
/// ResultList = List + Item
/// List + Item = ResultList
/// 
appendItem(ResultList,List,Item) :-
    append(ResultList,List,[Item]).


/// join (List, Left, Pivot, Right)
/// List = Left + Pivot + Right
/// 
join(List,Left,Pivot,Right) :-
    appendItem(L,Left,Pivot),
    append(List,L,Right).


/// Member (Item, List)
/// Item = member of List
/// 
member(Item,List) :-
    split(L,Item,R,List).


/// permute (Result, Items)
/// Result = permutation of Items
/// 
permute([Item],[Item]).
permute(Result,[Item|Items]) :-
    permute(P,Items),
    append(P,Left,Right),
    appendItem(R,Left,Item),
    append(Result,R,Right).


/// size (Size, List)
/// Size = size of List
/// List = list of Size
/// 
size(0,[]).
size(Size,[Item|Items]) :-
    size(S,Items),
    (Size := (S + 1)).


/// split (Left, Pivot, Right, List)
/// Left + Pivot + Right = List
/// 
split(Left,Pivot,Right,List) :-
    append(List,L,Right),
    appendItem(L,Left,Pivot).


/// reverse (Result, Items)
/// Result = reverse of Items
/// 
reverse([],[]).
reverse(Result,[Item|Items]) :-
    reverse(R,Items),
    appendItem(Result,R,Item).


/// prefix (Prefix, Items)
/// Prefix = prefix of Items
/// 
prefix(Prefix,Items) :-
    append(Items,Prefix,L).


/// suffix (Suffix, Items)
/// Suffix = suffix of Items
/// 
suffix(Suffix,Items) :-
    append(Items,L,Suffix).


/// Partition (LessEqual, Greater, Pivot, Items)
/// LessEqual = members of Items <= Pivot
/// Greater = members of Items > Pivot
/// 
partition([],[],Pivot,[]).
partition([Item|LessEqual],Greater,Pivot,[Item|Items]) :-
    (Item =< Pivot),
    partition(LessEqual,Greater,Pivot,Items).
partition(LessEqual,[Item|Greater],Pivot,[Item|Items]) :-
    (Item > Pivot),
    partition(LessEqual,Greater,Pivot,Items).


/// qsort (SortedItems, Items)
/// SortedItems = sorted list of Items
/// 
qsort([],[]).
qsort(Result,[Item|Items]) :-
    partition(LE,G,Item,Items),
    qsort(SortedLE,LE),
    qsort(SortedG,G),
    join(Result,SortedLE,Item,SortedG).


/// sequence (Items, Size)
/// Items = list of integers between 1 and Size, inclusive
/// 
sequence(Items,Size) :-
    sequence(Items,1,Size).


/// divide (Left, Right, Items, Count)
/// Left = first Count items of Items
/// Right = remaining items of Items
/// 
divide([],Items,Items,0).
divide([Item|Left],Right,[Item|Items],Count) :-
    greater(Count,0),
    (SubCount is subtract(Count,1)),
    divide(Left,Right,Items,SubCount).


/// shuffle (Result, Items, Count)
/// Result = Items shuffled Count times
/// 
shuffle(Items,Items,0).
shuffle(Result,Items,Count) :-
    greater(Count,0),
    (SubCount is subtract(Count,1)),
    shuffle(S,Items,SubCount),
    size(Size,Items),
    random(0,Size,R),
    divide(Left,Right,S,R),
    merge(Result,Right,Left).


/// merge (Result, Left, Right)
/// Result = merged results of Left and Right
/// 
merge([LeftItem,RightItem|Items],[LeftItem|Left],[RightItem|Right]) :-
    merge(Items,Left,Right).
merge([],[],[]).
merge(Items,Items,[]) :-
    size(Size,Items),
    greater(Size,0).
merge(Items,[],Items) :-
    size(Size,Items),
    greater(Size,0).


/// sequence (Items, Min, Max)
/// Items = list of integers between Min and Max, inclusive
/// 
sequence([Min|Items],Min,Max) :-
    less(Min,Max),
    (MinPlus is add(Min,1)),
    sequence(Items,MinPlus,Max).
sequence([MinMax],MinMax,MinMax).
"""