// authService.ts
// Service layer: responsible ONLY for making API calls to the backend.
// No UI, no state — just fetch().

import type { LoginRequest, RegisterRequest, ApiResponse, LoginResponseData, RegisterResponseData } from '../types/auth.types';

// Base URL of the backend API.
// Change this port if the API runs on a different port.
const BASE_URL = 'http://localhost:5000/api/v1';

// Log in a user — calls POST /api/v1/auth/login
export async function loginUser(
  request: LoginRequest
): Promise<ApiResponse<LoginResponseData>> {
  const response = await fetch(`${BASE_URL}/auth/login`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(request),
  });

  const data = await response.json();
  return data as ApiResponse<LoginResponseData>;
}

// Register a new customer — calls POST /api/v1/auth/register
export async function registerUser(
  request: RegisterRequest
): Promise<ApiResponse<RegisterResponseData>> {
  const response = await fetch(`${BASE_URL}/auth/register`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(request),
  });

  const data = await response.json();
  return data as ApiResponse<RegisterResponseData>;
}
