import { afterEach, describe, expect, it, vi } from "vitest";
import { downloadBusinessLicense } from "./businessLicenseService";

vi.mock("./authService", () => ({ getAuthToken: () => "owner-or-admin-token" }));
afterEach(() => vi.unstubAllGlobals());

describe("private business-license access (FR-ONB-013/014)", () => {
  it("uses the authenticated endpoint and returns a Blob without provider URLs", async () => {
    const blob = new Blob(["%PDF-1.7"], { type: "application/pdf" });
    const fetchMock = vi.fn().mockResolvedValue({ ok: true, blob: async () => blob });
    vi.stubGlobal("fetch", fetchMock);
    const controller = new AbortController();
    await expect(downloadBusinessLicense(42, controller.signal)).resolves.toBe(blob);
    expect(fetchMock).toHaveBeenCalledWith(expect.stringContaining("/hotel-registrations/42/business-license"), {
      headers: { Authorization: "Bearer owner-or-admin-token" }, signal: controller.signal,
      cache: "no-store", redirect: "error",
    });
    expect(fetchMock.mock.calls[0][0]).not.toContain("token");
  });

  it.each([401, 403, 404, 500])("rejects HTTP %s without opening its body as a file", async (status) => {
    const blob = vi.fn();
    vi.stubGlobal("fetch", vi.fn().mockResolvedValue({ ok: false, status, blob }));
    await expect(downloadBusinessLicense(42)).rejects.toThrow();
    expect(blob).not.toHaveBeenCalled();
  });

  it("rejects a JSON success response instead of displaying it as a document", async () => {
    vi.stubGlobal("fetch", vi.fn().mockResolvedValue({ ok: true,
      blob: async () => new Blob(["{}"], { type: "application/json" }) }));
    await expect(downloadBusinessLicense(42)).rejects.toThrow("Invalid business license response.");
  });

  it("propagates an aborted request", async () => {
    vi.stubGlobal("fetch", vi.fn().mockRejectedValue(new DOMException("Aborted", "AbortError")));
    await expect(downloadBusinessLicense(42, new AbortController().signal)).rejects.toMatchObject({ name: "AbortError" });
  });
});
