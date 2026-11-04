import type { HotelApprovalRequest } from "../../types/admin.types";

// FR-ONB-014: display the recorded decision without treating Pending/Cancelled updates as Admin decisions.
export default function HotelDecisionDetails({ request }: { request: HotelApprovalRequest }) {
  if (request.status !== "Approved" && request.status !== "Rejected") return null;
  const processor = request.processedByName?.trim()
    || (request.processedBy ? `Admin #${request.processedBy}` : "Not recorded");
  const decidedAt = request.processedAt ? new Date(request.processedAt) : null;
  const validDate = decidedAt && !Number.isNaN(decidedAt.getTime());

  return (
    <section className="req-info-card req-decision-details" aria-labelledby="hotel-decision-heading">
      <h3 id="hotel-decision-heading" className="req-info-card-title">Decision details</h3>
      <dl>
        <dt>Processed by</dt>
        <dd>{processor}</dd>
        <dt>Processed at</dt>
        <dd>{validDate ? <time dateTime={request.processedAt}>{decidedAt.toLocaleString()}</time> : "Not recorded"}</dd>
        {request.status === "Rejected" && <>
          <dt>Rejection reason</dt>
          {/* Legacy records may predate mandatory reasons; show absence rather than invent an explanation. */}
          <dd className="req-decision-reason">{request.adminRemark?.trim() || "No rejection reason recorded."}</dd>
        </>}
      </dl>
    </section>
  );
}
