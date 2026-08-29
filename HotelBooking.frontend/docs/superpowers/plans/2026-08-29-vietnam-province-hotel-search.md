# Vietnam Province Hotel Search Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a province autocomplete backed by the project's location API and a URL-addressable hotel results page backed by the existing hotel search API.

**Architecture:** `HomePage` owns the search form and province-loading state, while a focused `ProvinceAutocomplete` owns suggestion interaction. Search criteria are serialized into the `/search-results` query string; `SearchResultsPage` parses that URL and calls a dedicated hotel service, making results refreshable and shareable.

**Tech Stack:** React 19, TypeScript 6, React Router 7, native Fetch API, Vitest, Testing Library, CSS.

---

## File Structure

- Create `src/config/api.ts`: shared API base URL and Vietnam country ID.
- Modify `src/types/auth.types.ts`: keep the existing shared API response contract available to hotel/location services.
- Modify `src/types/hotel.types.ts`: add `Province` and parsed search criteria types.
- Create `src/utils/provinceSearch.ts`: pure Vietnamese normalization and province filtering.
- Create `src/services/locationService.ts`: fetch provinces from the backend.
- Create `src/services/hotelService.ts`: build hotel query parameters and fetch search results.
- Create `src/components/home/ProvinceAutocomplete.tsx`: accessible location combobox.
- Modify `src/components/home/SearchBar.tsx`: render the combobox and report province validation.
- Modify `src/components/home/SearchBar.css`: style the combobox dropdown and validation state.
- Modify `src/pages/HomePage.tsx`: load provinces and navigate with search query parameters.
- Create `src/pages/SearchResultsPage.tsx`: parse URL state and render hotel search states.
- Create `src/pages/SearchResultsPage.css`: results layout and responsive hotel cards.
- Modify `src/App.tsx`: register the search results route.
- Create `src/test/setup.ts` and focused `*.test.ts(x)` files beside tested modules.

### Task 1: Install and configure the frontend test harness

**Files:**
- Modify: `package.json`
- Modify: `vite.config.ts`
- Modify: `tsconfig.app.json`
- Create: `src/test/setup.ts`

- [ ] **Step 1: Install test dependencies**

Run:

```bash
npm install --save-dev vitest jsdom @testing-library/react @testing-library/jest-dom @testing-library/user-event
```

Expected: dependencies are added to `package.json` and `package-lock.json` without peer dependency errors.

- [ ] **Step 2: Add test scripts and Vitest configuration**

Add these scripts to `package.json`:

```json
"test": "vitest run",
"test:watch": "vitest"
```

Configure `vite.config.ts` with React and this test block:

```ts
/// <reference types="vitest/config" />
import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

export default defineConfig({
  plugins: [react()],
  test: {
    environment: "jsdom",
    setupFiles: "./src/test/setup.ts",
    restoreMocks: true,
  },
});
```

Add `"vitest/globals"` and `"@testing-library/jest-dom"` to `compilerOptions.types` in `tsconfig.app.json`, then create:

```ts
// src/test/setup.ts
import "@testing-library/jest-dom/vitest";
```

- [ ] **Step 3: Run the empty suite**

Run: `npm test`

Expected: PASS with a message that no test files exist (use `--passWithNoTests` temporarily only if the installed Vitest version exits non-zero).

- [ ] **Step 4: Commit the harness**

```bash
git add HotelBooking.frontend/package.json HotelBooking.frontend/package-lock.json HotelBooking.frontend/vite.config.ts HotelBooking.frontend/tsconfig.app.json HotelBooking.frontend/src/test/setup.ts
git commit -m "test(frontend): configure vitest and testing library"
```

### Task 2: Add province filtering and location API service

**Files:**
- Create: `src/config/api.ts`
- Modify: `src/types/hotel.types.ts`
- Create: `src/utils/provinceSearch.ts`
- Create: `src/utils/provinceSearch.test.ts`
- Create: `src/services/locationService.ts`
- Create: `src/services/locationService.test.ts`

- [ ] **Step 1: Write failing province normalization tests**

