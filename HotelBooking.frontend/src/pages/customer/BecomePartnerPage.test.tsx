import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router-dom";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import * as api from "../../services/customerService";
import BecomePartnerPage from "./BecomePartnerPage";

vi.mock("../../services/customerService", () => ({
  getMyUpgradeInfo: vi.fn(), createUpgradeRequest: vi.fn(), cancelUpgradeRequest: vi.fn(),
}));
vi.mock("../../services/authService", () => ({ isLoggedIn: () => true, getStoredRoles: () => ["Customer"] }));
beforeEach(() => {
  vi.resetAllMocks();
  vi.mocked(api.getMyUpgradeInfo).mockResolvedValue({ statusCode: "Success", message: "OK", content: {
    userId: 7, userName: "customer", fullName: "Customer", email: "", phoneNumber: "", requestId: 99, requestStatus: "Pending",
  } });
});
afterEach(() => vi.restoreAllMocks());

describe("Customer cancels Owner upgrade (FR-ONB-018)", () => {
  it.each(["Approved", "Rejected", "Cancelled"])("does not offer cancellation for %s", async requestStatus => {
    const info = await api.getMyUpgradeInfo();
    vi.mocked(api.getMyUpgradeInfo).mockResolvedValue({ ...info, content: { ...info.content!, requestStatus } });
    render(<MemoryRouter><BecomePartnerPage /></MemoryRouter>);
    await screen.findByRole("heading", { name: "Become a Partner" });
    expect(screen.queryByRole("button", { name: "Cancel request" })).not.toBeInTheDocument();
  });

  it("can cancel a newly submitted request using its returned ID", async () => {
    const info = await api.getMyUpgradeInfo();
    vi.mocked(api.getMyUpgradeInfo).mockResolvedValue({ ...info, content: { ...info.content!, requestStatus: "None", requestId: null } });
    vi.mocked(api.createUpgradeRequest).mockResolvedValue({ statusCode: "Success", message: "OK", content: { requestId: 123 } });
    vi.mocked(api.cancelUpgradeRequest).mockResolvedValue({ statusCode: "Success", message: "OK", content: true });
    vi.spyOn(window, "confirm").mockReturnValue(true);
    render(<MemoryRouter><BecomePartnerPage /></MemoryRouter>);
    await userEvent.type(await screen.findByLabelText(/Business Address/), "123 Business Street");
    await userEvent.type(screen.getByLabelText(/Tax Code/), "1234567890");
    await userEvent.click(screen.getByRole("button", { name: "Submit Request" }));
    await userEvent.click(await screen.findByRole("button", { name: "Cancel request" }));
    expect(api.cancelUpgradeRequest).toHaveBeenCalledExactlyOnceWith(123);
  });

  it("requires confirmation", async () => {
    vi.spyOn(window, "confirm").mockReturnValue(false);
    render(<MemoryRouter><BecomePartnerPage /></MemoryRouter>);
    await userEvent.click(await screen.findByRole("button", { name: "Cancel request" }));
    expect(api.cancelUpgradeRequest).not.toHaveBeenCalled();
  });

  it("locks repeated clicks and restores the submission form after cancellation", async () => {
    vi.spyOn(window, "confirm").mockReturnValue(true);
    let finish!: (response: Awaited<ReturnType<typeof api.cancelUpgradeRequest>>) => void;
    vi.mocked(api.cancelUpgradeRequest).mockReturnValue(new Promise(resolve => { finish = resolve; }));
    render(<MemoryRouter><BecomePartnerPage /></MemoryRouter>);
    await userEvent.click(await screen.findByRole("button", { name: "Cancel request" }));
    expect(screen.getByRole("button", { name: "Cancelling..." })).toBeDisabled();
    expect(api.cancelUpgradeRequest).toHaveBeenCalledExactlyOnceWith(99);
    finish({ statusCode: "Success", message: "OK", content: true });
    expect(await screen.findByRole("button", { name: "Submit Request" })).toBeInTheDocument();
    expect(screen.queryByRole("button", { name: "Cancel request" })).not.toBeInTheDocument();
  });

  it("shows a rejected cancellation and retains Pending status for retry", async () => {
    vi.spyOn(window, "confirm").mockReturnValue(true);
    vi.mocked(api.cancelUpgradeRequest).mockResolvedValue({ statusCode: "Not Found", message: "Request is no longer Pending.", content: false });
    render(<MemoryRouter><BecomePartnerPage /></MemoryRouter>);
    await userEvent.click(await screen.findByRole("button", { name: "Cancel request" }));
    expect(await screen.findByRole("alert")).toHaveTextContent("Request is no longer Pending.");
    expect(screen.getByRole("button", { name: "Cancel request" })).toBeEnabled();
  });
});
