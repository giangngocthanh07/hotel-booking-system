import { afterEach, describe, expect, it, vi } from "vitest";
import { getMyRegistrations } from "./ownerRegistrationService";

vi.mock("./authService", () => ({ getAuthToken: () => "owner-token" }));
afterEach(() => vi.unstubAllGlobals());

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
