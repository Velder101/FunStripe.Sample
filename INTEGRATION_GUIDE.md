# Complete FunStripe / FunStripeLite Integration Guide

## Overview

This repository contains a complete sample application demonstrating how to integrate FunStripeLite for Stripe payment processing. The sample covers the most important use cases for building a minimum viable product (MVP) with online payments.

This repository uses FunStripeLite NuGet-package, but usage is identical to full FunStripe, just with a few dependencies removed.

## Project Structure

```
FunStripe.Sample/
├── README.md                   # Main documentation
├── INTEGRATION_GUIDE.md        # This file - complete integration guide
├── src/                        # F# Backend Application
│   ├── Program.fs             # Main sample with demonstrations
│   ├── StripeService.fs       # Core Stripe operations (mock implementation)
│   ├── IntegrationExample.fs  # Complete web API patterns (reference)
│   └── *.fsproj               # Project file
├── frontend/                   # Frontend Integration Examples
│   ├── index.html             # Sample payment forms
│   ├── stripe-integration.js  # Stripe Elements implementation
│   └── styles.css             # Professional styling
└── webhooks/                   # Webhook Processing
    ├── Events.fs              # Event type definitions
    └── WebhookHandler.fs      # Complete webhook processing
```

## Getting Started

### 1. Configure Your Environment

