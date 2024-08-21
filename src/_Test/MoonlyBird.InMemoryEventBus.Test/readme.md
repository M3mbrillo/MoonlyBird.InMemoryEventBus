
# Test - InMemoryBus


## Credit Payment

A payment with credit card produce a PaymentEvent and dispatch two process:
- Debit the price of the product
- Alert if the payment is greater than x amount
- Throw Exception if detect a scammer transaction


## RacingLog

Many LongTaskEvents and log when this end.
A long task event could not stop another event


## OverrideAndScopes

Two handlers must have a distinct scope for DI and the event (and nested props) could be immutable

