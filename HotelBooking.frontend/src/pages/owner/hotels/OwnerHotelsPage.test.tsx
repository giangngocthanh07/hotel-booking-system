import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { Link, MemoryRouter } from "react-router-dom";
import type { OwnerHotelsPageData } from "../../../types/owner/ownerHotel.types";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { getOwnerHotels } from "../../../services/owner/ownerHotelService";
import OwnerHotelsPage from "./OwnerHotelsPage";

vi.mock("../../../services/owner/ownerHotelService", () => ({ getOwnerHotels: vi.fn() }));
const hotel = { id: 42, name: "Preparing Hotel", address: "1 Lake Street", status: "Inactive", isVerified: true };
const page = { items: [hotel], totalCount: 7, totalPages: 2, pageIndex: 1, pageSize: 6 };
beforeEach(() => { vi.resetAllMocks(); vi.mocked(getOwnerHotels).mockResolvedValue(page); });
const show = () => render(<MemoryRouter initialEntries={["/owner/hotels"]}><OwnerHotelsPage /></MemoryRouter>);

describe("My Hotels (FR-CAT-001/003/008)", () => {
  it("shows Inactive Hotels, verification and a management link", async () => {
    show();
    expect(await screen.findByText("Preparing Hotel")).toBeInTheDocument();
    expect(screen.getByText("Inactive")).toBeInTheDocument();
    expect(screen.getByText("Approved")).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "View Preparing Hotel" })).toHaveAttribute("href", "/owner/hotels/42?pageIndex=1");
    expect(screen.getByRole("img", { name: "No cover image" })).toBeInTheDocument();
  });

  it("loads the next server page and preserves the return page in links", async () => {
    show();
    await screen.findByText("Preparing Hotel");
    fireEvent.click(screen.getByRole("button", { name: "Next" }));
    await waitFor(() => expect(getOwnerHotels).toHaveBeenLastCalledWith(2, 6, expect.any(AbortSignal)));
    expect(await screen.findByRole("link", { name: "View Preparing Hotel" })).toHaveAttribute("href", "/owner/hotels/42?pageIndex=2");
  });

  it("offers registration when there are no owned Hotels", async () => {
    vi.mocked(getOwnerHotels).mockResolvedValue({ ...page, items: [], totalCount: 0, totalPages: 0 });
    show();
    expect(await screen.findByText("No hotels yet")).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Register a hotel" })).toHaveAttribute("href", "/owner/registration");
  });

  it("shows failures and supports retry", async () => {
    vi.mocked(getOwnerHotels).mockRejectedValueOnce(new Error("Unable to load hotels."));
    show();
    expect(await screen.findByRole("alert")).toHaveTextContent("Unable to load hotels.");
    fireEvent.click(screen.getByRole("button", { name: "Retry" }));
    expect(await screen.findByText("Preparing Hotel")).toBeInTheDocument();
  });

  it("aborts pending page requests when leaving", () => {
    vi.mocked(getOwnerHotels).mockImplementation(() => new Promise(() => {}));
    const { unmount } = show();
    expect(screen.getByRole("status")).toHaveTextContent("Loading hotels");
    const signal = vi.mocked(getOwnerHotels).mock.calls[0][2];
    unmount();
    expect(signal?.aborted).toBe(true);
  });

  it("ignores a late response from the previous page", async () => {
    let finishFirst!: (value: OwnerHotelsPageData) => void;
    vi.mocked(getOwnerHotels).mockImplementationOnce(() => new Promise(resolve => { finishFirst = resolve; }))
      .mockResolvedValueOnce({ ...page, items: [{ ...hotel, id: 43, name: "Second Hotel" }], pageIndex: 2 });
    render(<MemoryRouter initialEntries={["/owner/hotels"]}>
      <Link to="/owner/hotels?pageIndex=2">Go to page two</Link><OwnerHotelsPage />
    </MemoryRouter>);
    const firstSignal = vi.mocked(getOwnerHotels).mock.calls[0][2];
    fireEvent.click(screen.getByRole("link", { name: "Go to page two" }));
    expect(await screen.findByText("Second Hotel")).toBeInTheDocument();
    finishFirst(page);
    await waitFor(() => expect(firstSignal?.aborted).toBe(true));
    expect(screen.queryByText("Preparing Hotel")).not.toBeInTheDocument();
  });
});
