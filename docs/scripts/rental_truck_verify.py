from decimal import Decimal, ROUND_HALF_UP


def money(value: Decimal) -> Decimal:
    """Round money for display using ordinary cents rounding."""
    return value.quantize(Decimal("0.01"), rounding=ROUND_HALF_UP)


def dollars(value: Decimal) -> str:
    """Format a Decimal as dollars rounded to cents."""
    return f"${money(value)}"


def raw_dollars(value: Decimal) -> str:
    """Format an unrounded Decimal as dollars for calculation walkthroughs."""
    return f"${value}"


def section(title: str) -> None:
    print()
    print(title)
    print("-" * len(title))


# Rental Truck Choice benchmark verifier
#
# Goal:
#   Compute the expected answer for the Model World math benchmark using
#   deterministic decimal arithmetic. This gives the instructor a local source
#   of truth for comparing model outputs.

# Scenario inputs from the prompt.
# Use Decimal values for money and rates so the calculations avoid binary
# floating-point surprises such as 0.1 + 0.2 style artifacts.
rental_days = Decimal("3")
planned_miles = Decimal("486")

# Plan A:
#   - $79 per day
#   - 100 included miles per day
#   - $0.59 for each mile beyond the included miles
#   - $35 coupon applied before tax
plan_a_daily_rate = Decimal("79")
plan_a_included_miles_per_day = Decimal("100")
plan_a_extra_mile_rate = Decimal("0.59")
plan_a_coupon = Decimal("35")

# Plan B:
#   - $109 per day
#   - unlimited miles
#   - 9.5% insurance fee on the daily charge
plan_b_daily_rate = Decimal("109")
plan_b_insurance_rate = Decimal("0.095")

# Both plans add sales tax after discounts, mileage charges, and fees.
sales_tax_rate = Decimal("0.0825")

# Plan A mileage accounting.
# The common mistake is to use 100 included miles total. The prompt says
# 100 miles per day, so a 3-day rental includes 300 miles.
plan_a_included_miles = plan_a_included_miles_per_day * rental_days
plan_a_extra_miles = max(Decimal("0"), planned_miles - plan_a_included_miles)

# Plan A subtotal before tax.
# The coupon is subtracted before tax, after adding the extra-mile charge.
plan_a_daily_charge = plan_a_daily_rate * rental_days
plan_a_extra_mile_charge = plan_a_extra_miles * plan_a_extra_mile_rate
plan_a_subtotal_before_tax = plan_a_daily_charge + plan_a_extra_mile_charge - plan_a_coupon
plan_a_total = plan_a_subtotal_before_tax * (Decimal("1") + sales_tax_rate)

# Plan B subtotal before tax.
# There are no mileage charges, but the insurance fee is added before tax.
plan_b_daily_charge = plan_b_daily_rate * rental_days
plan_b_insurance_fee = plan_b_daily_charge * plan_b_insurance_rate
plan_b_subtotal_before_tax = plan_b_daily_charge + plan_b_insurance_fee
plan_b_total = plan_b_subtotal_before_tax * (Decimal("1") + sales_tax_rate)

# Compare the two plans and round only for display.
# Keeping raw values until the end avoids small rounding differences.
savings = plan_b_total - plan_a_total

print("Rental Truck Choice benchmark verifier")
print("=======================================")
print(f"Rental length: {rental_days} days")
print(f"Planned distance: {planned_miles} miles")
print(f"Sales tax: {sales_tax_rate * Decimal('100')}%")

section("Plan A")
print(f"Included miles: {plan_a_included_miles_per_day} miles/day x {rental_days} days = {plan_a_included_miles} miles")
print(f"Extra miles: {planned_miles} planned - {plan_a_included_miles} included = {plan_a_extra_miles} miles")
print(f"Daily charge: {rental_days} days x ${plan_a_daily_rate}/day = {dollars(plan_a_daily_charge)}")
print(f"Extra-mile charge: {plan_a_extra_miles} miles x ${plan_a_extra_mile_rate}/mile = {dollars(plan_a_extra_mile_charge)}")
print(f"Coupon before tax: -{dollars(plan_a_coupon)}")
print(f"Subtotal before tax: {dollars(plan_a_daily_charge)} + {dollars(plan_a_extra_mile_charge)} - {dollars(plan_a_coupon)} = {dollars(plan_a_subtotal_before_tax)}")
print(f"After tax: {dollars(plan_a_subtotal_before_tax)} x 1.0825 = {raw_dollars(plan_a_total)} -> {dollars(plan_a_total)}")

section("Plan B")
print(f"Daily charge: {rental_days} days x ${plan_b_daily_rate}/day = {dollars(plan_b_daily_charge)}")
print(f"Insurance fee: {dollars(plan_b_daily_charge)} x 9.5% = {raw_dollars(plan_b_insurance_fee)} -> {dollars(plan_b_insurance_fee)}")
print(f"Subtotal before tax: {dollars(plan_b_daily_charge)} + {raw_dollars(plan_b_insurance_fee)} = {raw_dollars(plan_b_subtotal_before_tax)}")
print(f"After tax: {raw_dollars(plan_b_subtotal_before_tax)} x 1.0825 = {raw_dollars(plan_b_total)} -> {dollars(plan_b_total)}")

section("Comparison")
print(f"Plan A total: {dollars(plan_a_total)}")
print(f"Plan B total: {dollars(plan_b_total)}")
print(f"Savings: {raw_dollars(plan_b_total)} - {raw_dollars(plan_a_total)} = {raw_dollars(savings)} -> {dollars(savings)}")

section("Expected benchmark answer")
print(f"Plan A total: {dollars(plan_a_total)}")
print(f"Cheaper plan: Plan A by {dollars(savings)}")