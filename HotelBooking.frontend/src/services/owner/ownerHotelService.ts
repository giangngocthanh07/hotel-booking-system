import { API_BASE_URL } from "../../config/api";
import { getAuthToken } from "../authService";
import type { OwnerHotelDetails, OwnerHotelsPageData, OwnerHotelInformationUpdate } from "../../types/owner/ownerHotel.types";

export class OwnerHotelApiError extends Error {
  readonly status: number;
  constructor(message: string, status: number) { super(message); this.status = status; }
}

async function read<T>(path: string, signal?: AbortSignal, options: RequestInit = {}): Promise<T> {
  const token = getAuthToken();
  if (!token) throw new OwnerHotelApiError("Please sign in again to view your hotels.", 401);
  const response = await fetch(`${API_BASE_URL}/owner/hotels${path}`, {
    ...options, headers: { Authorization: `Bearer ${token}`, ...(options.body ? { "Content-Type": "application/json" } : {}) }, signal, cache: "no-store",
  });
  const body = await response.json().catch(failure => { if (signal?.aborted) throw failure; return null; });
  if (!response.ok) throw new OwnerHotelApiError(
    [400, 409].includes(response.status) && typeof body?.message === "string" ? body.message : messageFor(response.status), response.status);
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

export async function updateOwnerHotelInformation(hotelId: number, input: OwnerHotelInformationUpdate, signal?: AbortSignal): Promise<boolean> {
  if (!Number.isSafeInteger(hotelId) || hotelId <= 0 || hotelId > 2147483647) throw new OwnerHotelApiError("Invalid Hotel ID.", 400);
  // FR-CAT-002/003: serialize only the editable fields, even if the caller supplies a larger object.
  const { name, description, address, publicPhone, publicEmail, countryId, provinceId, wardId, latitude, longitude } = input;
  const result = await read<boolean>(`/${hotelId}/information`, signal, { method: "PUT",
    body: JSON.stringify({ name, description, address, publicPhone, publicEmail, countryId, provinceId, wardId, latitude, longitude }) });
  if (result !== true) throw new OwnerHotelApiError("Hotel information could not be saved.", 500);
  return true;
}
