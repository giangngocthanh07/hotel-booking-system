import { useCallback } from "react";
import { Link, useLocation, useParams, useSearchParams } from "react-router-dom";
import { getOwnerHotel, OwnerHotelApiError } from "../../../services/owner/ownerHotelService";
import type { OwnerHotelDetails } from "../../../types/owner/ownerHotel.types";
import { HotelField, OwnerHotelCover, OwnerHotelStatus } from "./OwnerHotelPresentation";
import { ownerHotelDate, ownerPageIndex } from "./ownerHotelFormatting";
import useOwnerHotelData from "./useOwnerHotelData";
import "./OwnerHotels.css";

export default function OwnerHotelDetailsPage() {
  const { hotelId } = useParams();
  const [search] = useSearchParams();
  const location = useLocation();
  const load = useCallback((signal: AbortSignal) => {
    if (!hotelId || !/^\d+$/.test(hotelId) || Number(hotelId) <= 0 || Number(hotelId) > 2147483647)
      return Promise.reject(new OwnerHotelApiError("Invalid Hotel ID.", 400));
    return getOwnerHotel(Number(hotelId), signal);
  }, [hotelId]);
  const { data, loading, error, retry } = useOwnerHotelData(`hotel:${hotelId}`, load);
  return <section className="owned-hotels">
    <Link className="owned-back" to={`/owner/hotels?pageIndex=${ownerPageIndex(search.get("pageIndex"))}`}>Back to My Hotels</Link>
    {loading && <p role="status" className="owned-state">Loading hotel details…</p>}
    {error && <div role="alert" className="owned-state owned-error"><h1>Hotel details unavailable</h1><p>{error}</p><button onClick={retry}>Retry</button></div>}
    {!loading && !error && data && <>
      {location.state?.hotelInformationSaved === true && <p role="status" className="owned-success">Hotel information saved.</p>}
      <header className="owned-heading"><div><p className="owned-eyebrow">Hotel details</p><h1>{data.name}</h1><p>{data.address}</p></div>
        <div className="owned-detail-actions"><OwnerHotelStatus hotel={data} />
          <Link className="owned-action" to={`/owner/hotels/${data.id}/information?pageIndex=${ownerPageIndex(search.get("pageIndex"))}`}>Edit information</Link></div></header>
      <div className="owned-detail-cover"><OwnerHotelCover key={data.coverImageUrl} src={data.coverImageUrl} name={data.name} /></div>
      <HotelInformation hotel={data} />
    </>}
  </section>;
}

function HotelInformation({ hotel }: { hotel: OwnerHotelDetails }) {
  const coordinates = hotel.latitude != null && hotel.longitude != null ? `${hotel.latitude}, ${hotel.longitude}` : undefined;
  return <div className="owned-info-grid">
    <section className="owned-info"><h2>Property information</h2><p className="owned-description">{hotel.description || "No description provided."}</p>
      <dl><HotelField label="Property type" value={hotel.propertyTypeName} /><HotelField label="Star rating" value={hotel.starRating != null ? `${hotel.starRating} stars` : undefined} />
        <HotelField label="Tax code" value={hotel.taxCode} /><HotelField label="Created" value={ownerHotelDate(hotel.createdAt)} /></dl></section>
    <section className="owned-info"><h2>Contact and location</h2><dl>
      <HotelField label="Phone" value={hotel.publicPhone} /><HotelField label="Email" value={hotel.publicEmail} />
      <HotelField label="Address" value={hotel.address} /><HotelField label="Ward" value={hotel.wardName} />
      <HotelField label="Province" value={hotel.provinceName} /><HotelField label="Country" value={hotel.countryName} />
      <HotelField label="Coordinates" value={coordinates} /></dl></section>
  </div>;
}
