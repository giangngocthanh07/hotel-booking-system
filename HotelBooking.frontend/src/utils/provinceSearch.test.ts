import { describe, expect, it } from "vitest";
import type { Province } from "../types/hotel.types";
import { filterProvinces, normalizeVietnamese } from "./provinceSearch";

const provinces: Province[] = [
  { id: 1, name: "Đà Nẵng" },
  { id: 2, name: "Thành phố Hồ Chí Minh" },
  { id: 3, name: "Hà Nội" },
];

describe("province search", () => {
  it("normalizes Vietnamese accents and đ", () => {
    expect(normalizeVietnamese("Đà Nẵng")).toBe("da nang");
  });

  it("filters province names without requiring accents or matching case", () => {
    expect(filterProvinces(provinces, "DA nang")).toEqual([provinces[0]]);
  });

  it("returns all provinces for a blank query", () => {
    expect(filterProvinces(provinces, "  ")).toEqual(provinces);
  });
});
