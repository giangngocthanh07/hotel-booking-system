# Hotel Booking - User Stories & Acceptance Criteria

## Group 1: Guest (Search & Discovery)

### US01: Search for Hotels

**As a** Guest  
**I want to** search for hotels by location, dates, and guest count  
**So that** I can find available accommodation for my trip.

**Acceptance Criteria:**

- [ ] Search form allows inputting City/Province, Check-in/Check-out dates, and number of Adults/Children.
- [ ] Results show hotels with available rooms for the selected criteria.
- [ ] Results display key info: Hotel Name, Star Rating, Price per night, and Thumbnail image.
- [ ] System handles "No results found" gracefully.

### US02: View Hotel Details

**As a** Guest  
**I want to** view detailed information about a specific hotel  
**So that** I can decide if it meets my needs.

**Acceptance Criteria:**

- [ ] Display all hotel images in a gallery.
- [ ] Show hotel description, amenities, and location on a map.
- [ ] List available room types with their specific prices and features.
- [ ] Display recent reviews and average rating.

---

## Group 2: Guest (Booking & Payment)

### US03: Book a Room

**As a** Guest  
**I want to** book a specific room in a hotel  
**So that** I can secure my stay.

**Acceptance Criteria:**

- [ ] User must be logged in to book.
- [ ] Booking summary shows total price, dates, and room details before confirmation.
- [ ] Room availability is validated again at the moment of booking (to prevent race conditions).
- [ ] Successful booking generates a unique Booking Reference ID.

---

## Group 3: Hotel Owner (Management)

### US04: Register a Hotel

**As a** Hotel Owner  
**I want to** register my hotel on the platform  
**So that** I can start receiving bookings.

**Acceptance Criteria:**

- [ ] Provide hotel name, address, contact info, and legal documents.
- [ ] Upload hotel images and select amenities.
- [ ] Status is set to "Pending Approval" upon submission.

---

## Group 4: Admin (System Control)

### US06: Approve Hotel Registration

**As an** Admin  
**I want to** review and approve/reject hotel registrations  
**So that** only quality and verified hotels are listed.

**Acceptance Criteria:**

- [ ] View list of "Pending" hotel registrations.
- [ ] View uploaded documents and hotel details.
- [ ] Option to Approve (makes hotel visible to guests) or Reject (with a reason).

---

## Group 5: Admin (System Configuration & Master Data)

### US07: Manage Master Data Modules

**As an** Admin  
**I want to** manage system-wide categories (Amenities, Services, Policies, Room Attributes)  
**So that** Hotel Owners can select standard options for their listings.

**Acceptance Criteria:**

- [ ] Admin can view a dynamic menu based on the selected module (Service, Amenity, etc.).
- [ ] Each module displays its respective "Types" (e.g., Amenity Types like "In-room", "Food & Drink").
- [ ] Admin can add, edit, and soft-delete items within each module.
- [ ] System prevents deleting items that are currently in use by hotels (integrity check).

---

## Group 6: Request Management (Approvals & Upgrades)

### US09: Request Account Upgrade

**As a** Customer  
**I want to** submit a request to upgrade my account to "Hotel Owner"  
**So that** I can list and manage my hotels on the platform.

**Acceptance Criteria:**

- [ ] Customer provides necessary documents/reason for the upgrade.
- [ ] Status is set to "Pending" upon submission.
- [ ] Customer can view the status and history of their upgrade requests.

### US10: Review Upgrade Requests (Admin)

**As an** Admin  
**I want to** review and approve or reject user upgrade requests  
**So that** only verified users can become hotel owners.

**Acceptance Criteria:**

- [ ] Admin can view a list of all pending upgrade requests with user details.
- [ ] Admin can Approve (changes user role to Owner) or Reject (with a reason).

### US12: Request Overview Dashboard (Admin)

**As an** Admin  
**I want to** see an overview of all system requests (Upgrades & Approvals)  
**So that** I can track total, pending, and completed tasks at a glance.

**Acceptance Criteria:**

- [ ] Display statistics: Total, Pending, Approved, Rejected.
- [ ] List most recent requests across all categories.

---

## Group 7: Room Management (Hotel Owners)

### US13: Manage Room Types

**As a** Hotel Owner  
**I want to** define different types of rooms available in my hotel (e.g., Deluxe, Suite)  
**So that** guests can choose the accommodation that fits their budget and needs.

**Acceptance Criteria:**

- [ ] Owner can create a new Room Type with Name, Description, Price per Night, and Capacity.
- [ ] Owner can configure Bed Types for each Room Type.
- [ ] System suggests creative room names based on attributes.

### US14: Room Inventory & Availability

**As a** Hotel Owner  
**I want to** manage the total number of physical rooms for each Room Type  
**So that** I don't overbook and can track real-time occupancy.

**Acceptance Criteria:**

- [ ] Owner can specify the total number of rooms for a Room Type.
- [ ] System automatically tracks available rooms based on active bookings.
- [ ] Owner can manually block certain rooms for maintenance.

---

