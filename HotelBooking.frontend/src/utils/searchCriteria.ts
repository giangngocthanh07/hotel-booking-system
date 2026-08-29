import type { SearchCriteria } from "../types/hotel.types";

function isIsoDate(value: string | null): value is string {
  if (!value || !/^\d{4}-\d{2}-\d{2}$/.test(value)) return false;
  const date = new Date(`${value}T00:00:00Z`);
  return !Number.isNaN(date.getTime()) && date.toISOString().slice(0, 10) === value;
}

function parseInteger(value: string | null, minimum: number): number | null {
  if (value === null || !/^\d+$/.test(value)) return null;
  const parsed = Number(value);
  return Number.isSafeInteger(parsed) && parsed >= minimum ? parsed : null;
}

export function parseSearchCriteria(
  params: URLSearchParams,
): SearchCriteria | null {
  const cityName = params.get("cityName")?.trim() ?? "";
  const checkIn = params.get("checkIn");
  const checkOut = params.get("checkOut");
  const adults = parseInteger(params.get("adults"), 1);
  const children = parseInteger(params.get("children"), 0);
  const rooms = parseInteger(params.get("rooms"), 1);

  if (!cityName || !isIsoDate(checkIn) || !isIsoDate(checkOut)) return null;
  if (checkOut <= checkIn || adults === null || children === null || rooms === null) {
    return null;
  }
  return { cityName, checkIn, checkOut, adults, children, rooms };
}
