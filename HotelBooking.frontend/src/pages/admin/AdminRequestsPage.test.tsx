import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import * as api from "../../services/adminService";
import type { HotelApprovalRequest } from "../../types/admin.types";
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
  businessLicenseUrl: "https://example.com/license.pdf", propertyTypeName: "Hotel",
  publicPhone: "0901234567", publicEmail: "hotel@example.com", provinceName: "City", wardName: "Ward", countryName: "Vietnam",
};

const counts = { total: 1, pending: 1, approved: 0, rejected: 0, cancelled: 0, today: 1, thisWeek: 1, thisMonth: 1 };

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
});

describe("Admin hotel rejection (FR-ONB-017)", () => {
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
