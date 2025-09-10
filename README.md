# FunStripe / FunStripeLite Integration Guide

**FunStripe** is an F# library that provides a functional wrapper around the Stripe API for payment processing. This guide demonstrates the essential patterns for integrating Stripe payments in your application.

This repository uses FunStripeLite NuGet-package, but usage is identical to full FunStripe, just with a few dependencies removed.


## Overview

This sample covers the most important use cases for a minimum viable product (MVP):

- Creating Stripe customers
- Setting up payment methods with Setup Intents
- Processing one-time payments with Payment Intents
- Handling webhooks for payment events
- Frontend integration with Stripe Elements

## Prerequisites

- .NET 6.0 or later
- Stripe account (test keys for development)
- FunStripeLite NuGet package

## Quick Start

### 1. Configuration

First, configure your Stripe keys in your application:

```fsharp
open FunStripe
open FunStripe.StripeModel
open FunStripe.StripeRequest

// Configure your Stripe keys (use test keys for development)
let stripePublishableKey = "pk_test_..."
let stripeSecretKey = "sk_test_..."

// Set up Stripe account configuration
type StripeAccount = 
    | Live
    | Test

let getStripeConfig account =
    match account with
    | Live -> ("pk_live_...", "sk_live_...")
    | Test -> (stripePublishableKey, stripeSecretKey)
```

### 2. Creating Customers

```fsharp
let createCustomer (firstName: string) (lastName: string) (email: string) =
    async {
        let customerRequest = {
            CustomerCreateRequest.Default with
                Email = Some email
                Name = Some $"{firstName} {lastName}"
                Description = Some "Sample customer"
        }
        
        let! result = StripeRequest.Customer.create customerRequest
        return result
    }
```

### 3. Setup Intents (for saving payment methods)

```fsharp
let createSetupIntent (customerId: string) =
    async {
        let setupRequest = {
            SetupIntentCreateRequest.Default with
                Customer = Some customerId
                PaymentMethodTypes = ["card"]
                Usage = Some SetupIntentUsage.OffSession
        }
        
        let! result = StripeRequest.SetupIntent.create setupRequest
        return result
    }
```

### 4. Payment Intents (for one-time payments)

```fsharp
let createPaymentIntent (amount: int64) (currency: string) (customerId: string) =
    async {
        let paymentRequest = {
            PaymentIntentCreateRequest.Default with
                Amount = amount
                Currency = currency
                Customer = Some (PaymentIntentCustomer'AnyOf.String customerId)
                PaymentMethodTypes = ["card"]
                ConfirmationMethod = PaymentIntentConfirmationMethod.Automatic
        }
        
        let! result = StripeRequest.PaymentIntent.create paymentRequest
        return result
    }
```

### 5. Frontend Integration

See the `frontend/` directory for complete examples of:
- Stripe Elements configuration
- Card setup forms
- Payment confirmation flows
- Error handling

### 6. Webhook Handling

See the `webhooks/` directory for examples of:
- Webhook endpoint setup
- Event verification
- Handling different event types

## Architecture Patterns

### Error Handling

FunStripeLite uses F# Result types for comprehensive error handling:

```fsharp
let handlePaymentResult result =
    match result with
    | Ok paymentIntent ->
        // Success - process the payment intent
        printfn $"Payment created: {paymentIntent.Id}"
    | Error stripeError ->
        // Handle the error appropriately
        printfn $"Error: {stripeError.StripeError.Message}"
```

### Async Operations

All Stripe operations are asynchronous and return `Async<Result<'T, StripeError>>`:

```fsharp
let processPayment() =
    async {
        let! customerResult = createCustomer "John" "Doe" "john@example.com"
        match customerResult with
        | Ok customer ->
            let! paymentResult = createPaymentIntent 2000L "usd" customer.Id
            return paymentResult
        | Error error ->
            return Error error
    }
```

## Project Structure

```
FunStripeLite.Sample/
├── README.md                 # This file
├── src/
│   ├── Program.fs           # Main sample application
│   ├── StripeService.fs     # Core Stripe operations
│   └── FunStripeLite.Sample.fsproj
├── frontend/
│   ├── index.html           # Sample payment form
│   ├── stripe-integration.js  # Stripe Elements integration
│   └── styles.css           # Basic styling
└── webhooks/
    ├── WebhookHandler.fs    # Webhook processing
    └── Events.fs            # Event type definitions
```

## Security Considerations

- **Never expose secret keys in frontend code** - only use publishable keys
- **Validate webhook signatures** to ensure events come from Stripe
- **Use HTTPS** for all payment-related endpoints
- **Implement proper error handling** to avoid exposing sensitive information

## Testing

Use Stripe's test environment and test card numbers:
- Successful payment: `4242424242424242`
- Declined payment: `4000000000000002` 
- 3D Secure required: `4000002500003155`

## Next Steps

For production applications, consider implementing:
- Customer portal for managing payment methods
- Subscription billing (if applicable)
- Advanced webhook handling (retries, idempotency)
- Multi-party payments and marketplace features
- Dispute handling workflows

## Resources

- [Stripe API Documentation](https://stripe.com/docs/api)
- [FunStripeLite on NuGet](https://www.nuget.org/packages/FunStripeLite/)
- [Stripe Elements Documentation](https://stripe.com/docs/stripe-js)