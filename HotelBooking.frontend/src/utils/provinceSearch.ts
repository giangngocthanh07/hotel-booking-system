import type { Province } from "../types/hotel.types";

export function normalizeVietnamese(value: string): string {
  return value
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .replace(/đ/g, "d")
    .replace(/Đ/g, "D")
    .toLowerCase()
    .trim();
}

export function filterProvinces(
  provinces: Province[],
  query: string,
): Province[] {
  const normalizedQuery = normalizeVietnamese(query);
  if (!normalizedQuery) return provinces;

  return provinces.filter((province) =>
    normalizeVietnamese(province.name).includes(normalizedQuery),
  );
}
