import { afterEach, describe, expect, it, vi } from "vitest";
import { getVietnamProvinces } from "./locationService";

afterEach(() => vi.unstubAllGlobals());

describe("getVietnamProvinces", () => {
  it("returns province content from the location API", async () => {
    vi.stubGlobal(
      "fetch",
      vi.fn().mockResolvedValue({
        ok: true,
        json: async () => ({
          statusCode: "Success",
          message: "OK",
          content: [{ id: 1, name: "Hà Nội" }],
        }),
      }),
    );

    await expect(getVietnamProvinces()).resolves.toEqual([
      { id: 1, name: "Hà Nội" },
    ]);
  });

  it("rejects an unsuccessful API response", async () => {
    vi.stubGlobal(
      "fetch",
      vi.fn().mockResolvedValue({
        ok: true,
        json: async () => ({
          statusCode: "BadRequest",
          message: "Failed",
          content: [],
        }),
      }),
    );

    await expect(getVietnamProvinces()).rejects.toThrow("Failed");
  });
});
