import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import * as api from "../../../services/owner/ownerHotelService";
import * as locations from "../../../services/ownerRegistrationService";
import OwnerHotelInformationPage from "./OwnerHotelInformationPage";
import OwnerHotelDetailsPage from "./OwnerHotelDetailsPage";

vi.mock("../../../services/owner/ownerHotelService", async original => ({
  ...await original<typeof import("../../../services/owner/ownerHotelService")>(), getOwnerHotel: vi.fn(), updateOwnerHotelInformation: vi.fn(),
}));
vi.mock("../../../services/ownerRegistrationService", () => ({ getCountries: vi.fn(), getProvinces: vi.fn(), getWards: vi.fn() }));
const hotel = { id: 42, name: "Preparing Hotel", description: "A quiet hotel.", address: "123 Hotel Street", status: "Inactive", isVerified: true,
  propertyTypeId: 1, propertyTypeName: "Hotel", countryId: 4, countryName: "Vietnam", provinceId: 1, provinceName: "First Province",
  wardId: 2, wardName: "First Ward", publicPhone: "0901234567", publicEmail: "hotel@example.com", latitude: 0, longitude: 106, taxCode: "protected-tax" };
beforeEach(() => {
  vi.resetAllMocks();
  vi.mocked(api.getOwnerHotel).mockResolvedValue(hotel);
  vi.mocked(api.updateOwnerHotelInformation).mockResolvedValue(true);
  vi.mocked(locations.getCountries).mockResolvedValue({ statusCode: "Success", message: "OK", content: [{ id: 4, name: "Vietnam" }, { id: 5, name: "Other Country" }] });
  vi.mocked(locations.getProvinces).mockImplementation(async id => ({ statusCode: "Success", message: "OK",
    content: id === 4 ? [{ id: 1, name: "First Province" }] : [{ id: 3, name: "Second Province" }] }));
  vi.mocked(locations.getWards).mockImplementation(async id => ({ statusCode: "Success", message: "OK",
    content: id === 1 ? [{ id: 2, name: "First Ward" }] : [{ id: 6, name: "Second Ward" }] }));
});

function show() {
  return render(<MemoryRouter initialEntries={["/owner/hotels/42/information?pageIndex=2"]}><Routes>
    <Route path="/owner/hotels/:hotelId/information" element={<OwnerHotelInformationPage />} />
    <Route path="/owner/hotels/:hotelId" element={<OwnerHotelDetailsPage />} />
  </Routes></MemoryRouter>);
}
async function ready() {
  await waitFor(() => expect(screen.getByRole("button", { name: "Save information" })).toBeEnabled());
}

describe("Owner Hotel information form (FR-CAT-002/003)", () => {
  it("initializes information and zero coordinates without exposing protected edit fields", async () => {
    show();
    await ready();
    expect(screen.getByLabelText("Hotel name")).toHaveValue("Preparing Hotel");
    expect(screen.getByLabelText("Latitude")).toHaveValue(0);
    expect(screen.getByLabelText("Country")).toHaveValue("4");
    expect(screen.queryByLabelText("Tax code")).not.toBeInTheDocument();
    expect(screen.queryByLabelText("Status")).not.toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Back to hotel" })).toHaveAttribute("href", "/owner/hotels/42?pageIndex=2");
  });

  it("saves and reloads Hotel details with a confirmation", async () => {
    vi.mocked(api.getOwnerHotel).mockResolvedValueOnce(hotel).mockResolvedValue({ ...hotel, name: "Changed Hotel" });
    show();
    await ready();
    fireEvent.change(screen.getByLabelText("Hotel name"), { target: { value: "Changed Hotel" } });
    fireEvent.click(screen.getByRole("button", { name: "Save information" }));
    expect(await screen.findByRole("heading", { name: "Changed Hotel" })).toBeInTheDocument();
    expect(screen.getByRole("status")).toHaveTextContent("Hotel information saved.");
    expect(api.getOwnerHotel).toHaveBeenCalledTimes(2);
    expect(api.updateOwnerHotelInformation).toHaveBeenCalledWith(42, expect.objectContaining({ name: "Changed Hotel", latitude: 0 }), expect.any(AbortSignal));
  });

  it("clears dependent selections when their parent changes", async () => {
    show();
    await ready();
    fireEvent.change(screen.getByLabelText("Country"), { target: { value: "5" } });
    expect(await screen.findByRole("option", { name: "Second Province" })).toBeInTheDocument();
    expect(screen.getByLabelText("Province")).toHaveValue("0");
    expect(screen.getByLabelText("Ward")).toHaveValue("0");
    fireEvent.change(screen.getByLabelText("Province"), { target: { value: "3" } });
    expect(await screen.findByRole("option", { name: "Second Ward" })).toBeInTheDocument();
    expect(locations.getWards).toHaveBeenLastCalledWith(3, expect.any(AbortSignal));
  });

  it("rejects an incomplete coordinate pair before saving", async () => {
    show();
    await ready();
    fireEvent.change(screen.getByLabelText("Longitude"), { target: { value: "" } });
    fireEvent.click(screen.getByRole("button", { name: "Save information" }));
    expect(await screen.findByRole("alert")).toHaveTextContent("Provide both Latitude and Longitude");
    expect(api.updateOwnerHotelInformation).not.toHaveBeenCalled();
  });

  it("preserves entered values when the backend refuses the update", async () => {
    vi.mocked(api.updateOwnerHotelInformation).mockRejectedValue(new api.OwnerHotelApiError("Hotel metadata cannot be updated safely.", 409));
    show();
    await ready();
    fireEvent.change(screen.getByLabelText("Hotel name"), { target: { value: "Changed Hotel" } });
    fireEvent.click(screen.getByRole("button", { name: "Save information" }));
    expect(await screen.findByRole("alert")).toHaveTextContent("Hotel metadata cannot be updated safely.");
    expect(screen.getByLabelText("Hotel name")).toHaveValue("Changed Hotel");
    expect(screen.queryByText("Hotel information saved.")).not.toBeInTheDocument();
  });

  it("prevents duplicate saves and aborts when leaving", async () => {
    vi.mocked(api.updateOwnerHotelInformation).mockImplementation(() => new Promise(() => {}));
    const { unmount } = show();
    await ready();
    fireEvent.submit(screen.getByRole("form", { name: "Hotel information" }));
    fireEvent.submit(screen.getByRole("form", { name: "Hotel information" }));
    expect(screen.getByRole("button", { name: "Saving…" })).toBeDisabled();
    expect(api.updateOwnerHotelInformation).toHaveBeenCalledTimes(1);
    const signal = vi.mocked(api.updateOwnerHotelInformation).mock.calls[0][2];
    unmount();
    expect(signal?.aborted).toBe(true);
  });
});
