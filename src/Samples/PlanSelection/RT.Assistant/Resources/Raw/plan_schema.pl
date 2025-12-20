plan(
    "<plan title>",              % Title
    category("all"),
    prices([
      line(1, monthly_price(20), original_price(25)),
      line(2, monthly_price(40), original_price(26))
    ]),
    features(
      [
        feature(taxes_and_fees(
                desc("<summary of property values>"),
                included_in_monthly_price(yes | no)
            ),
            applies_to_lines([all | lines(LOWER_BOUND,UPPER_BOUND)])
        ),
        feature(autopay_monthly_discount(
               desc("<summary of property values>"),
               discount_per_line(5),
               lines_up_to(8),
               included_in_monthly_price(yes)
            ),
            applies_to_lines(all)
        ),
        feature(no_annual_service_contract(
               desc("<summary of property values>")
            ),
            applies_to_lines(all)
         ),
         feature(phone_upgrades(
               desc("<summary of property values>"),
               frequency_year(1)
            ),
            applies_to_lines(all)
         ),
         feature(wi_fi_calling(
               desc("<summary of property values>")
            ),
            applies_to_lines(all)
         ),
         feature(voicemail_to_text(
               desc("<summary of property values>"),
            ),
            applies_to_lines(all)
         ),
         feature(scam_shield_premium(
               desc("<summary of property values>"),
            ),
            applies_to_lines(all)
         ),
         feature(high_speed_data(
               desc("<summary of property values>"),
               unlimited(yes),
               premium_data_limited(yes,limit_gb(50))
            ),
            applies_to_lines(all)
         ),
         feature(access_5g_at_no_extra_cost(
               desc("<summary of property values>"),
            ),
            applies_to_lines(lines(1,1))
         ),
         feature(canada_and_mexico_included(
               desc("<summary of property values>"),
               high_speed_data_gb(0)
            ),
            applies_to_lines(all)
         ),
         feature(unlimited_talk_and_text(
               desc("<summary of property values>"),
            ),
            applies_to_lines(all)
         ),
         feature(unlimited_international_texting_from_home(
               desc("<summary of property values>"),
            ),
            applies_to_lines(all)
         ),
         feature(data_and_texting_while_abroad(
               desc("<summary of property values>"),             
               high_speed_data_limit_gb(5),
               high_speed_data_country_limit(yes,countries(11))
            ),
            applies_to_lines(all)
         ),
         feature(low_flat_rate_calling_while_abroad(
            desc("<summary of property values>")
            ),
            applies_to_lines(all)
         ),
         feature(mobile_hotspot(
               desc("<summary of property values>"),
               high_speed_data_limit_gb(15)
            ),
            applies_to_lines(all)
         ),
         feature(video_streaming_quality(
               desc("<summary of property values>"),
               quality_480p(yes),
               quality_720p(yes),
               quality_4k_uhd(no)
            ),
            applies_to_lines(all)
         ),
         feature(in_flight_connection(
               desc("<summary of property values>"),
               wifi_hours(unlimited),
               streaming_included(yes)
            ),
            applies_to_lines(all)
         ),
         feature(apple_tv_plus(
               desc("<summary of property values>"),
               included(yes | yes_for_6_months | no)
            ),
            applies_to_lines(all)
         ),
         feature(hulu(
               desc("<summary of property values>"),
               included(yes | no)
            ),
            applies_to_lines(all)
         ),
         feature(netflix(
                desc("<summary of property values>"),
                included(yes | no)               
            ),
            applies_to_lines(lines(2,2))
         ),
         feature(one_year_aaa_membership_on_us(
               desc("<summary of property values>")               
            ),
            applies_to_lines(all)
         ),
         feature(connect_telco_travel(
               desc("<summary of property values>")
            ),
            applies_to_lines(all)
         )                
      ]
    )   
).