## Group 8: Media Management (Images & Files)

### US15: Upload Hotel Images

**As a** Hotel Owner  
**I want to** upload multiple images for my hotel  
**So that** guests can visualize the property before booking.

**Acceptance Criteria:**

- [ ] Owner can upload a "Cover Image" and multiple gallery images.
- [ ] System optimizes images during upload.
- [ ] Images are organized in folders in the cloud storage.

---

## Group 9: User Management (Authentication & Profile)

### US18: User Registration

**As a** Guest  
**I want to** create a new account  
**So that** I can access personalized features.

**Acceptance Criteria:**

- [ ] User provides credentials and basic info.
- [ ] System validates uniqueness of Username and Email.
- [ ] Default role is "Customer".

### US19: User Login

**As a** Registered User  
**I want to** log in to my account  
**So that** I can manage my bookings and profile.

**Acceptance Criteria:**

- [ ] User provides credentials.
- [ ] Successful login returns a JWT.

### US19a: Forgot Password

### US20: Manage Profile

**As a** Logged-in User  
**I want to** view and update my profile information  
**So that** my contact details stay up to date.

**Acceptance Criteria:**

- [ ] User can view their current details (Full Name, Email, Phone, Avatar).
- [ ] User can update their profile information.
- [ ] Changes are persisted to the database.
- [ ] Access is restricted to the owner of the profile (via JWT).

---

## Group 10: Review Management (Feedback & Ratings)

### US21: Submit a Review

**As a** Guest who has completed a stay  
**I want to** rate and review the hotel  
**So that** I can share my experience.

**Acceptance Criteria:**

- [ ] User can provide a rating (1-10) and a comment.
- [ ] Only "Completed" bookings can be reviewed.
- [ ] System prevents multiple reviews for the same booking.

### US22: View Hotel Reviews

**As a** Guest  
**I want to** read reviews from other guests  
**So that** I can make an informed decision.

**Acceptance Criteria:**

- [ ] Display average rating and total review count.
- [ ] List recent reviews with guest name, rating, and comment.

---

## Group 11: Dashboard & Analytics (Admin)

### US23: Admin Dashboard Overview

**As an** Admin  
**I want to** see a comprehensive dashboard of system metrics and pending tasks  
**So that** I can monitor the platform's performance and handle urgent approvals efficiently.

**Acceptance Criteria:**

- [ ] Display summary cards for Total Revenue, Total Users, Total Hotels, and Total Bookings.
- [ ] Show a list of recent "Pending" hotel and upgrade requests.
- [ ] Display monthly revenue and booking volume charts.

---

## Group 12: Dashboard & Analytics (Hotel Owners)

### US24: Owner Dashboard & Property Insights

**As a** Hotel Owner  
**I want to** see real-time statistics of my property's performance and daily operations  
**So that** I can manage check-ins, check-outs, and optimize my room rates.

**Acceptance Criteria:**

- [ ] Display daily operational counts: Today's Arrivals, Departures, and Stay-overs.
- [ ] Show business metrics: Total Revenue, Occupancy Rate, and Total Bookings.
- [ ] List recent bookings and room availability summary.

---

## Group 13: Booking History & Management

### US25: Guest Booking History

**As a** Guest  
**I want to** view a history of my bookings  
**So that** I can manage my upcoming trips and review past stays.

**Acceptance Criteria:**

- [ ] Display a list of bookings with Hotel Name, Dates, Total Price, and Status.
- [ ] Filter bookings by status: All, Upcoming, Completed, Cancelled.

### US26: Owner Booking Management

**As a** Hotel Owner  
**I want to** see all bookings made for my hotels  
**So that** I can prepare for arrivals and manage room occupancy.

**Acceptance Criteria:**

- [ ] List all bookings for the owner's properties with search and filter capabilities.
- [ ] Update booking status (e.g., mark as "Checked-in").

---

## Group 14: Review Moderation & Engagement

### US27: Owner Response to Reviews

**As a** Hotel Owner  
**I want to** reply to guest reviews  
**So that** I can engage with customers and show that I value their feedback.

**Acceptance Criteria:**

- [ ] Owner can view a list of all reviews for their hotels.
- [ ] Owner can submit a single response to each guest review.

### US28: Admin Review Moderation

**As an** Admin  
**I want to** moderate and hide inappropriate reviews  
**So that** the platform maintains high-quality and respectful feedback.

**Acceptance Criteria:**

- [ ] Admin can mark a review as "Hidden" (soft-delete).
- [ ] Hidden reviews are not visible to guests.

---

## Group 15: Payment & Transactions

### US29: Secure Checkout & Payment

**As a** Guest  
**I want to** pay for my booking using various payment methods  
**So that** I can complete my reservation securely.

**Acceptance Criteria:**

- [ ] System integrates with a payment gateway (e.g., VNPay).
- [ ] Booking status is updated automatically upon successful payment.

### US30: Transaction History

**As a** User  
**I want to** view transaction details  
**So that** I can track my spending.

**Acceptance Criteria:**

- [ ] Display a detailed transaction log.
