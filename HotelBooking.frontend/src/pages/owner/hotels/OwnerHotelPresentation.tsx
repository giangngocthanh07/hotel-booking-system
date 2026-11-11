import { useState } from "react";
import type { ReactNode } from "react";
import type { OwnerHotelSummary } from "../../../types/owner/ownerHotel.types";

export function OwnerHotelCover({ src, name }: { src?: string | null; name: string }) {
  const [failed, setFailed] = useState(false);
  return src?.trim() && !failed
    ? <img className="owned-cover" src={src} alt={`${name} cover`} onError={() => setFailed(true)} />
    : <div className="owned-cover owned-cover--empty" role="img" aria-label="No cover image">No cover image</div>;
}

export function OwnerHotelStatus({ hotel }: { hotel: OwnerHotelSummary }) {
  const verification = hotel.isVerified === true ? "Approved" : hotel.isVerified === false ? "Not verified" : "Verification unknown";
  return <div className="owned-statuses">
    <span className="owned-status" data-status={hotel.status ?? "Unknown"}>{hotel.status || "Status unknown"}</span>
    <span className="owned-verification">{verification}</span>
  </div>;
}

export function HotelField({ label, value }: { label: string; value?: ReactNode }) {
  return <div className="owned-field"><dt>{label}</dt><dd>{value == null || value === "" ? "Not provided" : value}</dd></div>;
}

