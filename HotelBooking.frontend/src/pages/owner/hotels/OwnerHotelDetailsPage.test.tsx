import { render, screen } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { getOwnerHotel, OwnerHotelApiError } from "../../../services/owner/ownerHotelService";
import OwnerHotelDetailsPage from "./OwnerHotelDetailsPage";

vi.mock("../../../services/owner/ownerHotelService", async (original) => ({
  ...await original<typeof import("../../../services/owner/ownerHotelService")>(), getOwnerHotel: vi.fn(),
}));
const hotel = {
  id: 42, name: "Preparing Hotel", address: "1 Lake Street", status: "Suspended", isVerified: true,
  description: "A quiet place.", propertyTypeId: 1, propertyTypeName: "Hotel", countryId: 4, countryName: "Vietnam",
  provinceId: 1, provinceName: "City", wardId: 1, wardName: "Ward", publicPhone: "0901234567",
  publicEmail: "hotel@example.com", latitude: 0, longitude: 106, starRating: 3, taxCode: "1234567890",
};
beforeEach(() => { vi.resetAllMocks(); vi.mocked(getOwnerHotel).mockResolvedValue(hotel); });
const show = (id = "42") => render(<MemoryRouter initialEntries={[`/owner/hotels/${id}?pageIndex=2`]}>
  <Routes><Route path="/owner/hotels/:hotelId" element={<OwnerHotelDetailsPage />} /></Routes>
</MemoryRouter>);

describe("owned Hotel details (FR-CAT-001/003/008)", () => {
  it("shows operational/contact/location data, including zero coordinates", async () => {
    show();
    expect(await screen.findByRole("heading", { name: "Preparing Hotel" })).toBeInTheDocument();
    expect(screen.getByText("Suspended")).toBeInTheDocument();
    expect(screen.getByText("0, 106")).toBeInTheDocument();
    expect(screen.getByText("hotel@example.com")).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Back to My Hotels" })).toHaveAttribute("href", "/owner/hotels?pageIndex=2");
    expect(screen.queryByRole("button", { name: /activate/i })).not.toBeInTheDocument();
  });

  it("shows the same unavailable state for foreign/deleted/missing Hotels", async () => {
    vi.mocked(getOwnerHotel).mockRejectedValue(new OwnerHotelApiError("Hotel is unavailable.", 404));
    show();
    expect(await screen.findByRole("alert")).toHaveTextContent("Hotel is unavailable.");
    expect(screen.queryByText("hotel@example.com")).not.toBeInTheDocument();
  });

  it("does not fetch invalid route IDs", async () => {
    show("invalid");
    expect(await screen.findByRole("alert")).toHaveTextContent("Invalid Hotel ID.");
    expect(getOwnerHotel).not.toHaveBeenCalled();
  });
});
