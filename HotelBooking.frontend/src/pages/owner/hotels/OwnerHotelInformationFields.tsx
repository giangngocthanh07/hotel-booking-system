import type { LocationItem } from "../../../services/ownerRegistrationService";
import type { HotelInformationValues } from "./ownerHotelInformationModel";

export type InformationTextField = "name" | "description" | "address" | "publicPhone" | "publicEmail" | "latitude" | "longitude";
export type InformationLocationField = "countryId" | "provinceId" | "wardId";

export function HotelInformationTextFields({ values, change }: {
  values: HotelInformationValues; change: (field: InformationTextField, value: string) => void;
}) {
  return <>
    <section className="owned-info"><h2>Property information</h2><div className="owned-form-fields">
      <label>Hotel name<input required maxLength={50} value={values.name} onChange={event => change("name", event.target.value)} /></label>
      <label>Description<textarea rows={4} maxLength={500} value={values.description} onChange={event => change("description", event.target.value)} /></label>
      <label>Address<input required maxLength={500} value={values.address} onChange={event => change("address", event.target.value)} /></label>
    </div></section>
    <section className="owned-info"><h2>Contact information</h2><div className="owned-form-fields">
      <label>Phone<input required type="tel" maxLength={10} value={values.publicPhone} onChange={event => change("publicPhone", event.target.value)} /></label>
      <label>Email<input required type="email" maxLength={254} value={values.publicEmail} onChange={event => change("publicEmail", event.target.value)} /></label>
    </div></section>
    <section className="owned-info"><h2>Coordinates</h2><p>Provide both coordinates, or leave both blank.</p><div className="owned-form-fields">
      <label>Latitude<input type="number" min={-90} max={90} step="any" value={values.latitude} onChange={event => change("latitude", event.target.value)} /></label>
      <label>Longitude<input type="number" min={-180} max={180} step="any" value={values.longitude} onChange={event => change("longitude", event.target.value)} /></label>
    </div></section>
  </>;
}

export function HotelLocationFields({ values, options, change }: {
  values: HotelInformationValues;
  options: { countries: LocationItem[]; provinces: LocationItem[]; wards: LocationItem[]; loading: boolean };
  change: (field: InformationLocationField, value: number) => void;
}) {
  const fields = [
    { key: "countryId", label: "Country", items: options.countries },
    { key: "provinceId", label: "Province", items: options.provinces },
    { key: "wardId", label: "Ward", items: options.wards },
  ] as const;
  return <section className="owned-info"><h2>Location</h2><div className="owned-form-fields">
    {fields.map(field => <label key={field.key}>{field.label}<select required value={values[field.key]}
      disabled={options.loading} onChange={event => change(field.key, Number(event.target.value))}>
      <option value={0} disabled>Select {field.label}</option>
      {field.items.map(item => <option key={item.id} value={item.id}>{item.name}</option>)}
    </select></label>)}
    {options.loading && <p role="status">Loading location choices…</p>}
  </div></section>;
}
