import { API_BASE_URL } from "../../config/api";
import { getAuthToken } from "../authService";
import { OwnerHotelApiError } from "./ownerHotelService";
import type { HotelImageKind, OwnerHotelMedia } from "../../types/owner/ownerHotelMedia.types";

async function request<T>(hotelId: number, suffix: string, signal?: AbortSignal, body?: FormData, method = body ? "POST" : "GET"): Promise<T> {
  if (!Number.isSafeInteger(hotelId) || hotelId <= 0 || hotelId > 2147483647) throw new OwnerHotelApiError("Invalid Hotel ID.", 400);
  const token = getAuthToken();
  if (!token) throw new OwnerHotelApiError("Please sign in again.", 401);
  // FR-CAT-003 / FR-MED-001: identity comes from authentication; the browser supplies multipart boundaries.
  const response = await fetch(`${API_BASE_URL}/owner/hotels/${hotelId}/images${suffix}`, {
    method, headers: { Authorization: `Bearer ${token}` }, body, signal, cache: "no-store",
  });
  const result = await response.json().catch(error => { if (signal?.aborted) throw error; return null; });
  if (!response.ok) {
    const message = [400, 409].includes(response.status) && typeof result?.message === "string" ? result.message
      : response.status === 401 ? "Please sign in again." : response.status === 403 ? "This account cannot manage Owner images."
      : response.status === 404 ? "Hotel could not be found or is unavailable." : "Unable to load or save hotel images. Please try again.";
    throw new OwnerHotelApiError(message, response.status);
  }
  if (result?.statusCode !== "Success" || result.content == null) throw new OwnerHotelApiError("Unable to load or save hotel images. Please try again.", 500);
  return result.content as T;
}

export const getOwnerHotelMedia = (hotelId: number, signal?: AbortSignal) => request<OwnerHotelMedia>(hotelId, "", signal);

export function uploadOwnerHotelImage(hotelId: number, kind: HotelImageKind, file: File, signal?: AbortSignal) {
  const body = new FormData(); body.append("file", file);
  return request<{ hotelId: number; imageUrl: string }>(hotelId, `/${kind}`, signal, body);
}

export async function deleteOwnerHotelImage(hotelId: number, imageId: number, signal?: AbortSignal) {
  if (!Number.isSafeInteger(imageId) || imageId <= 0 || imageId > 2147483647) throw new OwnerHotelApiError("Invalid image ID.", 400);
  const result = await request<boolean>(hotelId, `/gallery/${imageId}`, signal, undefined, "DELETE");
  if (result !== true) throw new OwnerHotelApiError("Image could not be removed. Please try again.", 500);
  return true;
}