```ts
// src/utils/provinceSearch.test.ts
import { describe, expect, it } from "vitest";
import type { Province } from "../types/hotel.types";
import { filterProvinces, normalizeVietnamese } from "./provinceSearch";

const provinces: Province[] = [
  { id: 1, name: "Đà Nẵng" },
  { id: 2, name: "Thành phố Hồ Chí Minh" },
  { id: 3, name: "Hà Nội" },
];

describe("province search", () => {
  it("normalizes Vietnamese accents and đ", () => {
    expect(normalizeVietnamese("Đà Nẵng")).toBe("da nang");
  });

  it("filters province names without requiring accents or matching case", () => {
    expect(filterProvinces(provinces, "DA nang")).toEqual([provinces[0]]);
  });

  it("returns all provinces for a blank query", () => {
    expect(filterProvinces(provinces, "  ")).toEqual(provinces);
  });
});
```

- [ ] **Step 2: Run the utility test and verify RED**

Run: `npm test -- src/utils/provinceSearch.test.ts`

Expected: FAIL because `provinceSearch.ts` and its exports do not exist.

- [ ] **Step 3: Add types, config, and minimal filter implementation**

Append to `src/types/hotel.types.ts`:

```ts
export interface Province {
  id: number;
  name: string;
}

export type SearchCriteria = HotelSearchRequest;
```

Create:

```ts
// src/config/api.ts
export const API_BASE_URL = "http://localhost:5083/api/v1";
export const VIETNAM_COUNTRY_ID = 4;
```

```ts
// src/utils/provinceSearch.ts
import type { Province } from "../types/hotel.types";

export function normalizeVietnamese(value: string): string {
  return value
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .replace(/đ/g, "d")
    .replace(/Đ/g, "D")
    .toLowerCase()
    .trim();
}

export function filterProvinces(
  provinces: Province[],
  query: string,
): Province[] {
  const normalizedQuery = normalizeVietnamese(query);
  if (!normalizedQuery) return provinces;
  return provinces.filter((province) =>
    normalizeVietnamese(province.name).includes(normalizedQuery),
  );
}
```

- [ ] **Step 4: Run the utility test and verify GREEN**

Run: `npm test -- src/utils/provinceSearch.test.ts`

Expected: 3 tests PASS.

- [ ] **Step 5: Write the failing location service tests**

```ts
// src/services/locationService.test.ts
import { afterEach, describe, expect, it, vi } from "vitest";
import { getVietnamProvinces } from "./locationService";

afterEach(() => vi.unstubAllGlobals());

describe("getVietnamProvinces", () => {
  it("returns province content from the location API", async () => {
    vi.stubGlobal("fetch", vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({
        statusCode: "Success",
        message: "OK",
        content: [{ id: 1, name: "Hà Nội" }],
      }),
    }));

    await expect(getVietnamProvinces()).resolves.toEqual([
      { id: 1, name: "Hà Nội" },
    ]);
  });

  it("rejects an unsuccessful API response", async () => {
    vi.stubGlobal("fetch", vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({ statusCode: "BadRequest", message: "Failed", content: [] }),
    }));

    await expect(getVietnamProvinces()).rejects.toThrow("Failed");
  });
});
```

- [ ] **Step 6: Run the service test and verify RED**

Run: `npm test -- src/services/locationService.test.ts`

Expected: FAIL because `locationService.ts` does not exist.

- [ ] **Step 7: Implement the location service**

```ts
// src/services/locationService.ts
import { API_BASE_URL, VIETNAM_COUNTRY_ID } from "../config/api";
import type { ApiResponse } from "../types/auth.types";
import type { Province } from "../types/hotel.types";

export async function getVietnamProvinces(): Promise<Province[]> {
  const response = await fetch(
    `${API_BASE_URL}/locations/countries/${VIETNAM_COUNTRY_ID}/provinces`,
  );
  if (!response.ok) throw new Error("Không thể tải danh sách tỉnh thành.");

  const data = (await response.json()) as ApiResponse<Province[]>;
  if (data.statusCode !== "Success") {
    throw new Error(data.message || "Không thể tải danh sách tỉnh thành.");
  }
  return data.content;
}
```

- [ ] **Step 8: Run both Task 2 suites and commit**

Run: `npm test -- src/utils/provinceSearch.test.ts src/services/locationService.test.ts`

Expected: 5 tests PASS.

```bash
git add HotelBooking.frontend/src/config/api.ts HotelBooking.frontend/src/types/hotel.types.ts HotelBooking.frontend/src/utils HotelBooking.frontend/src/services/locationService.ts HotelBooking.frontend/src/services/locationService.test.ts
git commit -m "feat(frontend): load and filter Vietnam provinces"
```

