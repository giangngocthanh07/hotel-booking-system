import type { OwnerHotelDetails, OwnerHotelInformationUpdate } from "../../../types/owner/ownerHotel.types";

export interface HotelInformationValues extends Omit<OwnerHotelInformationUpdate, "latitude" | "longitude"> {
  latitude: string;
  longitude: string;
}
type Source = Pick<OwnerHotelDetails, keyof OwnerHotelInformationUpdate>;

export function hotelInformationValues(hotel: Source): HotelInformationValues {
  return { name: hotel.name, description: hotel.description ?? "", address: hotel.address, publicPhone: hotel.publicPhone ?? "",
    publicEmail: hotel.publicEmail ?? "", countryId: hotel.countryId, provinceId: hotel.provinceId, wardId: hotel.wardId,
    latitude: hotel.latitude == null ? "" : String(hotel.latitude), longitude: hotel.longitude == null ? "" : String(hotel.longitude) };
}

export function validateHotelInformation(values: HotelInformationValues): string {
  if (values.name.trim().length < 6 || values.name.trim().length > 50) return "Hotel name must contain 6 to 50 characters.";
  if (values.description.length > 500) return "Description must contain no more than 500 characters.";
  if (values.address.trim().length < 10 || values.address.trim().length > 500) return "Address must contain 10 to 500 characters.";
  if (!/^[0-9]{10}$/.test(values.publicPhone)) return "Phone must contain exactly 10 digits.";
  if (values.publicEmail.length > 254 || !/^[^\s@]+@[^\s@]+$/.test(values.publicEmail)) return "Provide a valid email address.";
  if (!values.countryId || !values.provinceId || !values.wardId) return "Select Country, Province and Ward.";
  return validateCoordinates(values);
}

function validateCoordinates(values: HotelInformationValues): string {
  const latitude = values.latitude.trim(), longitude = values.longitude.trim();
  if (Boolean(latitude) !== Boolean(longitude)) return "Provide both Latitude and Longitude, or leave both empty.";
  if (!latitude) return "";
  if (!Number.isFinite(Number(latitude)) || Math.abs(Number(latitude)) > 90) return "Latitude must be between -90 and 90.";
  if (!Number.isFinite(Number(longitude)) || Math.abs(Number(longitude)) > 180) return "Longitude must be between -180 and 180.";
  return "";
}

export function hotelInformationPayload(values: HotelInformationValues): OwnerHotelInformationUpdate {
  return { ...values, name: values.name.trim(), description: values.description.trim(), address: values.address.trim(),
    latitude: values.latitude.trim() === "" ? null : Number(values.latitude),
    longitude: values.longitude.trim() === "" ? null : Number(values.longitude) };
}
