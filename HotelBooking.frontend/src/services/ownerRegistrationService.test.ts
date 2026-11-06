import { afterEach, describe, expect, it, vi } from "vitest";
import { cancelRegistration, getMyRegistrations } from "./ownerRegistrationService";

vi.mock("./authService", () => ({ getAuthToken: () => "owner-token" }));
afterEach(() => vi.unstubAllGlobals());

describe("Owner registration cancellation (FR-ONB-018)", () => {
  it("posts to the selected request using the authenticated Owner token", async () => {
    const result = { statusCode: "Success", message: "Cancelled", content: true };
    const fetchMock = vi.fn().mockResolvedValue({ ok: true, status: 200, text: async () => JSON.stringify(result) });
    vi.stubGlobal("fetch", fetchMock);
    await expect(cancelRegistration(99)).resolves.toEqual(result);
    expect(fetchMock).toHaveBeenCalledWith(expect.stringContaining("/owner/hotel-registrations/99/cancel"), {
      method: "POST", headers: expect.any(Headers),
    });
    expect((fetchMock.mock.calls[0][1].headers as Headers).get("Authorization")).toBe("Bearer owner-token");
  });

  it("preserves a failed API response for the UI", async () => {
    const error = { statusCode: "Not Found", message: "Request is no longer Pending." };
    vi.stubGlobal("fetch", vi.fn().mockResolvedValue({ ok: false, status: 404, text: async () => JSON.stringify(error) }));
    await expect(cancelRegistration(99)).rejects.toEqual({ response: { data: error } });
  });
});

describe("paged Owner registration history (FR-ONB-013)", () => {
  it("sends page parameters and cancellation to the Owner-scoped endpoint", async () => {
    const controller = new AbortController();
    const result = { statusCode: "Success", content: { items: [], totalCount: 0, totalPages: 0, pageIndex: 2, pageSize: 5 } };
    const fetchMock = vi.fn().mockResolvedValue({ ok: true, status: 200, text: async () => JSON.stringify(result) });
    vi.stubGlobal("fetch", fetchMock);
    await expect(getMyRegistrations(2, 5, controller.signal)).resolves.toEqual(result);
    expect(fetchMock).toHaveBeenCalledWith(expect.stringContaining("/owner/hotel-registrations/me/paged?pageIndex=2&pageSize=5"), {
      signal: controller.signal, headers: expect.any(Headers),
    });
    const headers = fetchMock.mock.calls[0][1].headers as Headers;
    expect(headers.get("Authorization")).toBe("Bearer owner-token");
  });
});
