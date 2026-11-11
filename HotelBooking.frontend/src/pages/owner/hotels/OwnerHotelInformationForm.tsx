import { useEffect, useRef, useState } from "react";
import type { FormEvent } from "react";
import { useNavigate } from "react-router-dom";
import { updateOwnerHotelInformation } from "../../../services/owner/ownerHotelService";
import type { OwnerHotelDetails } from "../../../types/owner/ownerHotel.types";
import { hotelInformationPayload, hotelInformationValues, validateHotelInformation } from "./ownerHotelInformationModel";
import { HotelInformationTextFields, HotelLocationFields } from "./OwnerHotelInformationFields";
import type { InformationTextField, InformationLocationField } from "./OwnerHotelInformationFields";
import useOwnerHotelLocations from "./useOwnerHotelLocations";

export default function OwnerHotelInformationForm({ hotel, detailsPath }: { hotel: OwnerHotelDetails; detailsPath: string }) {
  const [values, setValues] = useState(() => hotelInformationValues(hotel));
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState("");
  const pending = useRef<AbortController | null>(null);
  const navigate = useNavigate();
  const locations = useOwnerHotelLocations(values.countryId, values.provinceId);
  useEffect(() => () => pending.current?.abort(), []);

  const changeText = (field: InformationTextField, value: string) => setValues(current => ({ ...current, [field]: value }));
  function changeLocation(field: InformationLocationField, value: number) {
    // FR-CAT-002: a child location from the previous parent must never remain selected.
    setValues(current => ({ ...current, [field]: value,
      ...(field === "countryId" ? { provinceId: 0, wardId: 0 } : field === "provinceId" ? { wardId: 0 } : {}) }));
  }

  function locationIsSelected(): boolean {
    return locations.countries.some(item => item.id === values.countryId) &&
      locations.provinces.some(item => item.id === values.provinceId) && locations.wards.some(item => item.id === values.wardId);
  }

  async function submit(event: FormEvent) {
    event.preventDefault();
    if (pending.current || locations.loading || locations.error) return;
    const validation = validateHotelInformation(values) || (!locationIsSelected() ? "Select a valid Country, Province and Ward." : "");
    setError(validation);
    if (validation) return;
    const controller = new AbortController();
    pending.current = controller;
    setSaving(true);
    try {
      await updateOwnerHotelInformation(hotel.id, hotelInformationPayload(values), controller.signal);
      if (!controller.signal.aborted) navigate(detailsPath, { replace: true, state: { hotelInformationSaved: true } });
    } catch (failure) {
      if (!controller.signal.aborted) setError(failure instanceof Error ? failure.message : "Unable to save hotel information.");
    } finally {
      if (!controller.signal.aborted) { pending.current = null; setSaving(false); }
    }
  }

  return <form className="owned-information-form" noValidate onSubmit={event => void submit(event)} aria-label="Hotel information">
    {error && <p role="alert" className="owned-state owned-error">{error}</p>}
    {locations.error && <div role="alert" className="owned-state owned-error">Unable to load location choices. <button type="button" onClick={locations.retry}>Retry locations</button></div>}
    <fieldset disabled={saving} className="owned-form-grid">
      <HotelInformationTextFields values={values} change={changeText} />
      <HotelLocationFields values={values} options={locations} change={changeLocation} />
    </fieldset>
    <div className="owned-form-actions">
      <button type="button" disabled={saving} onClick={() => navigate(detailsPath)}>Cancel</button>
      <button className="owned-save" type="submit" disabled={saving || locations.loading || Boolean(locations.error)} aria-busy={saving}>
        {saving ? "Saving…" : "Save information"}
      </button>
    </div>
  </form>;
}
