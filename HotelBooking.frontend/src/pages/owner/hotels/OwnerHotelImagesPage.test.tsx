import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { deleteOwnerHotelImage, getOwnerHotelMedia, uploadOwnerHotelImage } from "../../../services/owner/ownerHotelMediaService";
import OwnerHotelImagesPage from "./OwnerHotelImagesPage";

vi.mock("../../../services/owner/ownerHotelMediaService", () => ({ deleteOwnerHotelImage: vi.fn(), getOwnerHotelMedia: vi.fn(), uploadOwnerHotelImage: vi.fn() }));
beforeEach(() => {
  vi.resetAllMocks();
  vi.mocked(getOwnerHotelMedia).mockResolvedValue({ hotelId: 42, coverImageUrl: "https://example.com/cover.jpg", gallery: [{ id: 1, imageUrl: "https://example.com/gallery.jpg", displayOrder: 0 }] });
  URL.createObjectURL = vi.fn(() => "blob:preview"); URL.revokeObjectURL = vi.fn();
});
const show = (id = "42") => render(<MemoryRouter initialEntries={[`/owner/hotels/${id}/images?pageIndex=2`]}><Routes>
  <Route path="/owner/hotels/:hotelId/images" element={<OwnerHotelImagesPage />} />
</Routes></MemoryRouter>);

