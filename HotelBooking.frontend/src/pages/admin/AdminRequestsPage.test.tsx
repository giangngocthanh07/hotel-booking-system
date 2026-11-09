import { render, screen, waitFor, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import * as api from "../../services/adminService";
import type { HotelApprovalRequest, UpgradeRequest } from "../../types/admin.types";
import AdminRequestsPage from "./AdminRequestsPage";

vi.mock("../../services/adminService", () => ({
  getHotelApprovals: vi.fn(), getRequestStats: vi.fn(), rejectHotel: vi.fn(),
  getUpgradeRequests: vi.fn(), approveHotel: vi.fn(), approveUpgrade: vi.fn(), rejectUpgrade: vi.fn(),
}));

const hotel: HotelApprovalRequest = {
  requestId: 99, name: "Lake Hotel", address: "1 Lake Street", status: "Pending",
  requestedAt: "2026-11-04T00:00:00Z", typeDisplay: "Hotel", canApprove: true, canReject: true,
  requesterName: "Owner", ownerId: 7, ownerFullName: "Owner", ownerEmail: "owner@example.com",
  ownerPhoneNumber: "0901234567", ownerAddress: "Owner address", taxCode: "1234567890",
  propertyTypeName: "Hotel",
  publicPhone: "0901234567", publicEmail: "hotel@example.com", provinceName: "City", wardName: "Ward", countryName: "Vietnam",
};

const counts = { total: 1, pending: 1, approved: 0, rejected: 0, cancelled: 0, today: 1, thisWeek: 1, thisMonth: 1 };
const upgrade: UpgradeRequest = {
  requestId: 77, userId: 8, userName: "partner", fullName: "Partner User", email: "partner@example.com",
  phoneNumber: "0901234567", address: "1 Main Street", taxCode: "1234567890", status: "Pending",
  requestedAt: "2026-11-04T00:00:00Z", typeDisplay: "UpgradeOwner", canApprove: true, canReject: true, requesterName: "Partner User",
};

beforeEach(() => {
  vi.resetAllMocks();
  vi.mocked(api.getRequestStats).mockResolvedValue({
    statusCode: "Success", message: "OK",
    content: { overall: counts, hotelApproval: counts, upgradeRequest: counts, totalPending: 1, totalToday: 1 },
  });
  vi.mocked(api.getHotelApprovals).mockResolvedValue({
    statusCode: "Success", message: "OK",
    content: { items: [hotel], pageIndex: 1, pageSize: 10, totalPages: 1, totalCount: 1 },
  });
  vi.mocked(api.getUpgradeRequests).mockResolvedValue({
    statusCode: "Success", message: "OK",
    content: { items: [upgrade], pageIndex: 1, pageSize: 10, totalPages: 1, totalCount: 1 },
  });
});

describe("Admin document access (FR-ONB-014)", () => {
  it("offers authenticated document access without a provider URL", async () => {
    render(<AdminRequestsPage />);
    await userEvent.setup().click(await screen.findByText("Lake Hotel"));
    expect(screen.getByRole("button", { name: "View Document" })).toBeInTheDocument();
    expect(screen.queryByRole("link", { name: "View Document" })).not.toBeInTheDocument();
  });
});

describe("Admin upgrade rejection (FR-ONB-008)", () => {
  it("shows the saved reason in the rejected row and details", async () => {
    const user = userEvent.setup();
    const reason = "Tax documents do not match the registered business address. ".repeat(3);
    vi.mocked(api.getUpgradeRequests).mockResolvedValue({
      statusCode: "Success", message: "OK",
      content: { items: [{ ...upgrade, status: "Rejected", canApprove: false, canReject: false, rejectionReason: reason }],
        pageIndex: 1, pageSize: 10, totalPages: 1, totalCount: 1 },
    });
    render(<AdminRequestsPage />);

    await user.click(screen.getByRole("button", { name: "👤 Owner Upgrades" }));
    const row = (await screen.findByText("Partner User")).closest("tr")!;
    expect(within(row).getByText(`${reason.trim().slice(0, 100).trimEnd()}…`)).toBeInTheDocument();
    await user.click(screen.getByText("Partner User"));
    expect(screen.getByText(reason.trim())).toBeInTheDocument();
    // Reuse the decision spacing used by Hotel requests so the next section heading stays aligned.
    expect(screen.getByRole("region", { name: "Upgrade rejection details" })).toHaveClass("req-decision-details");
  });

  it("collects a reason and sends it when rejecting an owner upgrade", async () => {
    const user = userEvent.setup();
    vi.mocked(api.rejectUpgrade).mockResolvedValue({ statusCode: "Success", message: "Rejected", content: true });
    render(<AdminRequestsPage />);

    await user.click(screen.getByRole("button", { name: "👤 Owner Upgrades" }));
    await screen.findByText("Partner User");
    await user.click(screen.getByRole("button", { name: "Reject" }));
    await user.type(screen.getByRole("textbox", { name: "Rejection reason" }), "Tax document is incomplete.");
    await user.click(screen.getByRole("button", { name: "Confirm rejection" }));

    await waitFor(() => expect(api.rejectUpgrade).toHaveBeenCalledWith(77, "Tax document is incomplete."));
  });
});

describe("Admin hotel rejection (FR-ONB-017)", () => {
  // FR-ONB-014: both the list preview and detail view must explain a saved decision.
  it("shows a short rejection preview and the complete explanation in details", async () => {
    const user = userEvent.setup();
    const reason = "Business license does not match the property address. ".repeat(4);
    mockDecision({ status: "Rejected", adminRemark: reason, processedBy: 42,
      processedByName: "Review Admin", processedAt: "2026-11-04T01:00:00Z" });
    render(<AdminRequestsPage />);
    const row = (await screen.findByText("Lake Hotel")).closest("tr")!;
    expect(within(row).getByText(`${reason.trim().slice(0, 100).trimEnd()}…`)).toBeInTheDocument();
    await user.click(screen.getByText("Lake Hotel"));
    const decision = screen.getByRole("region", { name: "Decision details" });
    expect(within(decision).getByText(reason.trim())).toBeInTheDocument();
    expect(within(decision).getByText("Review Admin")).toBeInTheDocument();
    expect(decision.querySelector("time")).toHaveAttribute("dateTime", "2026-11-04T01:00:00Z");
  });

  it("shows the approval processor but does not display a stale rejection reason", async () => {
    const user = userEvent.setup();
    mockDecision({ status: "Approved", processedBy: 42, adminRemark: "Stale internal note" });
    render(<AdminRequestsPage />);
    await user.click(await screen.findByText("Lake Hotel"));
    const decision = screen.getByRole("region", { name: "Decision details" });
    expect(within(decision).getByText("Admin #42")).toBeInTheDocument();
    expect(within(decision).getByText("Not recorded")).toBeInTheDocument();
    expect(screen.queryByText("Stale internal note")).not.toBeInTheDocument();
  });

  it("handles missing legacy metadata and invalid decision dates without inventing values", async () => {
    const user = userEvent.setup();
    mockDecision({ status: "Rejected", adminRemark: "   ", processedAt: "invalid" });
    render(<AdminRequestsPage />);
    await user.click(await screen.findByText("Lake Hotel"));
    const decision = screen.getByRole("region", { name: "Decision details" });
    expect(within(decision).getByText("No rejection reason recorded.")).toBeInTheDocument();
    expect(within(decision).getAllByText("Not recorded")).toHaveLength(2);
    expect(screen.queryByText("Invalid Date")).not.toBeInTheDocument();
  });

  it.each(["Pending", "Cancelled"])("does not label a %s request as an Admin decision", async (status) => {
    const user = userEvent.setup();
    mockDecision({ status, adminRemark: "Internal note", processedByName: "Review Admin" });
    render(<AdminRequestsPage />);
    await user.click(await screen.findByText("Lake Hotel"));
    expect(screen.queryByRole("region", { name: "Decision details" })).not.toBeInTheDocument();
    expect(screen.queryByText("Internal note")).not.toBeInTheDocument();
  });

  it.each(["row", "details"])("rejects from %s and refreshes the list after success", async (entry) => {
    const user = userEvent.setup();
    vi.mocked(api.rejectHotel).mockResolvedValue({ statusCode: "Success", message: "Rejected", content: true });
    render(<AdminRequestsPage />);
    await screen.findByText("Lake Hotel");
    if (entry === "details") await user.click(screen.getByText("Lake Hotel"));
    await user.click(screen.getByRole("button", { name: entry === "row" ? "Reject" : "Reject Request" }));
    expect(api.rejectHotel).not.toHaveBeenCalled();
    await user.type(screen.getByRole("textbox", { name: "Rejection reason" }), "License is incomplete.");
    await user.click(screen.getByRole("button", { name: "Confirm rejection" }));
    await waitFor(() => expect(api.rejectHotel).toHaveBeenCalledWith(99, "License is incomplete."));
    expect(await screen.findByRole("status")).toHaveTextContent("rejected successfully");
    await waitFor(() => expect(api.getHotelApprovals).toHaveBeenCalledTimes(2));
    expect(screen.queryByRole("dialog")).not.toBeInTheDocument();
    expect(api.rejectUpgrade).not.toHaveBeenCalled();
  });

  it("shows an application failure without closing the form or refreshing the list", async () => {
    const user = userEvent.setup();
    vi.mocked(api.rejectHotel).mockResolvedValue({ statusCode: "Forbidden", message: "You cannot reject your own request.", content: false });
    render(<AdminRequestsPage />);
    await user.click(await screen.findByRole("button", { name: "Reject" }));
    await user.type(screen.getByRole("textbox", { name: "Rejection reason" }), "License is incomplete.");
    await user.click(screen.getByRole("button", { name: "Confirm rejection" }));
    expect(await screen.findByRole("alert")).toHaveTextContent("You cannot reject your own request.");
    expect(screen.getByRole("dialog")).toBeInTheDocument();
    expect(api.getHotelApprovals).toHaveBeenCalledTimes(1);
  });
});

function mockDecision(overrides: Partial<HotelApprovalRequest>) {
  vi.mocked(api.getHotelApprovals).mockResolvedValue({
    statusCode: "Success", message: "OK",
    content: { items: [{ ...hotel, canApprove: false, canReject: false, ...overrides }],
      pageIndex: 1, pageSize: 10, totalPages: 1, totalCount: 1 },
  });
}
