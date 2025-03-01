
plan("Connect Plus", category(all), lines([line(1, monthly_price(90), original_price(95)), line(2, monthly_price(150), original_price(160)), line(3, monthly_price(150), original_price(200)), line(4, monthly_price(185), original_price(240)), line(5, monthly_price(220), original_price(280))]), features([feature(taxes_and_fees(desc("Taxes and fees are included in the monthly price"), included_in_monthly_price(yes)), applies_to_lines(all)), feature(autopay_monthly_discount(desc("$5 disc. per line up to 8 lines w/AutoPay & eligible payment method."), discount_per_line(5), lines_up_to(8), included_in_monthly_price(yes)), applies_to_lines(all)), feature(no_annual_service_contract(desc("Included")), applies_to_lines(all)), feature(phone_upgrades(desc("Upgrade-ready every two years"), frequency_year(2)), applies_to_lines(all)), feature(wi_fi_calling(desc("Included")), applies_to_lines(all)), feature(voicemail_to_text(desc("Included")), applies_to_lines(all)), feature(scam_shield_premium(desc("Scam Shield Premium")), applies_to_lines(all)), feature(high_speed_data(desc("Unlimited 5G & 4G LTE with Unlimited Premium Data"), unlimited(yes), premium_data_limited(yes, limit_gb(50))), applies_to_lines(all)), feature(access_5g_at_no_extra_cost(desc("Included")), applies_to_lines(lines(1, 1))), feature(canada_and_mexico_included(desc("Unl. talk, text, & up to 15GB of high-speed data"), high_speed_data_gb(15)), applies_to_lines(all)), feature(unlimited_talk_and_text(desc("Unlimited")), applies_to_lines(all)), feature(unlimited_international_texting_from_home(desc("Unlimited international text from home")), applies_to_lines(all)), feature(data_and_texting_while_abroad(desc("Unlimited text & up to 5GB of high-speed data, then unlimited data at up to 256Kbps in 215+ countries & destinations"), high_speed_data_limit_gb(5), high_speed_data_country_limit(yes, countries(215))), applies_to_lines(all)), feature(low_flat_rate_calling_while_abroad(desc("$0.25/minute in 215+ countries & destinations")), applies_to_lines(all)), feature(mobile_hotspot(desc("50GB of high-speed mobile hotspot data included. Unlimited mobile hotspot data at max 3G speeds after 50GB."), high_speed_data_limit_gb(50)), applies_to_lines(all)), feature(video_streaming_quality(desc("Up to 4K UHD video"), quality_480p(yes), quality_720p(yes), quality_4k_uhd(yes)), applies_to_lines(all)), feature(in_flight_connection(desc("Unlimited in-flight texting & streaming where available"), wifi_hours(unlimited), streaming_included(yes)), applies_to_lines(all)), feature(apple_tv_plus(desc("Included"), included(yes)), applies_to_lines(all)), feature(hulu(desc("Not included"), included(no)), applies_to_lines(all)), feature(netflix(desc("Netflix Standard with Ads On Us"), included(yes)), applies_to_lines(lines(1, 5))), feature(one_year_aaa_membership_on_us(desc("Included")), applies_to_lines(all)), feature(connect_telco_travel(desc("No information available")), applies_to_lines(all))])).


