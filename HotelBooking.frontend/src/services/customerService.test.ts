import { afterEach, describe, expect, it, vi } from "vitest";
import { cancelUpgradeRequest } from "./customerService";

vi.mock("./authService", () => ({ getAuthToken: () => "customer-token" }));
afterEach(() => vi.unstubAllGlobals());

describe("cancel Owner upgrade (FR-ONB-018)", () => {
  it("uses PUT and the authenticated Customer token", async () => {
    const response = { statusCode: "Success", content: true };
    const fetchMock = vi.fn().mockResolvedValue({ json: async () => response });
    vi.stubGlobal("fetch", fetchMock);
    await expect(cancelUpgradeRequest(99)).resolves.toEqual(response);
    expect(fetchMock).toHaveBeenCalledWith(expect.stringContaining("/upgrade-requests/99/cancel"), {
      method: "PUT", headers: { "Content-Type": "application/json", Authorization: "Bearer customer-token" },
    });
  });
});
