import { useRef, useState } from "react";
import { cancelRegistration } from "../../services/ownerRegistrationService";
import type { HotelRegistrationHistoryItem } from "../../services/ownerRegistrationService";

export default function useRegistrationCancellation(refreshHistory: () => Promise<void>) {
  const busy = useRef(false);
  const [cancellingId, setCancellingId] = useState<number | null>(null);
  const [cancelError, setCancelError] = useState("");
  const [cancelSuccess, setCancelSuccess] = useState("");

  async function cancel(request: HotelRegistrationHistoryItem) {
    // FR-ONB-018: confirm intent and guard repeat clicks before React updates the buttons.
    if (busy.current || request.status !== "Pending" || !window.confirm(`Cancel registration for "${request.name}"?`)) return;
    busy.current = true;
    setCancellingId(request.requestId);
    setCancelError("");
    setCancelSuccess("");
    try {
      const response = await cancelRegistration(request.requestId);
      if (response.statusCode !== "Success") throw new Error(response.message || "Unable to cancel request.");
      setCancelSuccess(`Registration for "${request.name}" was cancelled.`);
      await refreshHistory();
    } catch (failure) {
      setCancelError(cancellationError(failure));
    } finally {
      busy.current = false;
      setCancellingId(null);
    }
  }

  return { cancel, cancellingId, cancelError, cancelSuccess };
}

function cancellationError(failure: unknown): string {
  // Preserve structured API errors as well as network failures without hiding history.
  if (failure && typeof failure === "object" && "response" in failure) {
    const response = failure.response as { data?: { message?: unknown } } | null;
    if (typeof response?.data?.message === "string" && response.data.message) return response.data.message;
  }
  return failure instanceof Error ? failure.message : "Unable to cancel request. Please try again.";
}
