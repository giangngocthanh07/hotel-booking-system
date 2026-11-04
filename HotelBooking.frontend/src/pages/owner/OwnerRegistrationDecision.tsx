import type { HotelRegistrationHistoryItem } from "../../services/ownerRegistrationService";

// FR-ONB-013: let the Owner understand a saved decision and what needs correcting after rejection.
export default function OwnerRegistrationDecision({ request }: { request: HotelRegistrationHistoryItem }) {
  if (request.status !== "Approved" && request.status !== "Rejected") return null;
  const decidedAt = request.processedAt ? new Date(request.processedAt) : null;
  const validDate = decidedAt && !Number.isNaN(decidedAt.getTime());

  return (
    <div className="hi-decision" role="group" aria-label="Registration decision">
      <p><strong>Decision time: </strong>
        {validDate ? <time dateTime={request.processedAt ?? undefined}>{decidedAt.toLocaleString()}</time> : <span>Not recorded</span>}
      </p>
      {request.status === "Rejected" && <>
        <p><strong>Rejection reason:</strong></p>
        {/* Legacy requests may have been rejected before explanations became mandatory. */}
        <p className="hi-rejection-reason">{request.adminRemark?.trim() || "No rejection reason recorded."}</p>
      </>}
    </div>
  );
}
