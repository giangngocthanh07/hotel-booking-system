import { afterEach, describe, expect, it, vi } from "vitest";
import { getOwnerHotelMedia, uploadOwnerHotelImage } from "./ownerHotelMediaService";

vi.mock("../authService", () => ({ getAuthToken: () => "owner-token" }));
afterEach(() => vi.unstubAllGlobals());

describe("Owner images (FR-CAT-003, FR-MED-001/002/003)", () => {
  it("reads owned media with authentication and cancellation", async () => {
    const content = { hotelId: 42, coverImageUrl: null, gallery: [] };
    const fetchMock = vi.fn().mockResolvedValue({ ok: true, json: async () => ({ statusCode: "Success", content }) });
    vi.stubGlobal("fetch", fetchMock);
    const signal = new AbortController().signal;
    await expect(getOwnerHotelMedia(42, signal)).resolves.toEqual(content);
    expect(fetchMock).toHaveBeenCalledWith(expect.stringMatching(/\/42\/images$/), expect.objectContaining({ signal, headers: { Authorization: "Bearer owner-token" } }));
  });
  it.each(["cover", "gallery"] as const)("uploads %s using browser multipart boundaries", async kind => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true, json: async () => ({ statusCode: "Success", content: { hotelId: 42 } }) });
    vi.stubGlobal("fetch", fetchMock);
    const file = new File(["image"], "hotel.jpg", { type: "image/jpeg" });
    await uploadOwnerHotelImage(42, kind, file);
    const [url, options] = fetchMock.mock.calls[0];
    expect(url).toMatch(new RegExp(`/42/images/${kind}$`));
    expect(options.method).toBe("POST");
    expect(options.body.get("file")).toBe(file);
    expect(options.headers).toEqual({ Authorization: "Bearer owner-token" });
  });
  it("preserves safe validation/conflict errors", async () => {
    vi.stubGlobal("fetch", vi.fn().mockResolvedValue({ ok: false, status: 409, json: async () => ({ message: "Four additional images are required." }) }));
    await expect(getOwnerHotelMedia(42)).rejects.toMatchObject({ status: 409, message: "Four additional images are required." });
  });
  it("rejects invalid IDs before network access", async () => {
    const fetchMock = vi.fn(); vi.stubGlobal("fetch", fetchMock);
    await expect(getOwnerHotelMedia(0)).rejects.toThrow("Invalid Hotel ID.");
    expect(fetchMock).not.toHaveBeenCalled();
  });
  it.each([401, 403, 404, 500])("handles HTTP %s with no JSON body", async status => {
    vi.stubGlobal("fetch", vi.fn().mockResolvedValue({ ok: false, status, json: async () => { throw new Error("No body"); } }));
    await expect(getOwnerHotelMedia(42)).rejects.toMatchObject({ status });
  });
});
