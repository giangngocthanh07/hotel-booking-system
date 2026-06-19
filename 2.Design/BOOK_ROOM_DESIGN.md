# US03: Book a Room - Design Document

## 1. Process Flow

```mermaid
graph TD
    A["Guest: Click 'Book Now' on room details"] --> B{"Logged in?"}
    B -- "No" --> C["Redirect to Login page"]
    B -- "Yes" --> D["Display Booking Summary"]
    
    D --> E["Guest: Confirm Booking"]
    E --> F["Send Request to API /post-booking"]
    
    F --> G["BookingService.CreateBookingAsync"]
    G --> H["Begin Transaction"]
    
    H --> I{"Check Room Availability - Lock Room"}
    I -- "Not available" --> J["Rollback & Error: Room fully booked"]
    I -- "Available" --> K["Recalculate Total Price"]
    
    K --> L["Save Booking record"]
    L --> M["Save BookingRoom details"]
    M --> N["Commit Transaction"]
    
    N --> O["Send Success Notification & Booking ID"]
    O --> P["Display success confirmation on UI"]
```

---

## 2. Wireframe (UI/UX Draft)

### 2.1. Booking Summary Page
```text
+---------------------------------------------------------------------------------------+
|  CONFIRM YOUR BOOKING                                                                 |
+---------------------------------------+-----------------------------------------------+
|  STAY DETAILS                         |  PRICE SUMMARY                                |
|  Hotel: Daewoo Hanoi                  |  Room: 1,500,000 VNĐ x 2 nights               |
|  Room: Superior Double Room           |  Tax (10%):  300,000 VNĐ                      |
|  Dates: 14/06 - 16/06 (2 nights)      |  -------------------------------------------  |
|  Guests: 2 Adults                     |  TOTAL PRICE: 3,300,000 VNĐ                   |
+---------------------------------------+-----------------------------------------------+
|  [ ] I agree to the hotel policies and terms.                                         |
|                                                                                       |
|  [ BACK ]                                                         [ CONFIRM ]         |
+---------------------------------------------------------------------------------------+
```

### 2.3. Success Page
```text
+---------------------------------------------------------------------------------------+
|       [ ICON SUCCESS ]                                                                |
|       THANK YOU FOR YOUR BOOKING!                                                     |
|       Your Reference ID is: #BK-123456                                                |
|       Details have been sent to your email: guest@example.com                         |
|                                                                                       |
|       [ VIEW BOOKING HISTORY ]                            [ BACK TO HOME ]            |
+---------------------------------------------------------------------------------------+
```
