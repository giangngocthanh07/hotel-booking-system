import { API_BASE_URL } from "../../config/api";
import { getAuthToken } from "../authService";
import type { OwnerHotelDetails, OwnerHotelsPageData } from "../../types/owner/ownerHotel.types";

export class OwnerHotelApiError extends Error {
  readonly status: number;
  constructor(message: string, status: number) { super(message); this.status = status; }
}

async function read<T>(path: string, signal?: AbortSignal): Promise<T> {
  const token = getAuthToken();
  if (!token) throw new OwnerHotelApiError("Please sign in again to view your hotels.", 401);
  const response = await fetch(`${API_BASE_URL}/owner/hotels${path}`, {
    headers: { Authorization: `Bearer ${token}` }, signal, cache: "no-store",
  });
  const body = await response.json().catch(() => null);
  if (!response.ok) throw new OwnerHotelApiError(messageFor(response.status), response.status);
  if (body?.statusCode !== "Success" || body.content == null)
    throw new OwnerHotelApiError("Unable to load hotel information. Please try again.", 500);
  return body.content as T;
}

function messageFor(status: number): string {
  if (status === 401) return "Please sign in again to view your hotels.";
  if (status === 403) return "This account cannot access Owner hotels.";
  if (status === 404) return "Hotel could not be found or is unavailable.";
  return "Unable to load hotel information. Please try again.";
}

// FR-CAT-001/003/008: use management endpoints; ownership comes from the bearer identity, not a caller ID.
export const getOwnerHotels = (pageIndex = 1, pageSize = 6, signal?: AbortSignal) =>
  read<OwnerHotelsPageData>(`?pageIndex=${pageIndex}&pageSize=${pageSize}`, signal);

export function getOwnerHotel(hotelId: number, signal?: AbortSignal): Promise<OwnerHotelDetails> {
  if (!Number.isSafeInteger(hotelId) || hotelId <= 0 || hotelId > 2147483647)
    return Promise.reject(new OwnerHotelApiError("Invalid Hotel ID.", 400));
  return read<OwnerHotelDetails>(`/${hotelId}`, signal);
}