### Task 3: Build the accessible province autocomplete

**Files:**
- Create: `src/components/home/ProvinceAutocomplete.tsx`
- Create: `src/components/home/ProvinceAutocomplete.test.tsx`
- Modify: `src/components/home/SearchBar.css`

- [ ] **Step 1: Write failing interaction tests**

Create a test using `userEvent` and these core assertions:

```tsx
// src/components/home/ProvinceAutocomplete.test.tsx
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";
import ProvinceAutocomplete from "./ProvinceAutocomplete";

const provinces = [
  { id: 1, name: "Đà Nẵng" },
  { id: 2, name: "Hà Nội" },
];

describe("ProvinceAutocomplete", () => {
  it("filters without accents and selects a suggestion", async () => {
    const user = userEvent.setup();
    const onInputChange = vi.fn();
    const onSelect = vi.fn();
    const view = render(
      <ProvinceAutocomplete value="" provinces={provinces} selectedProvince={null}
        onInputChange={onInputChange} onSelect={onSelect} isLoading={false} />,
    );
    await user.click(screen.getByRole("combobox"));
    await user.type(screen.getByRole("combobox"), "da nang");
    view.rerender(
      <ProvinceAutocomplete value="da nang" provinces={provinces} selectedProvince={null}
        onInputChange={onInputChange} onSelect={onSelect} isLoading={false} />,
    );
    await user.click(screen.getByRole("option", { name: "Đà Nẵng" }));
    expect(onSelect).toHaveBeenCalledWith(provinces[0]);
  });

  it("selects the highlighted suggestion with the keyboard", async () => {
    const user = userEvent.setup();
    const onSelect = vi.fn();
    render(
      <ProvinceAutocomplete value="" provinces={provinces} selectedProvince={null}
        onInputChange={vi.fn()} onSelect={onSelect} isLoading={false} />,
    );
    await user.click(screen.getByRole("combobox"));
    await user.keyboard("{ArrowDown}{Enter}");
    expect(onSelect).toHaveBeenCalledWith(provinces[0]);
  });
});
```

- [ ] **Step 2: Run the component test and verify RED**

Run: `npm test -- src/components/home/ProvinceAutocomplete.test.tsx`

Expected: FAIL because the component does not exist.

- [ ] **Step 3: Implement the minimal combobox**

Implement `ProvinceAutocomplete` with this public contract:

```ts
interface ProvinceAutocompleteProps {
  value: string;
  provinces: Province[];
  selectedProvince: Province | null;
  onInputChange: (value: string) => void;
  onSelect: (province: Province) => void;
  isLoading: boolean;
  error?: string;
  onRetry?: () => void;
}
```

Use `role="combobox"`, `aria-expanded`, `aria-controls`, `role="listbox"`, and `role="option"`. Store only `isOpen` and `activeIndex` locally, derive options with `filterProvinces`, select on Enter/click, close on Escape, and use a wrapper `onBlur` check against `event.currentTarget.contains(event.relatedTarget)`.

- [ ] **Step 4: Add dropdown styles**

Add `.province-autocomplete`, `.province-options`, `.province-option`, `.province-option--active`, `.province-empty`, `.province-error`, and `.province-retry` rules to `SearchBar.css`. Position `.province-options` absolutely below the location field with `z-index: 120`, a white background, rounded corners, and the existing search panel shadow language.

- [ ] **Step 5: Run the component tests and commit**

Run: `npm test -- src/components/home/ProvinceAutocomplete.test.tsx`

Expected: 2 tests PASS with no accessibility query failures.

```bash
git add HotelBooking.frontend/src/components/home/ProvinceAutocomplete.tsx HotelBooking.frontend/src/components/home/ProvinceAutocomplete.test.tsx HotelBooking.frontend/src/components/home/SearchBar.css
git commit -m "feat(frontend): add province autocomplete"
```

### Task 4: Integrate province loading and URL navigation on Home

**Files:**
- Modify: `src/components/home/SearchBar.tsx`
- Modify: `src/pages/HomePage.tsx`
- Create: `src/pages/HomePage.test.tsx`

- [ ] **Step 1: Write a failing Home navigation test**

