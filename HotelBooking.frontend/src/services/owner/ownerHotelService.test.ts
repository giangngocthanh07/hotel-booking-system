import { afterEach, describe, expect, it, vi } from "vitest";
import { getOwnerHotel, getOwnerHotels } from "./ownerHotelService";

vi.mock("../authService", () => ({ getAuthToken: () => "owner-token" }));
afterEach(() => vi.unstubAllGlobals());

describe("Owner Hotel reads (FR-CAT-001/003/008)", () => {
  it("requests an Owner-scoped page with authentication and cancellation", async () => {
    const content = { items: [], totalCount: 0, totalPages: 0, pageIndex: 2, pageSize: 6 };
    const fetchMock = vi.fn().mockResolvedValue({ ok: true, json: async () => ({ statusCode: "Success", content }) });
    vi.stubGlobal("fetch", fetchMock);
    const signal = new AbortController().signal;
    await expect(getOwnerHotels(2, 6, signal)).resolves.toEqual(content);
    expect(fetchMock).toHaveBeenCalledWith(expect.stringContaining("/owner/hotels?pageIndex=2&pageSize=6"), {
      headers: { Authorization: "Bearer owner-token" }, signal, cache: "no-store",
    });
  });

  it("uses the management detail endpoint, including for Inactive Hotels", async () => {
    const content = { id: 42, name: "Preparing Hotel", status: "Inactive" };
    const fetchMock = vi.fn().mockResolvedValue({ ok: true, json: async () => ({ statusCode: "Success", content }) });
    vi.stubGlobal("fetch", fetchMock);
    await expect(getOwnerHotel(42)).resolves.toEqual(content);
    expect(fetchMock.mock.calls[0][0]).toMatch(/\/owner\/hotels\/42$/);
  });

  it.each([401, 403, 404, 500])("reports HTTP %s even when the body is empty", async (status) => {
    vi.stubGlobal("fetch", vi.fn().mockResolvedValue({ ok: false, status, json: async () => { throw new Error("Empty body"); } }));
    await expect(getOwnerHotel(42)).rejects.toMatchObject({ status });
  });

  it("rejects an invalid Hotel ID before fetching", async () => {
    const fetchMock = vi.fn();
    vi.stubGlobal("fetch", fetchMock);
    await expect(getOwnerHotel(0)).rejects.toThrow("Invalid Hotel ID.");
    expect(fetchMock).not.toHaveBeenCalled();
  });
});
