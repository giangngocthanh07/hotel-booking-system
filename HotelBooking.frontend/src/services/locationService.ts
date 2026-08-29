import { API_BASE_URL, VIETNAM_COUNTRY_ID } from "../config/api";
import type { ApiResponse } from "../types/auth.types";
import type { Province } from "../types/hotel.types";

const LOAD_PROVINCES_ERROR = "Không thể tải danh sách tỉnh thành.";

export async function getVietnamProvinces(): Promise<Province[]> {
  const response = await fetch(
    `${API_BASE_URL}/locations/countries/${VIETNAM_COUNTRY_ID}/provinces`,
  );
  if (!response.ok) throw new Error(LOAD_PROVINCES_ERROR);

  const data = (await response.json()) as ApiResponse<Province[]>;
  if (data.statusCode !== "Success") {
    throw new Error(data.message || LOAD_PROVINCES_ERROR);
  }

  return data.content;
}
