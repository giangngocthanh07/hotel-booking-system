import { act, fireEvent, render, screen, waitFor } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { downloadBusinessLicense } from "../../services/businessLicenseService";
import BusinessLicenseButton from "./BusinessLicenseButton";

vi.mock("../../services/businessLicenseService", () => ({ downloadBusinessLicense: vi.fn() }));
const tab = { opener: {}, closed: false, close: vi.fn(), focus: vi.fn(), location: { replace: vi.fn() } };
beforeEach(() => {
  vi.resetAllMocks();
  tab.closed = false;
  vi.spyOn(window, "open").mockReturnValue(tab as unknown as Window);
  vi.stubGlobal("URL", { createObjectURL: vi.fn(() => "blob:private-license"), revokeObjectURL: vi.fn() });
});
afterEach(() => { vi.useRealTimers(); vi.restoreAllMocks(); vi.unstubAllGlobals(); });

describe("private document button (FR-ONB-013/014)", () => {
  it("opens only the authorized Blob and revokes its URL on unmount", async () => {
    vi.mocked(downloadBusinessLicense).mockResolvedValue(new Blob(["%PDF-1.7"], { type: "application/pdf" }));
    const { unmount } = render(<BusinessLicenseButton requestId={42} />);
    fireEvent.click(screen.getByRole("button", { name: "View License" }));
    await waitFor(() => expect(tab.location.replace).toHaveBeenCalledWith("blob:private-license"));
    expect(downloadBusinessLicense).toHaveBeenCalledWith(42, expect.any(AbortSignal));
    expect(tab.opener).toBeNull();
    fireEvent.click(screen.getByRole("button", { name: "View License" }));
    expect(tab.focus).toHaveBeenCalled();
    expect(downloadBusinessLicense).toHaveBeenCalledTimes(1);
    unmount();
    expect(URL.revokeObjectURL).toHaveBeenCalledWith("blob:private-license");
  });

  it("disables duplicate clicks while downloading", async () => {
    vi.mocked(downloadBusinessLicense).mockImplementation(() => new Promise(() => {}));
    render(<BusinessLicenseButton requestId={42} />);
    fireEvent.click(screen.getByRole("button", { name: "View License" }));
    expect(screen.getByRole("button", { name: "Loading license…" })).toBeDisabled();
    expect(downloadBusinessLicense).toHaveBeenCalledTimes(1);
  });

  it("closes the placeholder and shows unavailable errors", async () => {
    vi.mocked(downloadBusinessLicense).mockRejectedValue(new Error("Business license is unavailable."));
    render(<BusinessLicenseButton requestId={42} />);
    fireEvent.click(screen.getByRole("button", { name: "View License" }));
    expect(await screen.findByRole("alert")).toHaveTextContent("Business license is unavailable.");
    expect(tab.close).toHaveBeenCalled();
    expect(URL.createObjectURL).not.toHaveBeenCalled();
  });

  it("does not fetch when the browser blocks the new tab", async () => {
    vi.mocked(window.open).mockReturnValue(null);
    render(<BusinessLicenseButton requestId={42} />);
    fireEvent.click(screen.getByRole("button", { name: "View License" }));
    expect(await screen.findByRole("alert")).toHaveTextContent("Allow pop-ups");
    expect(downloadBusinessLicense).not.toHaveBeenCalled();
  });

  it("aborts a pending download and closes its placeholder on unmount", () => {
    vi.mocked(downloadBusinessLicense).mockImplementation(() => new Promise(() => {}));
    const { unmount } = render(<BusinessLicenseButton requestId={42} />);
    fireEvent.click(screen.getByRole("button", { name: "View License" }));
    const signal = vi.mocked(downloadBusinessLicense).mock.calls[0][1];
    unmount();
    expect(signal?.aborted).toBe(true);
    expect(tab.close).toHaveBeenCalled();
    expect(URL.createObjectURL).not.toHaveBeenCalled();
  });

  it("resets pending state when switching to a different request", () => {
    vi.mocked(downloadBusinessLicense).mockImplementation(() => new Promise(() => {}));
    const { rerender } = render(<BusinessLicenseButton requestId={42} />);
    fireEvent.click(screen.getByRole("button", { name: "View License" }));
    const oldSignal = vi.mocked(downloadBusinessLicense).mock.calls[0][1];
    rerender(<BusinessLicenseButton requestId={43} />);
    expect(oldSignal?.aborted).toBe(true);
    fireEvent.click(screen.getByRole("button", { name: "View License" }));
    expect(downloadBusinessLicense).toHaveBeenLastCalledWith(43, expect.any(AbortSignal));
  });

  it("revokes the temporary URL when the document tab is closed", async () => {
    vi.useFakeTimers();
    vi.mocked(downloadBusinessLicense).mockResolvedValue(new Blob(["%PDF-1.7"], { type: "application/pdf" }));
    render(<BusinessLicenseButton requestId={42} />);
    await act(async () => { fireEvent.click(screen.getByRole("button", { name: "View License" })); });
    tab.closed = true;
    act(() => { vi.advanceTimersByTime(1000); });
    expect(URL.revokeObjectURL).toHaveBeenCalledWith("blob:private-license");
  });
});