Mock `getVietnamProvinces`, render `HomePage` under `MemoryRouter`, select `Đà Nẵng`, click Search, and assert the router location contains every criterion:

```tsx
expect(screen.getByTestId("location")).toHaveTextContent(
  "/search-results?cityName=%C4%90%C3%A0+N%E1%BA%B5ng&checkIn=",
);
expect(screen.getByTestId("location")).toHaveTextContent("&checkOut=");
expect(screen.getByTestId("location")).toHaveTextContent("&adults=1&children=0&rooms=1");
```

Also add a test that enters unmatched free text, clicks Search, remains on `/`, and sees `Vui lòng chọn một tỉnh/thành trong danh sách.`

- [ ] **Step 2: Run the Home test and verify RED**

Run: `npm test -- src/pages/HomePage.test.tsx`

Expected: FAIL because Home does not load provinces or navigate.

- [ ] **Step 3: Extend SearchBar's presentation contract**

Add these props:

```ts
provinces: Province[];
selectedProvince: Province | null;
onProvinceSelect: (province: Province) => void;
provinceLoading: boolean;
provinceLoadError?: string;
onProvinceRetry: () => void;
cityValidationError?: string;
```

Replace the raw location `<input>` with `ProvinceAutocomplete`, keep `onCityChange`, and render the field validation message directly beneath the location field. Editing text must call `onCityChange` so Home can clear the selected province.

- [ ] **Step 4: Implement Home loading, validation, and navigation**

In `HomePage`, add `useEffect`, `useCallback`, and `useNavigate`. Load provinces with `getVietnamProvinces`; track `provinces`, `selectedProvince`, `provinceLoading`, `provinceLoadError`, and `cityValidationError`.

Use this submission logic:

```ts
function handleSearch() {
  if (!selectedProvince || selectedProvince.name !== searchForm.cityName) {
    setCityValidationError("Vui lòng chọn một tỉnh/thành trong danh sách.");
    return;
  }

  const query = new URLSearchParams({
    cityName: selectedProvince.name,
    checkIn: searchForm.checkIn,
    checkOut: searchForm.checkOut,
    adults: String(searchForm.adults),
    children: String(searchForm.children),
    rooms: String(searchForm.rooms),
  });
  navigate(`/search-results?${query.toString()}`);
}
```

When input text changes, clear `selectedProvince` and the stale validation message. When a province is selected, update both `selectedProvince` and `searchForm.cityName`.

- [ ] **Step 5: Run Home and regression tests, then commit**

Run: `npm test -- src/pages/HomePage.test.tsx src/components/home/ProvinceAutocomplete.test.tsx`

Expected: all tests PASS.

```bash
git add HotelBooking.frontend/src/components/home/SearchBar.tsx HotelBooking.frontend/src/pages/HomePage.tsx HotelBooking.frontend/src/pages/HomePage.test.tsx
git commit -m "feat(frontend): navigate from province search"
```

### Task 5: Add hotel search URL parsing and API service

**Files:**
- Create: `src/utils/searchCriteria.ts`
- Create: `src/utils/searchCriteria.test.ts`
- Create: `src/services/hotelService.ts`
- Create: `src/services/hotelService.test.ts`

- [ ] **Step 1: Write failing criteria parsing tests**

Test a valid query returns all typed values, and test missing city, invalid dates, checkout not after check-in, adults below 1, children below 0, or rooms below 1 returns `null`.

```ts
expect(parseSearchCriteria(new URLSearchParams(
  "cityName=H%C3%A0+N%E1%BB%99i&checkIn=2026-09-01&checkOut=2026-09-03&adults=2&children=1&rooms=1",
))).toEqual({ cityName: "Hà Nội", checkIn: "2026-09-01", checkOut: "2026-09-03", adults: 2, children: 1, rooms: 1 });
```

- [ ] **Step 2: Run criteria tests and verify RED**

Run: `npm test -- src/utils/searchCriteria.test.ts`

Expected: FAIL because `parseSearchCriteria` does not exist.

- [ ] **Step 3: Implement strict criteria parsing**

Create `parseSearchCriteria(params): SearchCriteria | null`. Validate ISO dates with `/^\d{4}-\d{2}-\d{2}$/`, verify both dates parse, require checkout after check-in, and use a small `parseInteger(value, minimum)` helper for counts. Keep each helper under 20 functional lines.

- [ ] **Step 4: Run criteria tests and verify GREEN**

