# Vietnam Province Hotel Search Design

## Goal

Allow guests to find one of Vietnam's 34 current provinces or municipalities from the Home page, submit the complete hotel search form, and view hotels returned by the existing search API and `sp_SearchHotels` stored procedure on a dedicated results page.

## Scope

- Load provinces from the project's existing location API.
- Provide an accessible autocomplete dropdown in the `Where to?` field.
- Filter suggestions case-insensitively and accent-insensitively.
- Require a valid province selection before submitting the search.
- Navigate to a URL-addressable search results page with all search criteria.
- Fetch and render hotel search results, including loading, error, and empty states.

Hotel details, additional result filters, sorting, pagination, and backend or database changes are outside this change.

## Architecture

The Home page remains the search-form container. It loads provinces through a dedicated location service and passes the data and loading state to the presentational search components. The location field is extracted from `SearchBar` so autocomplete behavior remains isolated and testable.

Submitting the form creates a `/search-results` URL whose query string contains `cityName`, `checkIn`, `checkOut`, `adults`, `children`, and `rooms`. The results page treats this URL as the source of truth, parses and validates the parameters, and calls the existing hotel search endpoint through a dedicated hotel service. This makes search URLs refreshable, bookmarkable, and shareable.

## Components and Responsibilities

### Province data and service

- Add a `Province` frontend type matching the location API response.
- Add a location service that calls `GET /api/v1/locations/countries/{countryId}/provinces`.
- Use the project's Vietnam country ID configured alongside the location request rather than duplicating it across components.
- Report failed requests to the Home page; do not silently replace backend data with a second hard-coded province list.

### Province autocomplete

- Display the selected province name in the existing `Where to?` input.
- Open suggestions on focus and as the guest types.
- Normalize both the query and province names by lowercasing, removing Vietnamese diacritics, and mapping `đ` to `d` before matching.
- Show only matching API results and show a clear empty message when none match.
- Support mouse selection and keyboard navigation with Arrow Up, Arrow Down, Enter, and Escape.
- Close when a province is selected or focus moves outside the control.
- Invalidate the prior selection when the guest edits its text.
- Prevent submission until the text corresponds to a selected API province, and show a concise validation message.

### Search navigation

- Preserve the existing date and guest controls.
- Validate the province and existing search constraints before navigation.
- Encode every query value with `URLSearchParams`.
- Navigate with React Router to `/search-results` instead of calling the hotel API from Home.

### Search results page

- Register `/search-results` in `App.tsx`.
- Read and validate search criteria from the query string.
- Call `GET /api/v1/hotels/search` with the backend contract's exact parameter names.
- Render a loading state while the request is active.
- Render a retryable error state for failed requests.
- Render an empty state when the request succeeds without hotels.
- Render returned hotels as result cards showing the available DTO fields: cover image, name, address/location, rating and review count, starting price, capacity, and available rooms.
- Keep the criteria visible in a compact summary so guests understand which search produced the results.

## Data Flow

1. Home mounts and requests the province list.
2. The guest types; the autocomplete filters the already-loaded list locally.
3. The guest selects a province and completes dates and guest counts.
4. Search validates the form and navigates to the encoded results URL.
5. The results page parses the URL and requests matching hotels.
6. The API executes the existing application/repository flow backed by `sp_SearchHotels`.
7. The results page renders the returned state without retaining hidden navigation-only data.

## Error Handling

- Province loading failure leaves the location control unavailable and displays a retry action.
- Invalid or unselected province text keeps the guest on Home and displays field-level guidance.
- Invalid results-page query parameters display an actionable invalid-search state with a link back to Home.
- Hotel API failures display an error message and retry action without discarding the current URL criteria.
- Missing hotel images use a local visual fallback; broken response items must not crash the entire results list.

## Testing

Implementation follows test-driven development.

- Unit-test Vietnamese normalization and filtering, including `Đà Nẵng` matched by `da nang`.
- Component-test opening, filtering, keyboard navigation, selection, and invalid free text in the autocomplete.
- Test Home search submission produces the expected encoded route and complete query string.
- Test location and hotel services against representative successful and failed API responses.
- Test results-page loading, success, empty, invalid-query, retry, and API-error states.
- Run the complete frontend test suite, lint, and production build before completion.

## Acceptance Criteria

- The `Where to?` control is populated from the backend's current 34-province dataset.
- Suggestions update immediately from the current input and ignore case and Vietnamese accents.
- A guest can select a valid province by mouse or keyboard.
- Search cannot proceed with arbitrary text that is not a selected province.
- Search navigates to a refreshable `/search-results` URL containing all criteria.
- The results page calls the existing hotel search endpoint and presents loading, error, empty, and populated states.
- Existing calendar and guest-room behavior remains intact.
