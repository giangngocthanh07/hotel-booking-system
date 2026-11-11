import { useCallback } from "react";
import { Link, useParams, useSearchParams } from "react-router-dom";
import { getOwnerHotel, OwnerHotelApiError } from "../../../services/owner/ownerHotelService";
import { ownerPageIndex } from "./ownerHotelFormatting";
import OwnerHotelInformationForm from "./OwnerHotelInformationForm";
import useOwnerHotelData from "./useOwnerHotelData";
import "./OwnerHotels.css";

export default function OwnerHotelInformationPage() {
  const { hotelId } = useParams();
  const [search] = useSearchParams();
  const page = ownerPageIndex(search.get("pageIndex"));
  const load = useCallback((signal: AbortSignal) => {
    if (!hotelId || !/^\d+$/.test(hotelId) || Number(hotelId) <= 0 || Number(hotelId) > 2147483647)
      return Promise.reject(new OwnerHotelApiError("Invalid Hotel ID.", 400));
    return getOwnerHotel(Number(hotelId), signal);
  }, [hotelId]);
  const { data, loading, error, retry } = useOwnerHotelData(`information:${hotelId}`, load);
  const detailsPath = `/owner/hotels/${hotelId}?pageIndex=${page}`;
  return <section className="owned-hotels">
    <Link className="owned-back" to={data ? detailsPath : `/owner/hotels?pageIndex=${page}`}>{data ? "Back to hotel" : "Back to My Hotels"}</Link>
    <header className="owned-heading"><div><p className="owned-eyebrow">{data?.name || "Your hotel"}</p><h1>Edit hotel information</h1></div></header>
    {loading && <p role="status" className="owned-state">Loading hotel information…</p>}
    {error && <div role="alert" className="owned-state owned-error">{error} <button onClick={retry}>Retry</button></div>}
    {!loading && !error && data && <OwnerHotelInformationForm key={data.id} hotel={data} detailsPath={detailsPath} />}
  </section>;
}