Run: `npm test -- src/utils/searchCriteria.test.ts`

Expected: all valid and invalid cases PASS.

- [ ] **Step 5: Write failing hotel service tests**

Stub `fetch`, call `searchHotels(criteria)`, assert the requested URL ends in:

```text
/hotels/search?cityName=H%C3%A0+N%E1%BB%99i&checkIn=2026-09-01&checkOut=2026-09-03&adults=2&children=1&rooms=1
```

Assert a successful `ApiResponse<SearchHotelResult[]>` returns `content`, and unsuccessful HTTP/API responses reject with the backend message or `Không thể tìm kiếm khách sạn.`.

- [ ] **Step 6: Run hotel service tests and verify RED**

Run: `npm test -- src/services/hotelService.test.ts`

Expected: FAIL because `hotelService.ts` does not exist.

- [ ] **Step 7: Implement hotel search service**

```ts
export async function searchHotels(
  criteria: SearchCriteria,
): Promise<SearchHotelResult[]> {
  const query = new URLSearchParams({
    cityName: criteria.cityName,
    checkIn: criteria.checkIn,
    checkOut: criteria.checkOut,
    adults: String(criteria.adults),
    children: String(criteria.children),
    rooms: String(criteria.rooms),
  });
  const response = await fetch(`${API_BASE_URL}/hotels/search?${query}`);
  if (!response.ok) throw new Error("Không thể tìm kiếm khách sạn.");
  const data = (await response.json()) as ApiResponse<SearchHotelResult[]>;
  if (data.statusCode !== "Success") {
    throw new Error(data.message || "Không thể tìm kiếm khách sạn.");
  }
  return data.content;
}
```

- [ ] **Step 8: Run Task 5 suites and commit**

Run: `npm test -- src/utils/searchCriteria.test.ts src/services/hotelService.test.ts`

Expected: all tests PASS.

```bash
git add HotelBooking.frontend/src/utils/searchCriteria.ts HotelBooking.frontend/src/utils/searchCriteria.test.ts HotelBooking.frontend/src/services/hotelService.ts HotelBooking.frontend/src/services/hotelService.test.ts
git commit -m "feat(frontend): add hotel search client"
```

### Task 6: Build and route the hotel results page

**Files:**
- Create: `src/pages/SearchResultsPage.tsx`
- Create: `src/pages/SearchResultsPage.css`
- Create: `src/pages/SearchResultsPage.test.tsx`
- Modify: `src/App.tsx`

- [ ] **Step 1: Write failing results-page state tests**

Mock `searchHotels` and cover these independent behaviors:

```tsx
// src/pages/SearchResultsPage.test.tsx
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { searchHotels } from "../services/hotelService";
import SearchResultsPage from "./SearchResultsPage";

vi.mock("../services/hotelService", () => ({ searchHotels: vi.fn() }));
const mockedSearchHotels = vi.mocked(searchHotels);
const validUrl = "/search-results?cityName=H%C3%A0+N%E1%BB%99i&checkIn=2026-09-01&checkOut=2026-09-03&adults=2&children=1&rooms=1";
const hotel = {
  id: 1,
  name: "Lake Hotel",
  address: "1 Hồ Gươm",
  description: "Central hotel",
  cityName: "Hà Nội",
  countryName: "Việt Nam",
  coverImageUrl: "",
  priceFrom: 1200000,
  maxAdultCapacity: 2,
  maxChildCapacity: 1,
  avgRating: 4.7,
  reviewCount: 31,
  availableRooms: 3,
};

function renderPage(url = validUrl) {
  return render(
    <MemoryRouter initialEntries={[url]}>
      <SearchResultsPage />
    </MemoryRouter>,
  );
}

describe("SearchResultsPage", () => {
  beforeEach(() => mockedSearchHotels.mockReset());

  it("shows loading then returned hotels and criteria", async () => {
    mockedSearchHotels.mockResolvedValue([hotel]);
    renderPage();
    expect(screen.getByRole("status")).toHaveTextContent("Đang tìm");
    expect(await screen.findByText("Lake Hotel")).toBeInTheDocument();
    expect(screen.getByText(/Hà Nội/)).toBeInTheDocument();
    expect(screen.getByText(/2 người lớn/)).toBeInTheDocument();
  });

  it("shows an empty state for no matches", async () => {
    mockedSearchHotels.mockResolvedValue([]);
    renderPage();
    expect(await screen.findByText("Không tìm thấy khách sạn phù hợp.")).toBeInTheDocument();
  });

  it("shows a retry action after an API error", async () => {
    const user = userEvent.setup();
    mockedSearchHotels.mockRejectedValueOnce(new Error("Mất kết nối"));
    mockedSearchHotels.mockResolvedValueOnce([hotel]);
    renderPage();
    await user.click(await screen.findByRole("button", { name: "Thử lại" }));
    expect(await screen.findByText("Lake Hotel")).toBeInTheDocument();
    expect(mockedSearchHotels).toHaveBeenCalledTimes(2);
  });

  it("does not call the API for an invalid query", async () => {
    renderPage("/search-results?checkIn=2026-09-01");
    expect(screen.getByText("Điều kiện tìm kiếm không hợp lệ.")).toBeInTheDocument();
    expect(mockedSearchHotels).not.toHaveBeenCalled();
  });
});
```

