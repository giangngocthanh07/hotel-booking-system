import { useCallback } from "react";
import { getCountries, getProvinces, getWards } from "../../../services/ownerRegistrationService";
import type { ApiResponse, LocationItem } from "../../../services/ownerRegistrationService";
import useOwnerHotelData from "./useOwnerHotelData";

function items(response: ApiResponse<LocationItem[]>): LocationItem[] {
  if (response.statusCode !== "Success" || !Array.isArray(response.content)) throw new Error("Unable to load location choices.");
  return response.content;
}

export default function useOwnerHotelLocations(countryId: number, provinceId: number) {
  const loadCountries = useCallback(async (signal: AbortSignal) => items(await getCountries(signal)), []);
  const loadProvinces = useCallback(async (signal: AbortSignal) => countryId ? items(await getProvinces(countryId, signal)) : [], [countryId]);
  const loadWards = useCallback(async (signal: AbortSignal) => provinceId ? items(await getWards(provinceId, signal)) : [], [provinceId]);
  const countries = useOwnerHotelData("countries", loadCountries);
  const provinces = useOwnerHotelData(`provinces:${countryId}`, loadProvinces);
  const wards = useOwnerHotelData(`wards:${provinceId}`, loadWards);
  return { countries: countries.data ?? [], provinces: provinces.data ?? [], wards: wards.data ?? [],
    loading: countries.loading || provinces.loading || wards.loading,
    error: countries.error || provinces.error || wards.error,
    retry: () => { countries.retry(); provinces.retry(); wards.retry(); } };
}
