import { describe, expect, it } from "vitest";
import { parseSearchCriteria } from "./searchCriteria";

const validQuery =
  "cityName=H%C3%A0+N%E1%BB%99i&checkIn=2026-09-01&checkOut=2026-09-03&adults=2&children=1&rooms=1";

describe("parseSearchCriteria", () => {
  it("parses a complete valid query", () => {
    expect(parseSearchCriteria(new URLSearchParams(validQuery))).toEqual({
      cityName: "Hà Nội",
      checkIn: "2026-09-01",
      checkOut: "2026-09-03",
      adults: 2,
      children: 1,
      rooms: 1,
    });
  });

  it.each([
    ["missing city", validQuery.replace("cityName=H%C3%A0+N%E1%BB%99i&", "")],
    ["bad check-in", validQuery.replace("2026-09-01", "01-09-2026")],
    ["checkout before check-in", validQuery.replace("2026-09-03", "2026-08-31")],
    ["zero adults", validQuery.replace("adults=2", "adults=0")],
    ["negative children", validQuery.replace("children=1", "children=-1")],
    ["zero rooms", validQuery.replace("rooms=1", "rooms=0")],
  ])("rejects %s", (_name, query) => {
    expect(parseSearchCriteria(new URLSearchParams(query))).toBeNull();
  });
});
