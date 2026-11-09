import { fireEvent, render, screen, within, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import * as api from "../../services/ownerRegistrationService";
import HotelRegistrationPage from "./HotelRegistrationPage";

vi.mock("../../services/ownerRegistrationService", () => ({
  getCountries: vi.fn(), getPropertyTypes: vi.fn(), getMyRegistrations: vi.fn(),
  getProvinces: vi.fn(), getWards: vi.fn(), uploadBusinessLicense: vi.fn(), submitRegistration: vi.fn(), cancelRegistration: vi.fn(),
}));

const registration = {
  requestId: 99, name: "Lake Hotel", status: "Rejected", requestedAt: "2026-11-04T00:00:00Z",
  propertyTypeName: "Hotel", address: "1 Lake Street", wardName: "Ward", provinceName: "City",
  taxCode: "1234567890",
  adminRemark: "The business license does not match the address. ".repeat(5).trim(),
  processedAt: "2026-11-04T01:00:00Z",
};

beforeEach(() => {
  vi.resetAllMocks();
  vi.mocked(api.getCountries).mockResolvedValue({ statusCode: "Success", message: "OK", content: [] });
  vi.mocked(api.getPropertyTypes).mockResolvedValue({ statusCode: "Success", message: "OK", content: [] });
  vi.mocked(api.getMyRegistrations).mockResolvedValue({ statusCode: "Success", message: "OK", content: pageOf([registration]) });
});

describe("Owner hotel-registration history (FR-ONB-013)", () => {
  it("offers authenticated license access without a provider URL", async () => {
    render(<HotelRegistrationPage />);
    expect(await screen.findByRole("button", { name: "View License" })).toBeInTheDocument();
    expect(screen.queryByRole("link", { name: "View License" })).not.toBeInTheDocument();
  });
  it("requests three records per page without a page-size selector", async () => {
    const user = userEvent.setup();
    vi.mocked(api.getMyRegistrations).mockImplementation(async (page = 1, size = 3) => ({
      statusCode: "Success", message: "OK", content: {
        items: [{ ...registration, name: page === 1 ? "Lake Hotel" : "Second page hotel" }],
        pageIndex: page, pageSize: size, totalCount: 6, totalPages: Math.ceil(6 / size),
      },
    }));
    render(<HotelRegistrationPage />);
    await screen.findByText("Lake Hotel");
    expect(api.getMyRegistrations).toHaveBeenLastCalledWith(1, 3, expect.any(AbortSignal));
    expect(screen.queryByRole("combobox", { name: "Requests per page" })).not.toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Previous" })).toBeDisabled();
    await user.click(screen.getByRole("button", { name: "Next" }));
    await screen.findByText("Second page hotel");
    expect(api.getMyRegistrations).toHaveBeenLastCalledWith(2, 3, expect.any(AbortSignal));
    expect(screen.getByRole("status")).toHaveTextContent("Page 2 of 2 · 6 requests");
    expect(screen.getByRole("button", { name: "Next" })).toBeDisabled();
    await user.click(screen.getByRole("button", { name: "Previous" }));
    await screen.findByText("Lake Hotel");
    expect(api.getMyRegistrations).toHaveBeenLastCalledWith(1, 3, expect.any(AbortSignal));
  });

  it("shows a failed page as an error and retries instead of claiming the history is empty", async () => {
    const user = userEvent.setup();
    vi.mocked(api.getMyRegistrations).mockRejectedValueOnce(new Error("History unavailable"));
    render(<HotelRegistrationPage />);
    expect(await screen.findByRole("alert")).toHaveTextContent("History unavailable");
    expect(screen.queryByText("No registration requests found.")).not.toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: "Retry history" }));
    expect(await screen.findByText("Lake Hotel")).toBeInTheDocument();
    await waitFor(() => expect(api.getMyRegistrations).toHaveBeenCalledTimes(2));
  });
  it("shows the full rejection explanation and decision time without modifying a request", async () => {
    // The Owner needs the complete explanation to correct a later submission, not a truncated preview.
    render(<HotelRegistrationPage />);
    await screen.findByText("Lake Hotel");
    const decision = screen.getByRole("group", { name: "Registration decision" });
    expect(within(decision).getByText(registration.adminRemark)).toBeInTheDocument();
    expect(decision.querySelector("time")).toHaveAttribute("dateTime", registration.processedAt);
    expect(api.getMyRegistrations).toHaveBeenCalledOnce();
    expect(api.submitRegistration).not.toHaveBeenCalled();
  });

  it("shows approval time without exposing a stale rejection note", async () => {
    mockHistory({ status: "Approved" });
    render(<HotelRegistrationPage />);
    await screen.findByText("Lake Hotel");
    const decision = screen.getByRole("group", { name: "Registration decision" });
    expect(decision.querySelector("time")).toHaveAttribute("dateTime", registration.processedAt);
    expect(screen.queryByText(registration.adminRemark)).not.toBeInTheDocument();
  });

  it.each([undefined, "invalid"])("handles an absent or invalid legacy decision timestamp: %s", async (processedAt) => {
    mockHistory({ adminRemark: "   ", processedAt });
    render(<HotelRegistrationPage />);
    await screen.findByText("Lake Hotel");
    expect(screen.getByText("No rejection reason recorded.")).toBeInTheDocument();
    expect(screen.getByText("Not recorded")).toBeInTheDocument();
    expect(screen.queryByText("Invalid Date")).not.toBeInTheDocument();
  });

  it.each(["Pending", "Cancelled"])("does not present a %s request as an Admin decision", async (status) => {
    mockHistory({ status });
    render(<HotelRegistrationPage />);
    await screen.findByText("Lake Hotel");
    expect(screen.queryByRole("group", { name: "Registration decision" })).not.toBeInTheDocument();
    expect(screen.queryByText(registration.adminRemark)).not.toBeInTheDocument();
  });

  it("preserves the empty-history state", async () => {
    vi.mocked(api.getMyRegistrations).mockResolvedValue({ statusCode: "Success", message: "OK", content: pageOf([]) });
    render(<HotelRegistrationPage />);
    expect(await screen.findByText("No registration requests found.")).toBeInTheDocument();
  });
});