describe("Owner hotel images (FR-CAT-003, FR-MED-001/002/003)", () => {
  it("removes only the selected saved gallery image and reloads", async () => {
    vi.mocked(deleteOwnerHotelImage).mockResolvedValue(true);
    show(); await screen.findByAltText("Current hotel cover");
    fireEvent.click(screen.getByRole("button", { name: "Remove gallery image 1" }));
    await waitFor(() => expect(deleteOwnerHotelImage).toHaveBeenCalledWith(42, 1, expect.any(AbortSignal)));
    await waitFor(() => expect(getOwnerHotelMedia).toHaveBeenCalledTimes(2));
    expect(screen.getByText("Gallery image removed successfully.")).toBeInTheDocument();
  });
  it("shows a blocked removal reason and leaves the image visible", async () => {
    vi.mocked(deleteOwnerHotelImage).mockRejectedValue(new Error("Add a replacement image before removing this one."));
    show(); await screen.findByAltText("Current hotel cover");
    fireEvent.click(screen.getByRole("button", { name: "Remove gallery image 1" }));
    expect(await screen.findByRole("alert")).toHaveTextContent("Add a replacement");
    expect(screen.getByAltText("Gallery image 1")).toBeInTheDocument();
  });
  it("lets the Owner discard a selected preview without deleting saved images", async () => {
    show(); await screen.findByAltText("Current hotel cover");
    fireEvent.change(screen.getByLabelText("Choose gallery image"), { target: { files: [new File(["x"], "hotel.jpg", { type: "image/jpeg" })] } });
    fireEvent.click(screen.getByRole("button", { name: "Discard gallery selection" }));
    expect(screen.queryByAltText("Gallery preview")).not.toBeInTheDocument();
    expect(deleteOwnerHotelImage).not.toHaveBeenCalled();
  });
  it("shows current cover/gallery and preserves return navigation", async () => {
    show();
    expect(await screen.findByAltText("Current hotel cover")).toHaveAttribute("src", "https://example.com/cover.jpg");
    expect(screen.getByAltText("Gallery image 1")).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Back to hotel" })).toHaveAttribute("href", "/owner/hotels/42?pageIndex=2");
  });
  it("rejects a PDF before upload", async () => {
    show(); await screen.findByAltText("Current hotel cover");
    fireEvent.change(screen.getByLabelText("Choose cover image"), { target: { files: [new File(["pdf"], "license.pdf", { type: "application/pdf" })] } });
    expect(screen.getByRole("alert")).toHaveTextContent("JPG, JPEG or PNG");
    expect(uploadOwnerHotelImage).not.toHaveBeenCalled();
  });
  it("previews then uploads gallery and refreshes server data", async () => {
    vi.mocked(uploadOwnerHotelImage).mockResolvedValue({ hotelId: 42, imageUrl: "https://example.com/new.jpg" });
    show(); await screen.findByAltText("Current hotel cover");
    const file = new File(["image"], "room.png", { type: "image/png" });
    fireEvent.change(screen.getByLabelText("Choose gallery image"), { target: { files: [file] } });
    expect(await screen.findByAltText("Gallery preview")).toHaveAttribute("src", "blob:preview");
    fireEvent.click(screen.getByRole("button", { name: "Add gallery image" }));
    await waitFor(() => expect(getOwnerHotelMedia).toHaveBeenCalledTimes(2));
    expect(uploadOwnerHotelImage).toHaveBeenCalledWith(42, "gallery", file, expect.any(AbortSignal));
    await waitFor(() => expect(URL.revokeObjectURL).toHaveBeenCalledWith("blob:preview"));
  });
  it("keeps selection and displays upload errors for retry", async () => {
    vi.mocked(uploadOwnerHotelImage).mockRejectedValue(new Error("Four additional images are required."));
    show(); await screen.findByAltText("Current hotel cover");
    fireEvent.change(screen.getByLabelText("Choose cover image"), { target: { files: [new File(["image"], "hotel.jpg", { type: "image/jpeg" })] } });
    fireEvent.click(screen.getByRole("button", { name: "Upload cover" }));
    expect(await screen.findByRole("alert")).toHaveTextContent("Four additional images are required.");
    expect(screen.getByAltText("Cover preview")).toBeInTheDocument();
  });
  it("rejects invalid route IDs", async () => {
    show("invalid"); expect(await screen.findByRole("alert")).toHaveTextContent("Invalid Hotel ID.");
    expect(getOwnerHotelMedia).not.toHaveBeenCalled();
  });
  it("rejects empty or oversized images without network access", async () => {
    show(); await screen.findByAltText("Current hotel cover");
    for (const size of [0, 10 * 1024 * 1024 + 1]) {
      const file = new File(["x"], "hotel.png", { type: "image/png" });
      Object.defineProperty(file, "size", { value: size });
      fireEvent.change(screen.getByLabelText("Choose cover image"), { target: { files: [file] } });
      expect(screen.getByRole("alert")).toHaveTextContent("at most 10 MB");
    }
    expect(uploadOwnerHotelImage).not.toHaveBeenCalled();
  });
  it("distinguishes saved upload from failed refresh", async () => {
    vi.mocked(uploadOwnerHotelImage).mockResolvedValue({ hotelId: 42, imageUrl: "https://example.com/new.jpg" });
    show(); await screen.findByAltText("Current hotel cover");
    vi.mocked(getOwnerHotelMedia).mockRejectedValueOnce(new Error("Refresh failed."));
    fireEvent.change(screen.getByLabelText("Choose cover image"), { target: { files: [new File(["x"], "hotel.jpg", { type: "image/jpeg" })] } });
    fireEvent.click(screen.getByRole("button", { name: "Upload cover" }));
    expect(await screen.findByRole("alert")).toHaveTextContent("Your upload was saved");
    expect(uploadOwnerHotelImage).toHaveBeenCalledTimes(1);
  });
  it("disables both upload forms while pending and aborts on unmount", async () => {
    vi.mocked(uploadOwnerHotelImage).mockImplementation(() => new Promise(() => {}));
    const view = show(); await screen.findByAltText("Current hotel cover");
    fireEvent.change(screen.getByLabelText("Choose cover image"), { target: { files: [new File(["x"], "hotel.jpg", { type: "image/jpeg" })] } });
    fireEvent.click(screen.getByRole("button", { name: "Upload cover" }));
    expect(screen.getByLabelText("Choose gallery image")).toBeDisabled();
    const signal = vi.mocked(uploadOwnerHotelImage).mock.calls[0][3];
    view.unmount();
    expect(signal?.aborted).toBe(true);
    expect(URL.revokeObjectURL).toHaveBeenCalledWith("blob:preview");
  });
});
