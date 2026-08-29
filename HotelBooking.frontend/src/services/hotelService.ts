import { API_BASE_URL } from "../config/api";
import type { ApiResponse } from "../types/auth.types";
import type {
  SearchCriteria,
  SearchHotelResult,
} from "../types/hotel.types";

const SEARCH_ERROR = "Không thể tìm kiếm khách sạn.";

function buildSearchQuery(criteria: SearchCriteria): URLSearchParams {
  return new URLSearchParams({
    cityName: criteria.cityName,
    checkIn: criteria.checkIn,
    checkOut: criteria.checkOut,
    adults: String(criteria.adults),
    children: String(criteria.children),
    rooms: String(criteria.rooms),
  });
}

export async function searchHotels(
  criteria: SearchCriteria,
): Promise<SearchHotelResult[]> {
  const query = buildSearchQuery(criteria);
  const response = await fetch(`${API_BASE_URL}/hotels/search?${query}`);
  if (!response.ok) throw new Error(SEARCH_ERROR);

  const data = (await response.json()) as ApiResponse<SearchHotelResult[]>;
  if (data.statusCode !== "Success") {
    throw new Error(data.message || SEARCH_ERROR);
  }

  return data.content;
}
