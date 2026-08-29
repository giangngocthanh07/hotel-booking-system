import { getAuthToken } from "./authService";
import type { ApiResponse } from "../types/auth.types";

const BASE_URL = "http://localhost:5083/api/v1";

function getHeaders() {
  const token = getAuthToken();
  return {
    "Content-Type": "application/json",
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
  };
}

export interface CreateUpgradeRequest {
  address: string;
  taxCode: string;
}

export async function createUpgradeRequest(request: CreateUpgradeRequest): Promise<ApiResponse<any>> {
  const response = await fetch(`${BASE_URL}/upgrade-requests`, {
    method: "POST",
    headers: getHeaders(),
    body: JSON.stringify(request)
  });
  return response.json();
}
