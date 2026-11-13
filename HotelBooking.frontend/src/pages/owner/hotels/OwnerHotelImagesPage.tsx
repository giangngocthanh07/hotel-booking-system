import { useCallback, useState } from "react";
import { Link, useParams, useSearchParams } from "react-router-dom";
import { getOwnerHotelMedia } from "../../../services/owner/ownerHotelMediaService";
import { OwnerHotelApiError } from "../../../services/owner/ownerHotelService";
import { ownerPageIndex } from "./ownerHotelFormatting";
import useOwnerHotelData from "./useOwnerHotelData";
import OwnerHotelImageUpload from "./OwnerHotelImageUpload";
import OwnerHotelGallery from "./OwnerHotelGallery";
import "./OwnerHotels.css";
import "./OwnerHotelImages.css";

export default function OwnerHotelImagesPage() {
  const { hotelId } = useParams();
  // Remount upload state immediately when navigating between hotels.
  return <HotelImages key={hotelId} hotelId={hotelId ?? ""} />;
}

function HotelImages({ hotelId }: { hotelId: string }) {
  const [search] = useSearchParams();
  const [saved, setSaved] = useState<"" | "upload" | "remove">("");
  const [busy, setBusy] = useState(false);
  const load = useCallback((signal: AbortSignal) => {
    if (!/^\d+$/.test(hotelId)) return Promise.reject(new OwnerHotelApiError("Invalid Hotel ID.", 400));
    return getOwnerHotelMedia(Number(hotelId), signal);
  }, [hotelId]);
  const { data, loading, error, retry } = useOwnerHotelData(`media:${hotelId}`, load);
  function uploaded() { setSaved("upload"); retry(); }
  function removed() { setSaved("remove"); retry(); }
  return <section className="owned-hotels">
    <Link className="owned-back" to={`/owner/hotels/${hotelId}?pageIndex=${ownerPageIndex(search.get("pageIndex"))}`}>Back to hotel</Link>
    <header className="owned-heading"><div><p className="owned-eyebrow">Hotel management</p><h1>Manage images</h1>
      <p>Prepare your cover and at least four distinct additional gallery images before activating your hotel.</p></div></header>
    {saved && <p role="status" className="owned-success">{saved === "upload" ? "Image uploaded successfully." : "Gallery image removed successfully."}</p>}
    {loading && <p role="status">Loading hotel images…</p>}
    {error && <div role="alert" className="owned-state owned-error"><p>{saved === "upload" ? "Your upload was saved, but the image list could not be refreshed. " : saved === "remove" ? "The image was removed, but the image list could not be refreshed. " : ""}{error}</p><button onClick={retry}>Retry loading images</button></div>}
    {!loading && !error && data && <div className="owned-media-sections">
      <section className="owned-info"><h2>Cover image</h2>
        {data.coverImageUrl ? <img className="owned-current-cover" src={data.coverImageUrl} alt="Current hotel cover" /> : <p>No cover image yet.</p>}
        <OwnerHotelImageUpload hotelId={data.hotelId} kind="cover" onSaved={uploaded} locked={busy} onBusy={setBusy} />
      </section>
      <section className="owned-info"><h2>Gallery ({data.gallery.length})</h2>
        <p>These are additional images. The cover is managed separately.</p>
        <OwnerHotelGallery media={data} locked={busy} onBusy={setBusy} onRemoved={removed} />
        <OwnerHotelImageUpload hotelId={data.hotelId} kind="gallery" onSaved={uploaded} locked={busy} onBusy={setBusy} />
      </section>
    </div>}
  </section>;
}
