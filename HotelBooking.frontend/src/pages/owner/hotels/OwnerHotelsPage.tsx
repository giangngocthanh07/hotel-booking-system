import { useCallback } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { getOwnerHotels } from "../../../services/owner/ownerHotelService";
import { OwnerHotelCover, OwnerHotelStatus } from "./OwnerHotelPresentation";
import { ownerPageIndex } from "./ownerHotelFormatting";
import useOwnerHotelData from "./useOwnerHotelData";
import "./OwnerHotels.css";

export default function OwnerHotelsPage() {
  const [search, setSearch] = useSearchParams();
  const page = ownerPageIndex(search.get("pageIndex"));
  const load = useCallback((signal: AbortSignal) => getOwnerHotels(page, 6, signal), [page]);
  const { data, loading, error, retry } = useOwnerHotelData(`page:${page}`, load);
  return <section className="owned-hotels">
    <header className="owned-heading"><div><p className="owned-eyebrow">Your properties</p><h1>My Hotels</h1>
      <p>View your hotels and their operational status.</p></div><Link className="owned-action" to="/owner/registration">Register a hotel</Link></header>
    {loading && <p role="status" className="owned-state">Loading hotels…</p>}
    {error && <div role="alert" className="owned-state owned-error">{error} <button onClick={retry}>Retry</button></div>}
    {!loading && !error && data && <>
      {data.items.length === 0 ? <div className="owned-state"><h2>{data.totalCount === 0 ? "No hotels yet" : "No hotels on this page"}</h2>
        <p>{data.totalCount === 0 ? "Approved hotel registrations will appear here." : "Return to the first page to see your hotels."}</p>
        {page > 1 && <button onClick={() => setSearch({ pageIndex: "1" })}>First page</button>}</div>
        : <div className="owned-grid">{data.items.map(hotel => <article className="owned-card" key={hotel.id}>
          <OwnerHotelCover key={hotel.coverImageUrl} src={hotel.coverImageUrl} name={hotel.name} />
          <div className="owned-card-body"><OwnerHotelStatus hotel={hotel} /><h2>
            <Link to={`/owner/hotels/${hotel.id}?pageIndex=${page}`} aria-label={`View ${hotel.name}`}>{hotel.name}</Link></h2>
            <p>{hotel.address || "Address not provided"}</p></div></article>)}</div>}
      {data.totalCount > 0 && <nav className="owned-pagination" aria-label="Hotel pages">
        <button disabled={page <= 1} onClick={() => setSearch({ pageIndex: String(page - 1) })}>Previous</button>
        <span role="status">Page {page} of {data.totalPages} · {data.totalCount} hotels</span>
        <button disabled={page >= data.totalPages} onClick={() => setSearch({ pageIndex: String(page + 1) })}>Next</button>
      </nav>}
    </>}
  </section>;
}
