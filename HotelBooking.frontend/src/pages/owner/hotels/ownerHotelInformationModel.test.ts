import { describe, expect, it } from "vitest";
import { hotelInformationValues, hotelInformationPayload, validateHotelInformation } from "./ownerHotelInformationModel";

const hotel = { name: "Preparing Hotel", description: "", address: "123 Hotel Street", publicPhone: "0901234567",
  publicEmail: "hotel@example.com", countryId: 4, provinceId: 1, wardId: 2, latitude: 0, longitude: 106 };

describe("Owner Hotel information validation (FR-CAT-002)", () => {
  it("preserves zero coordinates when initializing and submitting", () => {
    const values = hotelInformationValues(hotel);
    expect(values.latitude).toBe("0");
    expect(validateHotelInformation(values)).toBe("");
    expect(hotelInformationPayload(values)).toMatchObject({ latitude: 0, longitude: 106 });
  });
  it("clears both coordinates using null", () => {
    const values = { ...hotelInformationValues(hotel), latitude: "", longitude: "" };
    expect(validateHotelInformation(values)).toBe("");
    expect(hotelInformationPayload(values)).toMatchObject({ latitude: null, longitude: null });
  });
  it.each([["0", ""], ["", "106"], ["91", "106"], ["0", "181"], ["NaN", "106"]])(
    "rejects invalid coordinate pair %s/%s", (latitude, longitude) => {
      expect(validateHotelInformation({ ...hotelInformationValues(hotel), latitude, longitude })).not.toBe("");
    });
});
