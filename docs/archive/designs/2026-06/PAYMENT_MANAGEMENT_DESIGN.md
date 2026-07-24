# Payment Management - Design Document

## 1. Process Flow: Checkout & Online Payment

```mermaid
graph TD
    A["Guest: Confirm Booking"] --> B["System: Double-check room availability (US03 Logic)"]
    B -- "Full" --> C["Notification Error & Stop"]
    B -- "Available" --> D["Send Request to API /create-payment-url"]
    
    D --> E["PaymentService: Create Payment record (Status: Pending)"]
    E --> F["PaymentService: Initialize Gateway URL (VNPay/Stripe)"]
    F --> G["Return Payment URL to Frontend"]
    
    G --> H["Guest: Redirect to Gateway Payment Page"]
    H --> I["Guest: Perform Payment"]
    I --> J["Gateway: Send result notification (Webhook / IPN)"]
    
    J --> K["System: Call API /payment-callback"]
    K --> L{Payment Successful?}
    
    L -- "No" --> M["Update Payment: Failed & Cancel Booking"]
    L -- "Yes" --> N["Update Payment: Success"]
    N --> O["Update Booking: Confirmed"]
    O --> P["Send Confirmation Email to Guest"]
```

---

## 2. Wireframe (UI/UX Draft)

### 2.1. Payment Method Selection (Part of Checkout)
```text
+-------------------------------------------------------+
|  CHOOSE PAYMENT METHOD                                |
+-------------------------------------------------------+
|  ( ) Pay at Hotel                                     |
|                                                       |
|  (x) Online Transfer (VNPay / Local Banks)            |
|      [ Logo VNPay ] [ Logo Banks... ]                 |
|                                                       |
|  ( ) International Card (Visa, Mastercard via Stripe) |
|      [ Logo Visa ] [ Logo Master ]                    |
+-------------------------------------------------------+
|                                  [ PAY NOW ]          |
+-------------------------------------------------------+
```

### 2.2. Transaction History (User Profile)
```text
+---------------------------------------------------------------------------------------+
|  TRANSACTION HISTORY                                                                  |
+---------------------------------------------------------------------------------------+
|  Date        | Booking ID | Method      | Amount      | Status     | Action           |
+--------------+------------+-------------+-------------+------------+------------------+
|  14/06/2026  | #BK-101    | VNPay       | 3,300,000 đ | [Success]  | [View Receipt]   |
|  10/05/2026  | #BK-095    | Cash        | 1,500,000 đ | [Pending]  | [Cancel]         |
+--------------+------------+-------------+-------------+------------+------------------+
```

---

## 3. Technical Implementation Details
- **Security:** Use **Checksum/Signature** to verify data integrity from Gateway.
- **Idempotency:** Ensure each transaction is processed only once to avoid duplicate payments.
- **Logging:** Log all Gateway Request/Response data for reconciliation.
- **Strategy Pattern:** Use Strategy Pattern for easy addition of new payment gateways.
