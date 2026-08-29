import { render, screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { searchHotels } from "../services/hotelService";
import SearchResultsPage from "./SearchResultsPage";

vi.mock("../services/hotelService", () => ({ searchHotels: vi.fn() }));

const mockedSearchHotels = vi.mocked(searchHotels);
const validUrl =
  "/search-results?cityName=H%C3%A0+N%E1%BB%99i&checkIn=2026-09-01&checkOut=2026-09-03&adults=2&children=1&rooms=1";
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
    const summary = screen.getByLabelText("Điều kiện tìm kiếm");
    expect(within(summary).getByText(/2 người lớn/)).toBeInTheDocument();
    expect(within(summary).getByText(/1 trẻ em/)).toBeInTheDocument();
  });

  it("shows an empty state for no matches", async () => {
    mockedSearchHotels.mockResolvedValue([]);
    renderPage();

    expect(
      await screen.findByText("Không tìm thấy khách sạn phù hợp."),
    ).toBeInTheDocument();
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

  it("does not call the API for an invalid query", () => {
    renderPage("/search-results?checkIn=2026-09-01");

    expect(
      screen.getByText("Điều kiện tìm kiếm không hợp lệ."),
    ).toBeInTheDocument();
    expect(mockedSearchHotels).not.toHaveBeenCalled();
  });
});
