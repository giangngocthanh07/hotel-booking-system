// authService.ts
// Service layer: responsible ONLY for making API calls to the backend.
// No UI, no state — just fetch().

import type {
  LoginRequest,
  RegisterRequest,
  ApiResponse,
  LoginResponseData,
  RegisterResponseData,
} from "../types/auth.types";

// Base URL of the backend API.
// Matches HotelBooking.api/Properties/launchSettings.json ("http" profile, applicationUrl).
const BASE_URL = "http://localhost:5083/api/v1";

// Log in a user — calls POST /api/v1/auth/login
export async function loginUser(
  request: LoginRequest,
): Promise<ApiResponse<LoginResponseData>> {
  const response = await fetch(`${BASE_URL}/auth/login`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(request),
  });

  const data = await response.json();
  return data as ApiResponse<LoginResponseData>;
}

// Register a new customer — calls POST /api/v1/auth/register
export async function registerUser(
  request: RegisterRequest,
): Promise<ApiResponse<RegisterResponseData>> {
  const response = await fetch(`${BASE_URL}/auth/register`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(request),
  });

  const data = await response.json();
  return data as ApiResponse<RegisterResponseData>;
}

// --- SESSION HELPERS (localStorage read/write, no API calls) ---

// Backend returns statusCode: "Success" on success (see StatusCodeResponse.cs) — anything else is a failure.
export function isApiSuccess(response: ApiResponse<unknown>): boolean {
  return response.statusCode === "Success";
}

// Fired whenever login/logout changes the stored session, so the Header can react in the same tab.
export const AUTH_CHANGED_EVENT = "auth-changed";

export function getStoredFullName(): string | null {
  return localStorage.getItem("fullName");
}

export function getStoredRoles(): string[] {
  const roles = localStorage.getItem("roles");
  if (!roles) return [];
  return roles.split(",");
}

export function getAuthToken(): string | null {
  return localStorage.getItem("accessToken");
}

export function isLoggedIn(): boolean {
  return localStorage.getItem("accessToken") !== null;
}

// Clears the session and notifies listeners (e.g. Header) in the current tab.
export function logout(): void {
  localStorage.removeItem("accessToken");
  localStorage.removeItem("fullName");
  localStorage.removeItem("roles");
  window.dispatchEvent(new Event(AUTH_CHANGED_EVENT));
}

export interface UserDetail {
  id: number;
  userName: string;
  email: string;
  phoneNumber: string;
  fullName: string;
  address: string | null;
  dateOfBirth: string | null;
  avatarUrl: string | null;
  roles: string[];
}

export interface UpdateUserProfileRequest {
  fullName: string;
  phoneNumber?: string;
  dateOfBirth?: string;
}

export async function getCurrentUser(): Promise<ApiResponse<UserDetail>> {
  const token = getAuthToken();
  const res = await fetch(${BASE_URL}/auth/me, {
    headers: {
      "Content-Type": "application/json",
      ...(token ? { Authorization: Bearer  } : {})
    }
  });
  return res.json();
}

export async function updateUserProfile(data: UpdateUserProfileRequest): Promise<ApiResponse<any>> {
  const token = getAuthToken();
  const res = await fetch(${BASE_URL}/auth/profile, {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
      ...(token ? { Authorization: Bearer  } : {})
    },
    body: JSON.stringify(data)
  });
  return res.json();
}