1. **Get Stripe Keys**: Sign up at [stripe.com](https://stripe.com) and get your API keys
2. **Update Configuration**: Replace placeholder keys in `StripeService.fs`:
   ```fsharp
   PublishableKey = "pk_test_your_actual_key_here"
   SecretKey = "sk_test_your_actual_secret_key_here"  
   ```
3. **Update Frontend**: Replace the key in `frontend/stripe-integration.js`:
   ```javascript
   const STRIPE_PUBLISHABLE_KEY = 'pk_test_your_actual_key_here';
   ```

### 2. Run the Sample

```bash
cd src
dotnet run
```

This will demonstrate:
- Customer creation
- Setup intents (for saving payment methods)
- Payment intents (for processing payments)
- Complete payment flows

### 3. Test the Frontend

Open `frontend/index.html` in a browser to see:
- Stripe Elements integration
- Card setup forms
- Payment processing flows
- Error handling examples

## Core Integration Patterns

### Backend Patterns (F#)

#### 1. Customer Management
```fsharp
let createCustomer firstName lastName email =
    async {
        // In real implementation, use Stripe.createCustomer
        // This returns Result<Customer, StripeError>
        let! result = Stripe.createCustomer account userId firstName lastName email
        return result
    }
```

#### 2. Payment Processing
```fsharp
let createPaymentIntent amount currency customerId =
    async {
        // In real implementation, use Stripe.createPaymentIntent
        let! result = Stripe.createPaymentIntent account amount currency customerId
        return result
    }
```

#### 3. Saving Payment Methods
```fsharp
let createSetupIntent customerId =
    async {
        // In real implementation, use Stripe.createSetupIntent
        let! result = Stripe.createSetupIntent account customerId
        return result
    }
```

### Frontend Patterns (JavaScript/TypeScript)

#### 1. Initialize Stripe Elements
```javascript
const stripe = Stripe('pk_test_...');
const elements = stripe.elements({
    appearance: { theme: 'stripe' },
    mode: 'payment', // or 'setup' for saving cards
    currency: 'usd',
    amount: 2000
});
```

#### 2. Create Payment Form
```javascript
const paymentElement = elements.create('payment');
paymentElement.mount('#payment-element');
```

#### 3. Process Payments
```javascript
const { error } = await stripe.confirmPayment({
    elements,
    confirmParams: {
        return_url: 'https://your-website.com/return'
    }
});
```

### Webhook Patterns (F#)

#### 1. Event Processing
```fsharp
let processWebhookEvent eventType eventData =
    async {
        match eventType with
        | PaymentIntentSucceeded ->
            // Update order status, send emails, fulfill orders
            let! result = handlePaymentSuccess paymentIntent
            return result
        | SetupIntentSucceeded ->
            // Save payment method, enable subscriptions
            let! result = handleSetupSuccess setupIntent
            return result
        // ... handle other events
    }
```

#### 2. Signature Verification
```fsharp
let verifyWebhookSignature endpointSecret signature payload =
    // Critical for security - verify events come from Stripe
    // Implementation in WebhookHandler.fs
```

## Architecture Recommendations

### Production Web API Structure

1. **Controllers/Endpoints**:
   - `POST /api/customers` - Create customers
   - `POST /api/payment-intents` - Create payment intents
   - `POST /api/setup-intents` - Create setup intents
   - `POST /webhooks/stripe` - Handle Stripe webhooks

2. **Service Layer**:
   - `StripeService` - Wraps FunStripeLite operations
   - `PaymentService` - Business logic for payments
   - `CustomerService` - Customer management
   - `WebhookService` - Event processing

3. **Database Integration**:
   - Store customer mappings (your user ID ↔ Stripe customer ID)
   - Track payment statuses and order history
   - Log webhook events for idempotency
   - Store payment method references

### Error Handling Strategy

1. **Stripe Errors**: Use FunStripe's Result types
```fsharp
match stripeResult with
| Ok data -> // Process successful result
| Error stripeError -> // Handle Stripe API errors
```

2. **Business Logic Errors**: Define your own error types
```fsharp
type PaymentError =
    | StripeApiError of StripeError
    | InsufficientFunds
    | InvalidCustomer
    | OrderNotFound
```

3. **Frontend Error Handling**: Show user-friendly messages
```javascript
if (error) {
    // Show error message to user
    showError(error.message);
} else {
    // Redirect to success page
    window.location.href = '/success';
}
```

## Security Best Practices

### API Keys
- ✅ Never expose secret keys in frontend code
- ✅ Use environment variables for keys
- ✅ Rotate keys regularly
- ✅ Use different keys for test/production

### Webhook Security
- ✅ Always verify webhook signatures
- ✅ Use HTTPS endpoints only
- ✅ Implement idempotency
- ✅ Store and replay events if needed

### Payment Security
- ✅ Never store card details yourself
- ✅ Use Stripe Elements for card collection
- ✅ Implement proper error handling
- ✅ Log security events

## Testing Strategy

### Test Cards (Stripe Test Mode)
- **Success**: `4242424242424242`
- **Decline**: `4000000000000002`
- **3D Secure**: `4000002500003155`
- **Insufficient Funds**: `4000000000009995`

### Test Scenarios
1. **Happy Path**: Successful payment flows
2. **Error Handling**: Failed payments, network errors
3. **Edge Cases**: Large amounts, international cards
4. **Security**: Invalid webhooks, tampered requests

### Automated Testing
```fsharp
[<Test>]
let ``should create customer successfully`` () =
    async {
        let! result = createCustomer "John" "Doe" "john@test.com"
        match result with
        | Ok customer -> Assert.IsNotEmpty(customer.Id)
        | Error error -> Assert.Fail($"Unexpected error: {error}")
    }
```

## Deployment Checklist

### Before Going Live
- [ ] Replace test keys with live Stripe keys
- [ ] Set up webhook endpoints with HTTPS
- [ ] Configure proper error monitoring
- [ ] Set up payment reconciliation
- [ ] Test all payment flows thoroughly
- [ ] Configure fraud prevention rules
- [ ] Set up customer support processes

### Monitoring & Alerts
- [ ] Payment success/failure rates
- [ ] Webhook delivery status
- [ ] API response times
- [ ] Error rates and patterns
- [ ] Revenue and transaction volume

## Common Integration Patterns

### Subscription Billing
```fsharp
// Create subscription with saved payment method
let createSubscription customerId priceId paymentMethodId =
    async {
        let subscriptionRequest = {
            Customer = customerId
            Items = [{ Price = priceId }]
            DefaultPaymentMethod = paymentMethodId
        }
        let! result = Stripe.createSubscription subscriptionRequest
        return result
    }
```

### Marketplace Payments
```fsharp
// Split payments between platform and vendors
let createConnectedAccount vendorInfo =
    async {
        let! result = Stripe.createAccount vendorInfo
        return result
    }
```

### Refund Processing
```fsharp
let processRefund paymentIntentId amount =
    async {
        let! result = Stripe.createRefund paymentIntentId amount
        return result
    }
```

## Getting Help

### Resources
- [FunStripeLite Documentation](https://github.com/SimonTreanor/FunStripe)
- [Stripe API Documentation](https://stripe.com/docs/api)
- [Stripe Elements Guide](https://stripe.com/docs/stripe-js)
- [Webhook Best Practices](https://stripe.com/docs/webhooks/best-practices)
- [Stripe test cards](https://docs.stripe.com/testing)

### Support
- Check the sample code in this repository
- Review the Stripe dashboard for payment details
- Use Stripe's test mode for safe experimentation
- Contact Stripe support for API-specific questions

---

This integration guide provides a complete foundation for building production-ready payment systems with FunStripeLite. Start with the basic patterns and gradually add more sophisticated features as your application grows.