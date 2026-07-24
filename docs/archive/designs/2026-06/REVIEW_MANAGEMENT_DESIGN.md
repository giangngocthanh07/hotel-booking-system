# Review Management - Design Document

## 1. Process Flow: Submit a Review

```mermaid
graph TD
    A["Guest: Access Booking History"] --> B["System: Check Booking Status"]
    B -- "Not Completed" --> C["Hide 'Submit Review' button"]
    B -- "Completed" --> D["Show 'Submit Review' button"]
    
    D --> E["Guest: Click 'Submit Review'"]
    E --> F["Display Form (Rating + Comment)"]
    F --> G["Guest: Submit Form"]
    
    G --> H["System: Backend Validation"]
    H --> I{"Already reviewed this booking?"}
    I -- "Yes" --> J["Error: One review per booking allowed"]
    I -- "No" --> K["ReviewService.SubmitReviewAsync"]
    
    K --> L["Save Review via UoW"]
    L --> M["Refresh Hotel Average Rating"]
    M --> N["Success Message & Return to History"]
```

## 2. Process Flow: View Reviews (Hotel Details)

```mermaid
graph TD
    A["Guest: View Hotel Details"] --> B["System: Call ReviewService.GetHotelReviewsAsync"]
    B --> C["Fetch Reviews for HotelId"]
    C --> D["Calculate Average Rating & Count"]
    D --> E["Map results to ReviewDTO"]
    E --> F["Display review list on UI"]
```

---

## 3. Wireframe (UI/UX Draft)

### 3.1. Submit Review Form (Modal)
```text
+-------------------------------------------------------+
|                SUBMIT YOUR REVIEW                     |
+-------------------------------------------------------+
|  Hotel: Hotel Daewoo Hanoi                            |
|  Stay Dates: 14/06 - 16/06/2026                       |
|                                                       |
|  Your Rating: [ * * * * * ] (5/5)                     |
|                                                       |
|  Comment:                                             |
|  [_________________________________________________]  |
|  [_________________________________________________]  |
|                                                       |
|  [ CANCEL ]                               [ SUBMIT ]  |
+-------------------------------------------------------+
```

### 3.2. Reviews List (In Hotel Details Page)
```text
+---------------------------------------------------------------------------------------+
|  CUSTOMER REVIEWS (8.5/10 - 120 Reviews)                                              |
+---------------------------------------------------------------------------------------+
|  John Doe      [ * * * * * ] (10/10)                                                  |
|  "Very clean room, friendly staff. Will come back!"                                   |
|  Posted: 14/06/2026                                                                   |
|  -----------------------------------------------------------------------------------  |
|  Jane Smith    [ * * * * ] (8/10)                                                     |
|  "Nice view, central location. Breakfast could be better."                            |
|  Posted: 12/06/2026                                                                   |
+---------------------------------------------------------------------------------------+
```

---

## 4. Technical Implementation Notes
- **Data Integrity:** Only allow reviews when Booking status is `Completed`.
- **Performance:** Use **Pagination** for hotels with thousands of reviews.
- **Security:** Sanitize comments to prevent XSS attacks.
- **Rating Calculation:** Average ratings are calculated dynamically for real-time accuracy.
