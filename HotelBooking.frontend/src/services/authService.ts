// authService.ts
// Tang Service: chi co nhiem vu goi API len backend
// Khong chua UI, khong chua state, chi co fetch()

import type { LoginRequest, RegisterRequest, ApiResponse, LoginResponseData, RegisterResponseData } from '../types/auth.types';

// URL goc cua BE API
// Neu BE chay tren port khac, chinh lai day
const BASE_URL = 'http://localhost:5000/api/v1';

// Ham dang nhap - goi POST /api/v1/auth/login
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

// Ham dang ky - goi POST /api/v1/auth/register
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
