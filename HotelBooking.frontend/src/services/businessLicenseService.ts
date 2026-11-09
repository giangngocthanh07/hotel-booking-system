import { API_BASE_URL } from "../config/api";
import { getAuthToken } from "./authService";

// FR-ONB-013/014: request content through the authorized API; never use a provider URL or token in a URL.
export async function downloadBusinessLicense(requestId: number, signal?: AbortSignal): Promise<Blob> {
  if (!Number.isSafeInteger(requestId) || requestId <= 0) throw new Error("Invalid document request.");
  const token = getAuthToken();
  if (!token) throw new Error("Please sign in again to view this document.");
  const response = await fetch(`${API_BASE_URL}/hotel-registrations/${requestId}/business-license`, {
    headers: { Authorization: `Bearer ${token}` }, signal, cache: "no-store", redirect: "error",
  });
  if (!response.ok) throw new Error(failureMessage(response.status));
  const file = await response.blob();
  const mime = file.type.split(";")[0].toLowerCase();
  if (!["application/pdf", "image/jpeg", "image/png"].includes(mime) || file.size === 0 || file.size > 10 * 1024 * 1024)
    throw new Error("Invalid business license response.");
  return file;
}

function failureMessage(status: number): string {
  if (status === 401) return "Please sign in again to view this document.";
  if (status === 403) return "You do not have permission to view this document.";
  if (status === 404) return "Business license is unavailable.";
  return "Unable to load the business license. Please try again.";
}
