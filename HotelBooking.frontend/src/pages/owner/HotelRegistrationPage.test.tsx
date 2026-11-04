import { render, screen, within, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import * as api from "../../services/ownerRegistrationService";
import HotelRegistrationPage from "./HotelRegistrationPage";

vi.mock("../../services/ownerRegistrationService", () => ({
  getCountries: vi.fn(), getPropertyTypes: vi.fn(), getMyRegistrations: vi.fn(),
  getProvinces: vi.fn(), getWards: vi.fn(), uploadBusinessLicense: vi.fn(), submitRegistration: vi.fn(),
}));

const registration = {
  requestId: 99, name: "Lake Hotel", status: "Rejected", requestedAt: "2026-11-04T00:00:00Z",
  propertyTypeName: "Hotel", address: "1 Lake Street", wardName: "Ward", provinceName: "City",
  taxCode: "1234567890", businessLicenseUrl: "https://example.com/license.pdf",
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

function mockHistory(overrides: { status?: string; adminRemark?: string; processedAt?: string }) {
  vi.mocked(api.getMyRegistrations).mockResolvedValue({
    statusCode: "Success", message: "OK", content: pageOf([{ ...registration, ...overrides }]),
  });
}

function pageOf(items: api.HotelRegistrationHistoryItem[]): api.RegistrationHistoryPage {
  return { items, totalCount: items.length, totalPages: items.length ? 1 : 0, pageIndex: 1, pageSize: 3 };
}
