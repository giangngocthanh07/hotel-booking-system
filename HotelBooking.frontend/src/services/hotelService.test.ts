import { afterEach, describe, expect, it, vi } from "vitest";
import type { SearchCriteria } from "../types/hotel.types";
import { searchHotels } from "./hotelService";

const criteria: SearchCriteria = {
  cityName: "Hà Nội",
  checkIn: "2026-09-01",
  checkOut: "2026-09-03",
  adults: 2,
  children: 1,
  rooms: 1,
};

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

afterEach(() => vi.unstubAllGlobals());

describe("searchHotels", () => {
  it("sends every criterion and returns hotel content", async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({
        statusCode: "Success",
        message: "OK",
        content: [hotel],
      }),
    });
    vi.stubGlobal("fetch", fetchMock);

    await expect(searchHotels(criteria)).resolves.toEqual([hotel]);
    expect(fetchMock).toHaveBeenCalledWith(
      "http://localhost:5083/api/v1/hotels/search?cityName=H%C3%A0+N%E1%BB%99i&checkIn=2026-09-01&checkOut=2026-09-03&adults=2&children=1&rooms=1",
    );
  });

  it("rejects an unsuccessful API response with its message", async () => {
    vi.stubGlobal(
      "fetch",
      vi.fn().mockResolvedValue({
        ok: true,
        json: async () => ({
          statusCode: "BadRequest",
          message: "Ngày không hợp lệ",
          content: [],
        }),
      }),
    );

    await expect(searchHotels(criteria)).rejects.toThrow("Ngày không hợp lệ");
  });

  it("rejects an unsuccessful HTTP response", async () => {
    vi.stubGlobal("fetch", vi.fn().mockResolvedValue({ ok: false }));

    await expect(searchHotels(criteria)).rejects.toThrow(
      "Không thể tìm kiếm khách sạn.",
    );
  });
});
