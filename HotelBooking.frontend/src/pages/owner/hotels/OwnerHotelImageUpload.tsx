import { useEffect, useRef, useState } from "react";
import { uploadOwnerHotelImage } from "../../../services/owner/ownerHotelMediaService";
import type { HotelImageKind } from "../../../types/owner/ownerHotelMedia.types";

function validation(file: File): string {
  // FR-MED-002/003: client checks improve feedback; server validates actual content again.
  const extension = file.name.split(".").pop()?.toLowerCase();
  if (!((extension === "jpg" || extension === "jpeg") && file.type === "image/jpeg") && !(extension === "png" && file.type === "image/png"))
    return "Please choose a JPG, JPEG or PNG photo, up to 10 MB. Other file formats are not supported. Renaming a file does not convert it into a photo.";
  return file.size < 1 || file.size > 10 * 1024 * 1024 ? "Choose a photo that is not empty and at most 10 MB." : "";
}

export default function OwnerHotelImageUpload({ hotelId, kind, onSaved, locked, onBusy }: {
  hotelId: number; kind: HotelImageKind; onSaved: () => void; locked: boolean; onBusy: (busy: boolean) => void;
}) {
  const [file, setFile] = useState<File | null>(null);
  const [error, setError] = useState("");
  const [busy, setBusy] = useState(false);
  const request = useRef<AbortController | null>(null);
  const input = useRef<HTMLInputElement | null>(null);
  const cover = kind === "cover";
  useEffect(() => () => request.current?.abort(), []);

  async function upload() {
    if (!file || request.current || locked) return;
    const controller = new AbortController(); request.current = controller;
    setBusy(true); onBusy(true); setError("");
    try {
      await uploadOwnerHotelImage(hotelId, kind, file, controller.signal);
      if (!controller.signal.aborted) {
        setFile(null);
        if (input.current) input.current.value = "";
        onSaved();
      }
    } catch (failure) {
      if (!controller.signal.aborted) setError(failure instanceof Error ? failure.message : "Unable to upload image. Please try again.");
    } finally {
      request.current = null;
      if (!controller.signal.aborted) { setBusy(false); onBusy(false); }
    }
  }

  return <div className="owned-image-upload">
    <label htmlFor={`hotel-image-${kind}`}>Choose {kind} image</label>
    <input ref={input} id={`hotel-image-${kind}`} type="file" accept=".jpg,.jpeg,.png,image/jpeg,image/png" disabled={locked}
      onChange={event => {
        const selected = event.target.files?.[0] ?? null;
        const issue = selected ? validation(selected) : "";
        setError(issue); setFile(issue ? null : selected);
        if (issue) event.target.value = "";
      }} />
    <p className="owned-image-help">Accepted photos: JPG, JPEG or PNG, maximum 10 MB each. Choose an original photo or export it as JPG/PNG; changing the filename alone is not enough.</p>
    {cover && <p className="owned-image-help">Recommended cover: <strong>landscape 16:9</strong>, <strong>1600 × 900 px</strong>. Choose or resize your photo to this ratio for the best fit.</p>}
    {file && <div className="owned-gallery-item owned-preview-item">
      <ImagePreview file={file} label={`${cover ? "Cover" : "Gallery"} preview`} />
      <button type="button" className="owned-image-remove" aria-label={`Discard ${kind} selection`} title="Discard selected photo" disabled={locked}
        onClick={() => { setFile(null); setError(""); if (input.current) input.current.value = ""; }}>×</button>
    </div>}
    {error && <p role="alert" className="owned-error">{error}</p>}
    <button className="owned-action" type="button" disabled={!file || locked} onClick={() => void upload()}>
      {busy ? "Uploading…" : cover ? "Upload cover" : "Add gallery image"}
    </button>
    {busy && <p role="status">Uploading image. Please wait…</p>}
  </div>;
}

function ImagePreview({ file, label }: { file: File; label: string }) {
  const image = useRef<HTMLImageElement | null>(null);
  useEffect(() => {
    const url = URL.createObjectURL(file);
    if (image.current) image.current.src = url;
    return () => URL.revokeObjectURL(url);
  }, [file]);
  return <img ref={image} className="owned-image-preview" alt={label} />;
}
