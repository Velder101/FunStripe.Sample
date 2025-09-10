module WebhookEvents

open System

/// Simplified representation of Stripe events for the sample
/// In a real implementation, you would use FunStripe.StripeModel types

type MockPaymentIntent = {
    Id: string
    Amount: int64
    Currency: string
    CustomerId: string option
}

type MockSetupIntent = {
    Id: string
    CustomerId: string option
    PaymentMethodId: string option
}

type MockCustomer = {
    Id: string
    Email: string option
    Name: string option
}

/// Represents the different types of Stripe events we handle
type StripeEventType =
    | PaymentIntentSucceeded
    | PaymentIntentPaymentFailed
    | SetupIntentSucceeded
    | CustomerCreated
    | ChargeSucceeded
    | InvoicePaymentSucceeded
    | Other of string

// We leave disputes out for now, but they are important if you have a service where
// your customers want to cancel their payments afterwards.

/// Convert string to event type
let parseEventType (eventType: string) =
    match eventType with
    | "payment_intent.succeeded" -> PaymentIntentSucceeded
    | "payment_intent.payment_failed" -> PaymentIntentPaymentFailed
    | "setup_intent.succeeded" -> SetupIntentSucceeded
    | "customer.created" -> CustomerCreated
    | "charge.succeeded" -> ChargeSucceeded
    | "invoice.payment_succeeded" -> InvoicePaymentSucceeded
    | other -> Other other

/// Event processing result
type EventProcessingResult =
    | Success of string
    | Error of string
    | Ignored of string

/// Business logic for handling payment completion
let handlePaymentSuccess (paymentIntent: MockPaymentIntent) =
    async {
        // In a real application, you would:
        // 1. Update your database to mark the order as paid
        // 2. Send confirmation emails
        // 3. Trigger fulfillment processes
        // 4. Update customer account status

        printfn $"Processing successful payment: {paymentIntent.Id}"
        printfn $"Amount: {paymentIntent.Amount} {paymentIntent.Currency}"

        match paymentIntent.CustomerId with
        | Some customerId ->
            printfn $"Customer: {customerId}"
            // Update customer's order history
            // Send confirmation email
            return Success $"Payment {paymentIntent.Id} processed successfully for customer {customerId}"
        | None ->
            return Success $"Payment {paymentIntent.Id} processed successfully (no customer)"
    }

/// Business logic for handling payment failures
let handlePaymentFailure (paymentIntent: MockPaymentIntent) =
    async {
        printfn $"Processing failed payment: {paymentIntent.Id}"

        // In a real application, you would handle the failure details
        printfn "Logging failure for analysis"
        // Possibly notify customer service
        // Update order status to failed

        return Success $"Payment failure {paymentIntent.Id} processed"
    }

/// Business logic for handling successful setup intents
let handleSetupSuccess (setupIntent: MockSetupIntent) =
    async {
        printfn $"Processing successful setup intent: {setupIntent.Id}"

        match setupIntent.CustomerId with
        | Some customerId ->
            match setupIntent.PaymentMethodId with
            | Some paymentMethodId ->
                printfn $"Payment method {paymentMethodId} saved for customer {customerId}"
                // Store the payment method reference in your database
                // Enable subscription features for the customer
                // Send confirmation that payment method was saved
                return Success $"Setup intent {setupIntent.Id} processed - payment method saved"
            | None ->
                return Success $"Setup intent {setupIntent.Id} processed"
        | None ->
            return Error $"Setup intent {setupIntent.Id} succeeded but no customer"
    }

/// Business logic for handling customer creation
let handleCustomerCreated (customer: MockCustomer) =
    async {
        printfn $"Processing new customer: {customer.Id}"
        let email = customer.Email |> Option.defaultValue "N/A"
        printfn $"Email: {email}"

        // In a real application:
        // 1. Update your user management system
        // 2. Send welcome emails
        // 3. Set up customer portal access
        // 4. Initialize customer preferences

        return Success $"Customer {customer.Id} creation processed"
    }