afterEach(() => vi.restoreAllMocks());

describe("Owner registration cancellation (FR-ONB-018)", () => {
  it.each(["Approved", "Rejected", "Cancelled"])("does not offer cancellation for %s", async status => {
    mockHistory({ status });
    render(<HotelRegistrationPage />);
    await screen.findByText("Lake Hotel");
    expect(screen.queryByRole("button", { name: "Cancel request" })).not.toBeInTheDocument();
  });

  it("does not call the API when confirmation is declined", async () => {
    const user = userEvent.setup();
    vi.spyOn(window, "confirm").mockReturnValue(false);
    mockHistory({ status: "Pending" });
    render(<HotelRegistrationPage />);
    await user.click(await screen.findByRole("button", { name: "Cancel request" }));
    expect(window.confirm).toHaveBeenCalledWith(expect.stringContaining("Lake Hotel"));
    expect(api.cancelRegistration).not.toHaveBeenCalled();
    expect(api.getMyRegistrations).toHaveBeenCalledOnce();
  });

  it("locks cancellation while sending and refreshes the current page on success", async () => {
    const user = userEvent.setup();
    vi.spyOn(window, "confirm").mockReturnValue(true);
    mockHistory({ status: "Pending" });
    let finish!: (value: api.ApiResponse<boolean>) => void;
    vi.mocked(api.cancelRegistration).mockReturnValue(new Promise(resolve => { finish = resolve; }));
    render(<HotelRegistrationPage />);
    await user.click(await screen.findByRole("button", { name: "Cancel request" }));
    expect(screen.getByRole("button", { name: "Cancelling..." })).toBeDisabled();
    await user.click(screen.getByRole("button", { name: "Cancelling..." }));
    expect(api.cancelRegistration).toHaveBeenCalledExactlyOnceWith(99);
    mockHistory({ status: "Cancelled" });
    finish({ statusCode: "Success", message: "OK", content: true });
    expect(await screen.findByText("Cancelled", { selector: ".status-badge" })).toBeInTheDocument();
    expect(api.getMyRegistrations).toHaveBeenLastCalledWith(1, 3, expect.any(AbortSignal));
    expect(api.getMyRegistrations).toHaveBeenCalledTimes(2);
    expect(screen.queryByRole("button", { name: "Cancel request" })).not.toBeInTheDocument();
  });

  it.each([
    [{ response: { data: { message: "Request is no longer Pending." } } }, "Request is no longer Pending."],
    [new Error("Network unavailable"), "Network unavailable"],
  ])("shows cancellation errors and allows retry without hiding history", async (failure, message) => {
    const user = userEvent.setup();
    vi.spyOn(window, "confirm").mockReturnValue(true);
    mockHistory({ status: "Pending" });
    vi.mocked(api.cancelRegistration).mockRejectedValue(failure);
    render(<HotelRegistrationPage />);
    await user.click(await screen.findByRole("button", { name: "Cancel request" }));
    expect(await screen.findByRole("alert")).toHaveTextContent(message as string);
    expect(screen.getByText("Lake Hotel")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Cancel request" })).toBeEnabled();
    expect(api.getMyRegistrations).toHaveBeenCalledOnce();
  });

  it("refreshes page two and returns to a valid page when the page count shrinks", async () => {
    const user = userEvent.setup();
    vi.spyOn(window, "confirm").mockReturnValue(true);
    vi.mocked(api.getMyRegistrations).mockImplementation(async (page = 1) => ({
      statusCode: "Success", message: "OK", content: {
        ...pageOf([{ ...registration, status: "Pending" }]), pageIndex: page, totalCount: 4, totalPages: 2,
      },
    }));
    vi.mocked(api.cancelRegistration).mockResolvedValue({ statusCode: "Success", message: "OK", content: true });
    render(<HotelRegistrationPage />);
    await screen.findByText("Lake Hotel");
    await user.click(screen.getByRole("button", { name: "Next" }));
    await screen.findByText("Page 2 of 2 · 4 requests");
    vi.mocked(api.getMyRegistrations).mockResolvedValue({ statusCode: "Success", message: "OK", content: pageOf([]) });
    // A smaller positive page count exercises the existing out-of-range-page recovery.
    vi.mocked(api.getMyRegistrations).mockResolvedValueOnce({
      statusCode: "Success", message: "OK", content: { ...pageOf([]), totalCount: 3, totalPages: 1 },
    });
    await user.click(screen.getByRole("button", { name: "Cancel request" }));
    await screen.findByText("No registration requests found.");
    expect(api.getMyRegistrations).toHaveBeenNthCalledWith(3, 2, 3, expect.any(AbortSignal));
    await waitFor(() => expect(api.getMyRegistrations).toHaveBeenLastCalledWith(1, 3, expect.any(AbortSignal)));
  });
});

describe("Registration coordinates (FR-ONB-010)", () => {
  beforeEach(() => {
    vi.mocked(api.uploadBusinessLicense).mockResolvedValue({ statusCode: "Success", message: "OK", content: {
      uploaded: true, uploadId: "12345678-1234-4234-8234-123456789abc", storedFileName: "https://example.com/license.pdf",
    } });
    vi.mocked(api.submitRegistration).mockResolvedValue({ statusCode: "Success", message: "OK" });
  });

  it.each([[0, 0], [0, 106], [10, 0], [undefined, undefined]])(
    "preserves optional coordinates in the payload: %s, %s", async (latitude, longitude) => {
      const { container } = render(<HotelRegistrationPage />);
      await screen.findByText("Lake Hotel");
      const [lat, lng] = screen.getAllByRole("spinbutton");
      if (latitude !== undefined) {
        fireEvent.change(lat, { target: { value: String(latitude) } });
        expect(lat).toHaveValue(latitude);
      }
      if (longitude !== undefined) {
        fireEvent.change(lng, { target: { value: String(longitude) } });
        expect(lng).toHaveValue(longitude);
      }
      await userEvent.upload(container.querySelector<HTMLInputElement>("#file-upload")!, new File(["license"], "license.pdf", { type: "application/pdf" }));
      // Exercise the submit handler independently of unrelated required HTML fields.
      fireEvent.submit(container.querySelector("form")!);
      await waitFor(() => expect(api.submitRegistration).toHaveBeenCalledOnce());
      const payload = vi.mocked(api.submitRegistration).mock.calls[0][0];
      if (latitude === undefined) {
        expect(payload).not.toHaveProperty("latitude");
        expect(payload).not.toHaveProperty("longitude");
      } else {
        expect(payload).toMatchObject({ latitude, longitude });
      }
    },
  );

  it.each([[0, undefined], [undefined, 0], [10, undefined], [undefined, 106]])(
    "rejects an incomplete pair before uploading: %s, %s", async (latitude, longitude) => {
      const { container } = render(<HotelRegistrationPage />);
      await screen.findByText("Lake Hotel");
      const [lat, lng] = screen.getAllByRole("spinbutton");
      fireEvent.change(lat, { target: { value: latitude === undefined ? "" : String(latitude) } });
      fireEvent.change(lng, { target: { value: longitude === undefined ? "" : String(longitude) } });
      await userEvent.upload(container.querySelector<HTMLInputElement>("#file-upload")!, new File(["license"], "license.pdf", { type: "application/pdf" }));
      fireEvent.submit(container.querySelector("form")!);
      expect(await screen.findByText("If you provide Latitude, you must also provide Longitude, and vice versa.")).toBeInTheDocument();
      expect(api.uploadBusinessLicense).not.toHaveBeenCalled();
      expect(api.submitRegistration).not.toHaveBeenCalled();
    },
  );
});

describe("Registration upload ownership (FR-ONB-009/013)", () => {
  const uploadId = "12345678-1234-4234-8234-123456789abc";
  beforeEach(() => {
    vi.mocked(api.submitRegistration).mockResolvedValue({ statusCode: "Success", message: "OK" });
  });

  it("submits the receipt ID without trusting or requiring a client URL", async () => {
    vi.mocked(api.uploadBusinessLicense).mockResolvedValue({ statusCode: "Success", message: "OK", content: { uploaded: true, uploadId } });
    const { container } = render(<HotelRegistrationPage />);
    await screen.findByText("Lake Hotel");
    await userEvent.upload(container.querySelector<HTMLInputElement>("#file-upload")!, new File(["license"], "license.pdf", { type: "application/pdf" }));
    fireEvent.submit(container.querySelector("form")!);
    await waitFor(() => expect(api.submitRegistration).toHaveBeenCalledOnce());
    expect(vi.mocked(api.submitRegistration).mock.calls[0][0]).toHaveProperty("businessLicenseUploadId", uploadId);
    expect(vi.mocked(api.submitRegistration).mock.calls[0][0]).not.toHaveProperty("businessLicenseUrl");
  });

  it.each([undefined, null, "", "not-a-guid", "00000000-0000-0000-0000-000000000000"])(
    "does not fall back to a URL when the receipt is invalid: %s", async receipt => {
      vi.mocked(api.uploadBusinessLicense).mockResolvedValue({ statusCode: "Success", message: "OK",
        content: { uploaded: true, uploadId: receipt, storedFileName: "https://example.com/license.pdf" } });
      const { container } = render(<HotelRegistrationPage />);
      await screen.findByText("Lake Hotel");
      await userEvent.upload(container.querySelector<HTMLInputElement>("#file-upload")!, new File(["license"], "license.pdf", { type: "application/pdf" }));
      fireEvent.submit(container.querySelector("form")!);
      expect(await screen.findByText("Could not verify your business license upload. Please upload again.")).toBeInTheDocument();
      expect(api.submitRegistration).not.toHaveBeenCalled();
      expect(screen.getByRole("button", { name: "Submit Registration" })).toBeEnabled();
    },
  );
});

function mockHistory(overrides: { status?: string; adminRemark?: string; processedAt?: string }) {
  vi.mocked(api.getMyRegistrations).mockResolvedValue({
    statusCode: "Success", message: "OK", content: pageOf([{ ...registration, ...overrides }]),
  });
}

function pageOf(items: api.HotelRegistrationHistoryItem[]): api.RegistrationHistoryPage {
  return { items, totalCount: items.length, totalPages: items.length ? 1 : 0, pageIndex: 1, pageSize: 3 };
}
