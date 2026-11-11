import { afterEach, describe, expect, it, vi } from "vitest";
import { getOwnerHotel, getOwnerHotels, updateOwnerHotelInformation } from "./ownerHotelService";

vi.mock("../authService", () => ({ getAuthToken: () => "owner-token" }));
afterEach(() => vi.unstubAllGlobals());

describe("Owner Hotel reads and updates (FR-CAT-001/002/003/008)", () => {
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

  it("updates only editable information with PUT and a bearer header", async () => {
    const input = { name: "Updated Hotel", description: "Description", address: "123 Updated Street", publicPhone: "0901234567",
      publicEmail: "hotel@example.com", countryId: 4, provinceId: 1, wardId: 2, latitude: 0, longitude: 106,
      status: "Active", ownerId: 999, taxCode: "untrusted" };
    const fetchMock = vi.fn().mockResolvedValue({ ok: true, json: async () => ({ statusCode: "Success", content: true }) });
    vi.stubGlobal("fetch", fetchMock);
    await expect(updateOwnerHotelInformation(42, input)).resolves.toBe(true);
    const [url, options] = fetchMock.mock.calls[0];
    expect(url).toMatch(/\/owner\/hotels\/42\/information$/);
    expect(options.method).toBe("PUT");
    expect(options.headers).toMatchObject({ Authorization: "Bearer owner-token", "Content-Type": "application/json" });
    expect(JSON.parse(options.body)).toMatchObject({ name: "Updated Hotel", latitude: 0 });
    expect(JSON.parse(options.body)).not.toHaveProperty("status");
    expect(JSON.parse(options.body)).not.toHaveProperty("ownerId");
    expect(JSON.parse(options.body)).not.toHaveProperty("taxCode");
  });

  it.each([400, 409])("preserves the backend update error for HTTP %s", async (status) => {
    vi.stubGlobal("fetch", vi.fn().mockResolvedValue({ ok: false, status,
      json: async () => ({ message: "Hotel metadata cannot be updated safely." }) }));
    const input = { name: "Updated Hotel", description: "", address: "123 Updated Street", publicPhone: "0901234567",
      publicEmail: "hotel@example.com", countryId: 4, provinceId: 1, wardId: 2, latitude: null, longitude: null };
    await expect(updateOwnerHotelInformation(42, input)).rejects.toMatchObject({ status, message: "Hotel metadata cannot be updated safely." });
  });
});
