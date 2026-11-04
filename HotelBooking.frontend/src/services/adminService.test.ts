import { afterEach, describe, expect, it, vi } from "vitest";
import { rejectHotel } from "./adminService";

vi.mock("./authService", () => ({ getAuthToken: () => "admin-token" }));
afterEach(() => vi.unstubAllGlobals());

describe("rejectHotel (FR-ONB-017)", () => {
  it("sends the trimmed reason in an authenticated JSON request", async () => {
    const response = { statusCode: "Success", content: true, message: "Rejected" };
    const fetchMock = vi.fn().mockResolvedValue({ ok: true, json: async () => response });
    vi.stubGlobal("fetch", fetchMock);
    await expect(rejectHotel(99, "  License is incomplete.  ")).resolves.toEqual(response);
    expect(fetchMock).toHaveBeenCalledWith(expect.stringContaining("/hotel-approvals/99/reject"), {
      method: "POST",
      headers: { "Content-Type": "application/json", Authorization: "Bearer admin-token" },
      body: JSON.stringify({ reason: "License is incomplete." }),
    });
  });

  it("preserves the backend error message on an unsuccessful HTTP response", async () => {
    vi.stubGlobal("fetch", vi.fn().mockResolvedValue({
      ok: false, json: async () => ({ message: "You cannot reject your own request." }),
    }));
    await expect(rejectHotel(99, "License is incomplete.")).rejects.toThrow("You cannot reject your own request.");
  });
});
