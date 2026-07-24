# Room Management - Design Document

## 1. Process Flow: Create Room Type

```mermaid
graph TD
    A["Owner: Click 'Add Room Type'"] --> B["System: Display Create Room Type Form"]
    B --> C["Owner: Input Name, Price, Capacity"]
    C --> D["Owner: Choose Bed Configuration (e.g., 1 King, 2 Single)"]
    D --> E["System: Call RoomNameSuggestionService (Optional)"]
    
    E --> F["Owner: Submit Form"]
    F --> G["System: Backend Validation (FluentValidation)"]
    
    G -- "Fail" --> H["Display Validation Errors"]
    G -- "Pass" --> I["RoomTypeService.CreateRoomTypeAsync"]
    
    I --> J["Begin Transaction"]
    J --> K{"Check Duplicate Name for Hotel?"}
    
    K -- "Yes" --> L["Rollback & Return Error: Name exists"]
    K -- "No" --> M["Save RoomType Entity"]
    
    M --> N["Save Bed Configurations"]
    N --> O["Commit Transaction"]
    
    O --> P["Success Message & Return to list"]
```

## 2. Process Flow: Manage Room Inventory (Batch Add)

```mermaid
graph TD
    A["Owner: Select Room Type"] --> B["Owner: Click 'Add Physical Rooms'"]
    B --> C["Owner: Input list of room numbers (e.g., 101, 102, 103)"]
    C --> D["System: Backend Validation"]
    
    D --> E{"Check Duplicate Room Numbers in Hotel?"}
    E -- "Yes" --> F["Error: Room numbers already exist"]
    E -- "No" --> G["RoomService.AddRoomsAsync"]
    
    G --> H["Begin Transaction"]
    H --> I["Save list of Room Entities"]
    I --> J["Commit Transaction"]
    
    J --> K["Refresh Inventory & Success Notification"]
```

---

## 3. Wireframe (UI/UX Draft)

### 3.1. Room Types Listing (Owner Portal)
```text
+---------------------------------------------------------------------------------------+
|  [ OWNER PORTAL ]                                      [ Notification ] [ Profile ]   |
+-----------------+---------------------------------------------------------------------+
| SIDEBAR         |  MY HOTEL: DAEWOO HANOI                                             |
|                 +---------------------------------------------------------------------+
| [D] Dashboard   |  [ + ADD NEW ROOM TYPE ]                                            |
| [H] My Hotels   |                                                                     |
| [R] Room Types  |  EXISTING ROOM TYPES:                                               |
| [B] Bookings    |  +----------------------+------------+-------------+-------------+  |
|                 |  | Room Type Name       | Price/Day  | Capacity    | Actions     |  |
| [S] Settings    |  +----------------------+------------+-------------+-------------+  |
|                 |  | Deluxe City View     | 1.500.000  | 2 Adults    | [Edit] [Del]|  |
|                 |  | Executive Suite      | 3.500.000  | 3 Adults    | [Edit] [Del]|  |
|                 |  +----------------------+------------+-------------+-------------+  |
+-----------------+---------------------------------------------------------------------+
```

### 3.2. Create Room Type Modal
```text
+---------------------------------------------------------------------------------------+
|  CREATE NEW ROOM TYPE                                                                 |
+---------------------------------------------------------------------------------------+
|  Room Name: [ Sea View Deluxe         ]  [ SUGGEST NAMES ]                            |
|  Description: [ Luxury room with balcony...                                       ]  |
|  Price/Night: [ 2.000.000 ]   Currency: [ VNĐ ]                                       |
+---------------------------------------+-----------------------------------------------+
|  BED CONFIGURATION                    |  CAPACITY                                     |
|  [+] 1 x King Bed                     |  Adults: [ 2 ]                                |
|  [-] 2 x Single Beds                  |  Children: [ 1 ]                              |
+---------------------------------------+-----------------------------------------------+
|  AMENITIES                            |  IMAGES                                       |
|  [x] WiFi  [x] AC  [ ] Mini Bar       |  [ UPLOAD IMAGES... ]                         |
+---------------------------------------+-----------------------------------------------+
|                                                           [ CANCEL ]      [ SAVE ]    |
+---------------------------------------------------------------------------------------+
```

### 3.3. Manage Room Inventory (Modal)
```text
+---------------------------------------------------------------------------------------+
|  MANAGE ROOMS: Deluxe City View                                                       |
+---------------------------------------------------------------------------------------+
|  CURRENT ROOMS: [ 101 ] [ 102 ] [ 103 ] [ 104 (Maintenance) ]                         |
|                                                                                       |
|  [ + BATCH ADD ROOMS ]                                                                |
|  Enter Room Numbers (separated by comma):                                             |
|  [ 201, 202, 203, 204, 205                                                        ]  |
|                                                                                       |
|  [ ] Auto-assign 'Active' status to new rooms.                                        |
+---------------------------------------------------------------------------------------+
|                                                           [ CANCEL ]      [ ADD ]     |
+---------------------------------------------------------------------------------------+
```

---

## 4. Technical Implementation Details
- **Atomicity:** `RoomType` and `RoomTypeBedConfig` must be saved in the same transaction.
- **Data Normalization:** Room names are trimmed and case-normalized before uniqueness checks.
- **Ghost ID Validation:** Use `RoomAttributeFacade` to verify foreign keys (UnitType, BedType, etc.) before business logic.
- **Additional Data:** Metadata (total rooms, smoking status) stored as JSON in `Additional` column for flexibility.
