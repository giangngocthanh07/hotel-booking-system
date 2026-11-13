import { useEffect, useRef, useState } from "react";
import { deleteOwnerHotelImage } from "../../../services/owner/ownerHotelMediaService";
import type { OwnerHotelMedia } from "../../../types/owner/ownerHotelMedia.types";

export default function OwnerHotelGallery({ media, locked, onBusy, onRemoved }: {
  media: OwnerHotelMedia; locked: boolean; onBusy: (busy: boolean) => void; onRemoved: () => void;
}) {
  const [removing, setRemoving] = useState<number | null>(null);
  const [error, setError] = useState("");
  const request = useRef<AbortController | null>(null);
  useEffect(() => () => request.current?.abort(), []);

  async function remove(imageId: number) {
    if (locked || request.current) return;
    const controller = new AbortController(); request.current = controller;
    setRemoving(imageId); setError(""); onBusy(true);
    try {
      await deleteOwnerHotelImage(media.hotelId, imageId, controller.signal);
      if (!controller.signal.aborted) onRemoved();
    } catch (failure) {
      if (!controller.signal.aborted) setError(failure instanceof Error ? failure.message : "Unable to remove image. Please try again.");
    } finally {
      request.current = null;
      if (!controller.signal.aborted) { setRemoving(null); onBusy(false); }
    }
  }

  return <>
    {error && <p role="alert" className="owned-error">{error}</p>}
    {removing != null && <p role="status">Removing gallery image…</p>}
    {media.gallery.length ? <div className="owned-gallery">{media.gallery.map((image, index) =>
      <div className="owned-gallery-item" key={image.id}>
        <img src={image.imageUrl} alt={`Gallery image ${index + 1}`} loading="lazy" />
        <button type="button" className="owned-image-remove" aria-label={`Remove gallery image ${index + 1}`}
          title="Remove this gallery image" disabled={locked} onClick={() => void remove(image.id)}>×</button>
      </div>)}</div> : <p>No gallery images yet.</p>}
  </>;
}