Use a complete fixed query string and assert the criteria summary contains the province, date range, travellers, and rooms.

- [ ] **Step 2: Run results-page tests and verify RED**

Run: `npm test -- src/pages/SearchResultsPage.test.tsx`

Expected: FAIL because `SearchResultsPage.tsx` does not exist.

- [ ] **Step 3: Implement results states and hotel cards**

Use `useSearchParams` and `parseSearchCriteria`. Keep `hotels`, `isLoading`, and `error` in local state. Wrap `searchHotels(criteria)` in a `useCallback`, call it from `useEffect`, and guard stale async completions with an `ignore` boolean in the effect cleanup.

Render:

- invalid query: heading, explanation, and `<Link to="/">Quay lại tìm kiếm</Link>`;
- loading: `role="status"` with `Đang tìm khách sạn phù hợp...`;
- error: `role="alert"`, backend-safe message, and retry button;
- empty: `Không tìm thấy khách sạn phù hợp.`;
- success: semantic list of cards using every reliable `SearchHotelResult` field and a local `/hotel-placeholder.svg` fallback.

- [ ] **Step 4: Add responsive results styles and fallback asset**

Create `public/hotel-placeholder.svg` with a neutral hotel/building illustration. Style the page with a max-width container, criteria summary panel, grid/list cards, 240px cover image column on desktop, stacked cards below 720px, and visible focus styles.

- [ ] **Step 5: Register the route**

Add:

```tsx
import SearchResultsPage from "./pages/SearchResultsPage";

<Route path="/search-results" element={<SearchResultsPage />} />
```

before the wildcard route in `App.tsx`.

- [ ] **Step 6: Run page tests and commit**

Run: `npm test -- src/pages/SearchResultsPage.test.tsx`

Expected: loading, success, empty, error/retry, and invalid-query tests PASS.

```bash
git add HotelBooking.frontend/src/pages/SearchResultsPage.tsx HotelBooking.frontend/src/pages/SearchResultsPage.css HotelBooking.frontend/src/pages/SearchResultsPage.test.tsx HotelBooking.frontend/public/hotel-placeholder.svg HotelBooking.frontend/src/App.tsx
git commit -m "feat(frontend): add hotel search results page"
```

### Task 7: Verify the complete frontend

**Files:**
- Modify only files implicated by verification failures.

- [ ] **Step 1: Run the full automated suite**

Run: `npm test`

Expected: every Vitest suite PASS with no unhandled rejection or React act warning.

- [ ] **Step 2: Run static checks**

Run: `npm run lint`

Expected: exit code 0 with no ESLint errors.

- [ ] **Step 3: Run the production build**

Run: `npm run build`

Expected: TypeScript and Vite build successfully and produce `dist/`.

- [ ] **Step 4: Perform a browser smoke test with the backend running**

Verify: Home loads 34 API provinces; `da nang` finds `Đà Nẵng`; mouse and keyboard selection work; Search creates the complete `/search-results` URL; refresh repeats the API search; populated, empty, and failure states remain readable on desktop and mobile widths.

- [ ] **Step 5: Commit verification-only fixes if any**

```bash
git add HotelBooking.frontend
git commit -m "fix(frontend): resolve hotel search verification issues"
```

Skip this commit when verification required no code changes. Do not stage unrelated user changes.
