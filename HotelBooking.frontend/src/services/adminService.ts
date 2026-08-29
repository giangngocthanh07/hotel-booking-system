import type { HotelApprovalRequest, UpgradeRequest, PagedResponse, RequestStats } from "../types/admin.types";
import type { ApiResponse } from "../types/auth.types";
import { getAuthToken } from "./authService";

const BASE_URL = "http://localhost:5083/api/v1";

function getHeaders() {
  const token = getAuthToken();
  return {
    "Content-Type": "application/json",
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
  };
}

// === REQUEST STATS ===

export async function getRequestStats(): Promise<ApiResponse<RequestStats>> {
  const response = await fetch(`${BASE_URL}/admin/requests/stats`, { headers: getHeaders() });
  return response.json();
}

// === HOTEL APPROVALS ===

export async function getHotelApprovals(pageIndex = 1, pageSize = 10, status?: string): Promise<ApiResponse<PagedResponse<HotelApprovalRequest>>> {
  let url = `${BASE_URL}/admin/hotel-approvals?pageIndex=${pageIndex}&pageSize=${pageSize}`;
  if (status) {
    url += `&status=${status}`;
  }
  const response = await fetch(url, { headers: getHeaders() });
  return response.json();
}

export async function approveHotel(id: number): Promise<ApiResponse<boolean>> {
  const response = await fetch(`${BASE_URL}/admin/hotel-approvals/${id}/approve`, {
    method: "POST",
    headers: getHeaders(),
  });
  return response.json();
}

export async function rejectHotel(id: number): Promise<ApiResponse<boolean>> {
  const response = await fetch(`${BASE_URL}/admin/hotel-approvals/${id}/reject`, {
    method: "POST",
    headers: getHeaders(),
  });
  return response.json();
}

// === UPGRADE REQUESTS ===

export async function getUpgradeRequests(pageIndex = 1, pageSize = 10, status?: string): Promise<ApiResponse<PagedResponse<UpgradeRequest>>> {
  let url = `${BASE_URL}/admin/upgrade-requests?pageIndex=${pageIndex}&pageSize=${pageSize}`;
  if (status) {
    url += `&status=${status}`;
  }
  const response = await fetch(url, { headers: getHeaders() });
  return response.json();
}

export async function approveUpgrade(id: number): Promise<ApiResponse<boolean>> {
  const response = await fetch(`${BASE_URL}/admin/upgrade-requests/${id}/approve`, {
    method: "POST",
    headers: getHeaders(),
  });
  return response.json();
}

export async function rejectUpgrade(id: number): Promise<ApiResponse<boolean>> {
  const response = await fetch(`${BASE_URL}/admin/upgrade-requests/${id}/reject`, {
    method: "POST",
    headers: getHeaders(),
  });
  return response.json();
}
